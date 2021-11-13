﻿using Microsoft.Win32;
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
        private Bitmap resultBitmap;

        private void btnSelectImage_Click(object sender, RoutedEventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".bmp";
            ofd.Filter = "BMP Files (*.bmp)|*.bmp";

            if (ofd.ShowDialog() == true) {
                string filename = ofd.FileName;
                inputBitmap = new Bitmap(filename);
                imgSelected.Source = ImgProcess.BitmapToImage(inputBitmap);
            }
        }

        private void BtnStart_Click(object sender, RoutedEventArgs e) {

            if(inputBitmap != null) {
                timer.Restart();
                resultBitmap = ImgProcess.EdgeDetection(inputBitmap, (int)sliderThreads.Value);
                timer.Stop();

                imgFilter.Source = ImgProcess.BitmapToImage(resultBitmap);
                
                TimeSpan time = timer.Elapsed;
                textblockTimer.Visibility = Visibility.Visible;
                textblockTimer.Text = $"Timer: {time.Minutes}m {time.Seconds}s {time.Milliseconds}ms";
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
