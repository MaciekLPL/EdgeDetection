using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace EdgeDetection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            timer = new Stopwatch();
        }

        private readonly Stopwatch timer;
        private Bitmap inputBitmap;

        private void btnSelectImage_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".bmp";
            dialog.Filter = "BMP Files (*.bmp)|*.bmp";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true) {
                string filename = dialog.FileName;
                inputBitmap = new Bitmap(filename);
                imgSelected.Source = ImgProcess.BitmapToImage(inputBitmap);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) {

            if(inputBitmap != null) {
                timer.Restart();
                Bitmap res = ImgProcess.EdgeDetection(inputBitmap, (int)sliderThreads.Value);
                timer.Stop();

                imgFilter.Source = ImgProcess.BitmapToImage(res);
                TimeSpan time = timer.Elapsed;
                textblockTimer.Visibility = Visibility.Visible;
                textblockTimer.Text = $"Timer: {time.Minutes}m {time.Seconds}s {time.Milliseconds}ms";
            }
        }
    }
}
