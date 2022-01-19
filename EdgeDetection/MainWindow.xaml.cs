/* ******************************************
 * Temat: Wykrywanie krawędzi - Operator Sobela
 * Autor: Maciej Lejczak, Informatyka Katowice, semestr 5, grupa 2
 * Prowadządzy: mgr inż. Krzysztof Hanzel
 * Rok akademicki: 2021/2022
 * ******************************************/

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
            timer = new Stopwatch();                    //create Timer
            threads = Environment.ProcessorCount;       //load processor count
            sliderThreads.Value = threads;              //set slider to processor count value
        }

        private readonly Stopwatch timer;
        private Bitmap inputBitmap;
        private Bitmap resultBitmap;                    
        private int threads;
        private string filename;

        /*
         * Open OpenFileDialog, let user select input image, if selected - load into input Image Control
         */
        private void btnSelectImage_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".jpeg";
            ofd.Filter = "JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp";

            if (ofd.ShowDialog() == true) {

                filename = ofd.FileName;
                inputBitmap = new Bitmap(filename);
                imgSelected.Source = ImgProcessing.BitmapToImage(inputBitmap);
                imgFilter.Source = new BitmapImage(new Uri(@"\Resources\no-image.png", UriKind.Relative));
            }
        }

        /*
         * Start execution - check if input is valid, start timer, call ImgProcessing.EdgeDetection
         * After execution - stop timer, print execution time
         */
        private void BtnStart_Click(object sender, RoutedEventArgs e) {

            if (inputBitmap != null && inputBitmap.Width > 2 && inputBitmap.Height > 2) {

                timer.Restart();
                resultBitmap = ImgProcessing.EdgeDetection(inputBitmap, (int)sliderThreads.Value, (bool) radioCS.IsChecked);
                timer.Stop();

                imgFilter.Source = ImgProcessing.BitmapToImage(resultBitmap);
                textblockTimer.Visibility = Visibility.Visible;
                textblockTimer.Text = $"Timer: {timer.ElapsedMilliseconds}ms";

            } else {
                MessageBox.Show("The image could not be processed. Check that it has minimum dimensions of 3x3", "Edge Detection - Image error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /*
         * Save image - check if there is result image, let user save the file using SaveFileDialog.
         */
        private void BtnSave_Click(object sender, RoutedEventArgs e) {

            if (resultBitmap != null) {

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.DefaultExt = ".jpeg";
                sfd.Filter = "JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp";

                if (sfd.ShowDialog() == true) {

                    if(sfd.FileName != filename)
                        resultBitmap.Save(sfd.FileName, ImageFormat.Jpeg);
                    else
                        MessageBox.Show("You cannot overwrite file you are using!\nTry saving file with different name.", "Edge Detection - Save error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
