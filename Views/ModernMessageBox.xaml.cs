using System.Windows;
using System.Windows.Media;

namespace PosApp.Views
{
    public partial class ModernMessageBox : Window
    {
        public MessageBoxResult Result { get; private set; } = MessageBoxResult.None;

        private ModernMessageBox(string message, string title, MessageBoxButton button, MessageBoxImage image)
        {
            InitializeComponent();

            if (Application.Current != null)
            {
                var activeWindow = System.Linq.Enumerable.FirstOrDefault(System.Linq.Enumerable.OfType<Window>(Application.Current.Windows), x => x.IsActive);
                if (activeWindow != null && activeWindow != this)
                {
                    this.Owner = activeWindow;
                }
                else if (Application.Current.MainWindow != null && Application.Current.MainWindow != this)
                {
                    this.Owner = Application.Current.MainWindow;
                }
            }

            TxtMessage.Text = message;
            TxtTitle.Text = title;
            
            // Set Icon and Color based on Image
            switch (image)
            {
                case MessageBoxImage.Error:
                    TxtIcon.Text = "❌";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#BA1A1A"));
                    break;
                case MessageBoxImage.Warning:
                    TxtIcon.Text = "⚠️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B3261E"));
                    break;
                case MessageBoxImage.Information:
                    TxtIcon.Text = "ℹ️";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004B73"));
                    break;
                case MessageBoxImage.Question:
                    TxtIcon.Text = "❓";
                    TxtTitle.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#004B73"));
                    break;
                default:
                    TxtIcon.Text = "💬";
                    break;
            }

            // Set Buttons
            switch (button)
            {
                case MessageBoxButton.OK:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnYes.Visibility = Visibility.Collapsed;
                    BtnCancel.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.OKCancel:
                    BtnOk.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnYes.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNo:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnCancel.Content = "Hayır";
                    BtnOk.Visibility = Visibility.Collapsed;
                    break;
                case MessageBoxButton.YesNoCancel:
                    BtnYes.Visibility = Visibility.Visible;
                    BtnCancel.Visibility = Visibility.Visible;
                    BtnOk.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.OK;
            DialogResult = true;
            Close();
        }

        private void BtnYes_Click(object sender, RoutedEventArgs e)
        {
            Result = MessageBoxResult.Yes;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if ((string)BtnCancel.Content == "Hayır")
                Result = MessageBoxResult.No;
            else
                Result = MessageBoxResult.Cancel;
                
            DialogResult = false;
            Close();
        }

        public static MessageBoxResult Show(string messageBoxText, string caption = "Mesaj", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
        {
            var msgBox = new ModernMessageBox(messageBoxText, caption, button, icon);
            
            if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsVisible)
            {
                msgBox.Owner = Application.Current.MainWindow;
            }
            
            msgBox.ShowDialog();
            return msgBox.Result;
        }
    }
}
