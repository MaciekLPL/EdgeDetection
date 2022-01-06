using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media.Imaging;

namespace EdgeDetection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            timer = new Stopwatch();
            threads = Environment.ProcessorCount;
            sliderThreads.Value = threads;
        }

        private readonly Stopwatch timer;
        private Bitmap inputBitmap;
        private Bitmap resultBitmap;
        private int threads;

        private void btnSelectImage_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".bmp";
            ofd.Filter = "BMP Files (*.bmp)|*.bmp";

            if (ofd.ShowDialog() == true) {
                string filename = ofd.FileName;
                inputBitmap = new Bitmap(filename);
                imgSelected.Source = ImgProcessing.BitmapToImage(inputBitmap);
                imgFilter.Source = new BitmapImage(new Uri(@"\Resources\no-image.png", UriKind.Relative));


            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) {

            if (inputBitmap != null) {

                if (radioCS.IsChecked == true) {

                    timer.Restart();
                    resultBitmap = ImgProcessing.EdgeDetection(inputBitmap, (int)sliderThreads.Value);
                    timer.Stop();

                } else {

                    timer.Restart();
                    resultBitmap = ImgProcessing.EdgeDetectionAsm(inputBitmap, (int)sliderThreads.Value);
                    timer.Stop();

                }

                imgFilter.Source = ImgProcessing.BitmapToImage(resultBitmap);
                textblockTimer.Visibility = Visibility.Visible;
                textblockTimer.Text = $"Timer: {timer.ElapsedMilliseconds}ms";
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e) {
            if (resultBitmap != null) {

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".bmp";
                sfd.Filter = "BMP Files (*.bmp)|*.bmp";

                if (sfd.ShowDialog() == true) {
                    resultBitmap.Save(sfd.FileName, ImageFormat.Bmp);
                }
            }
        }
    }
}
