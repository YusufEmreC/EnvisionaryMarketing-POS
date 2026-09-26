using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Printing;

namespace PosApp.Services
{
    public class PrinterManager
    {
        // Standart ESC/POS komutları
        private readonly byte[] INIT_PRINTER = new byte[] { 0x1B, 0x40 }; // Yazıcıyı başlat
        private readonly byte[] CUT_PAPER = new byte[] { 0x1D, 0x56, 0x41, 0x10 }; // Kağıdı kes
        
        // 3. KURAL: Para çekmecesi fırlatma komutu
        private readonly byte[] OPEN_DRAWER = new byte[] { 0x1B, 0x70, 0x00, 0xFF, 0xFF }; 

        private readonly string printerName;

        public PrinterManager(string printerName)
        {
            this.printerName = printerName;
        }

        public void CheckPrinterStatus()
        {
            try
            {
                // PrintQueue status require System.Printing from ReachFramework
                using (var server = new LocalPrintServer())
                {
                    var queue = server.GetPrintQueue(printerName);
                    queue.Refresh();

                    if (queue.IsOffline)
                        throw new Exception("Yazıcı çevrimdışı (Offline). Lütfen bağlantısını kontrol edin.");
                    
                    if (queue.IsOutOfPaper)
                        throw new Exception("Yazıcıda kâğıt bitti! Lütfen kâğıt rulosunu yenileyin.");
                    
                    if (queue.HasPaperProblem)
                        throw new Exception("Yazıcıda kâğıt sıkışması veya kapak açık hatası var.");
                        
                    if (queue.QueueStatus.HasFlag(PrintQueueStatus.Error))
                        throw new Exception("Yazıcı genel bir hata durumunda.");
                }
            }
            catch (PrintQueueException)
            {
                throw new Exception($"'{printerName}' isimli yazıcı sistemde bulunamadı. Lütfen ayarları kontrol edin.");
            }
        }

        public void KickCashDrawer()
        {
            CheckPrinterStatus();
            SendBytesToPrinter(printerName, OPEN_DRAWER);
        }

        public void PrintReceipt(string receiptText)
        {
            CheckPrinterStatus();
            
            // Windows-1254 (Türkçe) veya PC857 CodePage kullanarak karakter sorununu engelle
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var encoding = Encoding.GetEncoding(857); 
            
            byte[] textBytes = encoding.GetBytes(receiptText + "\n\n\n");

            // Başlatma + Metin + Kesme komutlarını birleştir
            int totalLength = INIT_PRINTER.Length + textBytes.Length + CUT_PAPER.Length;
            byte[] printData = new byte[totalLength];
            
            Buffer.BlockCopy(INIT_PRINTER, 0, printData, 0, INIT_PRINTER.Length);
            Buffer.BlockCopy(textBytes, 0, printData, INIT_PRINTER.Length, textBytes.Length);
            Buffer.BlockCopy(CUT_PAPER, 0, printData, INIT_PRINTER.Length + textBytes.Length, CUT_PAPER.Length);

            SendBytesToPrinter(printerName, printData);
        }

        // --- 3. KURAL: Raw (Ham) Yazdırma İşlemi (Windows Spooler kuyruğu by-pass edilir) ---
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        private class DOCINFOA
        {
            [MarshalAs(UnmanagedType.LPStr)] public string pDocName;
            [MarshalAs(UnmanagedType.LPStr)] public string pOutputFile;
            [MarshalAs(UnmanagedType.LPStr)] public string pDataType;
        }

        [DllImport("winspool.Drv", EntryPoint = "OpenPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool OpenPrinter([MarshalAs(UnmanagedType.LPStr)] string szPrinter, out IntPtr hPrinter, IntPtr pd);

        [DllImport("winspool.Drv", EntryPoint = "ClosePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool ClosePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartDocPrinterA", SetLastError = true, CharSet = CharSet.Ansi, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartDocPrinter(IntPtr hPrinter, Int32 level, [In, MarshalAs(UnmanagedType.LPStruct)] DOCINFOA di);

        [DllImport("winspool.Drv", EntryPoint = "EndDocPrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndDocPrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "StartPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool StartPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "EndPagePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool EndPagePrinter(IntPtr hPrinter);

        [DllImport("winspool.Drv", EntryPoint = "WritePrinter", SetLastError = true, ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
        private static extern bool WritePrinter(IntPtr hPrinter, IntPtr pBytes, Int32 dwCount, out Int32 dwWritten);

        private bool SendBytesToPrinter(string szPrinterName, byte[] data)
        {
            IntPtr pBytes = Marshal.AllocCoTaskMem(data.Length);
            Marshal.Copy(data, 0, pBytes, data.Length);

            bool success = false;
            IntPtr hPrinter = IntPtr.Zero;
            DOCINFOA di = new DOCINFOA();
            di.pDocName = "RAW POS Receipt";
            di.pDataType = "RAW"; // Spooler kuyruğu formatlamasını es geçer

            try
            {
                if (OpenPrinter(szPrinterName.Normalize(), out hPrinter, IntPtr.Zero))
                {
                    if (StartDocPrinter(hPrinter, 1, di))
                    {
                        if (StartPagePrinter(hPrinter))
                        {
                            Int32 dwWritten = 0;
                            success = WritePrinter(hPrinter, pBytes, data.Length, out dwWritten);
                            EndPagePrinter(hPrinter);
                        }
                        EndDocPrinter(hPrinter);
                    }
                    ClosePrinter(hPrinter);
                }
            }
            finally
            {
                Marshal.FreeCoTaskMem(pBytes);
            }
            return success;
        }
    }
}
