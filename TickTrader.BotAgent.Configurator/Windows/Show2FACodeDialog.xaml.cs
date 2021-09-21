using QRCoder;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace TickTrader.BotAgent.Configurator
{
    /// <summary>
    /// Interaction logic for Show2FACodeDialog.xaml
    /// </summary>
    public partial class Show2FACodeDialog : Window
    {
        public Show2FACodeDialog(string otpSecret, string login)
        {
            InitializeComponent();

            Secret.Text = otpSecret;

            var qrPayload = new PayloadGenerator.OneTimePassword
            {
                Secret = otpSecret,
                Issuer = "AlgoServer",
                Label = login,
            };
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(qrPayload, QRCodeGenerator.ECCLevel.Q);
            var bitmap = new QRCode(qrCodeData).GetGraphic(20);

            using (var buffer = new MemoryStream())
            {
                bitmap.Save(buffer, ImageFormat.Png);
                buffer.Position = 0;

                var bitmapImg = new BitmapImage();
                bitmapImg.BeginInit();
                bitmapImg.StreamSource = buffer;
                bitmapImg.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImg.EndInit();
                bitmapImg.Freeze();

                QrCode.Source = bitmapImg;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
