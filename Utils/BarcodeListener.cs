using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Input;

namespace PosApp.Utils
{
    public class BarcodeListener : IDisposable
    {
        // 2. KURAL: İşletim sistemi seviyesinde Düşük Seviyeli Klavye Kancası
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;

        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private LowLevelKeyboardProc _proc;
        private IntPtr _hookID = IntPtr.Zero;

        private StringBuilder _barcodeBuffer = new StringBuilder();
        private DateTime _lastKeystroke = DateTime.Now;

        // Barkod okunduğunda tetiklenecek event (Arayüze iletmek için)
        public event EventHandler<string> BarcodeScanned;

        public BarcodeListener()
        {
            _proc = HookCallback;
            _hookID = SetHook(_proc);
        }

        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                TimeSpan timeSinceLastKey = DateTime.Now - _lastKeystroke;
                _lastKeystroke = DateTime.Now;

                // 2. KURAL: 50ms algoritması (İnsan yazımı ile barkod cihazını ayırır)
                if (timeSinceLastKey.TotalMilliseconds > 50)
                {
                    // Eğer tuş vuruşları arası süre 50ms'den uzunsa bu barkod cihazı olamaz, bufferı temizle
                    _barcodeBuffer.Clear();
                }

                if (key == Key.Enter)
                {
                    if (_barcodeBuffer.Length > 0)
                    {
                        string barcode = _barcodeBuffer.ToString();
                        _barcodeBuffer.Clear();
                        
                        // Buffer'daki barkodu fırlat
                        BarcodeScanned?.Invoke(this, barcode);
                    }
                }
                else
                {
                    char c = GetCharFromKey(key);
                    if (c != '\0')
                    {
                        _barcodeBuffer.Append(c);
                    }
                }
            }

            // Diğer uygulamaların tuş vuruşunu algılamaya devam etmesi için kancayı devret
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        private char GetCharFromKey(Key key)
        {
            // Temel tuş çevirimi. Sadece Rakam ve Harfleri dikkate alıyoruz.
            if (key >= Key.D0 && key <= Key.D9) return (char)('0' + (key - Key.D0));
            if (key >= Key.NumPad0 && key <= Key.NumPad9) return (char)('0' + (key - Key.NumPad0));
            if (key >= Key.A && key <= Key.Z) return key.ToString()[0];
            
            return '\0';
        }

        public void Dispose()
        {
            UnhookWindowsHookEx(_hookID);
        }

        // --- Gerekli User32 ve Kernel32 DLL Importları ---
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
