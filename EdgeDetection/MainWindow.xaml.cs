﻿using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using CSharp;

namespace EdgeDetection {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public unsafe partial class MainWindow : Window {
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
        private string filename;

        [DllImport(@"C:\Users\Maciek\source\repos\EdgeDetection\x64\Release\Asm.dll")]
        static extern void mainSobel(byte* input, byte* output, int rows, int cols);


        private void btnSelectImage_Click(object sender, RoutedEventArgs e) {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".jpeg";
            ofd.Filter = "JPEG Image|*.jpg;*.jpeg|Bitmap Image|*.bmp";

            if (ofd.ShowDialog() == true) {

                filename = ofd.FileName;
                inputBitmap = new Bitmap(filename);
                imgSelected.Source = BitmapToImage(inputBitmap);
                imgFilter.Source = new BitmapImage(new Uri(@"\Resources\no-image.png", UriKind.Relative));
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) {

            if (inputBitmap != null && inputBitmap.Width > 2 && inputBitmap.Height > 2) {

                timer.Restart();
                resultBitmap = EdgeDetection(inputBitmap, (int)sliderThreads.Value, (bool) radioCS.IsChecked);
                timer.Stop();

                imgFilter.Source = BitmapToImage(resultBitmap);
                textblockTimer.Visibility = Visibility.Visible;
                textblockTimer.Text = $"Timer: {timer.ElapsedMilliseconds}ms";

            } else {
                MessageBox.Show("The image could not be processed. Check that it has minimum dimensions of 3x3", "Edge Detection - Image error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

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


        public static Bitmap EdgeDetection(Bitmap inputBmp, int threads, bool cs) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptrOriginal = (byte*)inputBmpData.Scan0.ToPointer();

            Bitmap resultBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptrResult = (byte*)resultBmpData.Scan0.ToPointer();

            int stride = inputBmpData.Stride;

            if (cs)
                ImgProcessing.CSharpSobel(ptrOriginal, ptrResult, width, height, stride, threads);
            else 
                Asm(ptrOriginal, ptrResult, width, height, threads);


            inputBmp.UnlockBits(inputBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }


        private static void Asm(byte* ptrOriginal, byte* ptrResult, int width, int height, int threads) {

            _ = Parallel.For(1, height - 1, new ParallelOptions { MaxDegreeOfParallelism = threads }, y => {
                mainSobel(ptrOriginal, ptrResult, y, width);
            });
        }

        public static BitmapImage BitmapToImage(Bitmap bitmap) {

            using MemoryStream memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            BitmapImage bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();
            return bitmapimage;
        }
    }
}
