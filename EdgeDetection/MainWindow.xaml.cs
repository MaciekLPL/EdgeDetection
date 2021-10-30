﻿using System;
using System.Collections.Generic;
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
            filter = new ImgProcess();
        }

        Bitmap sourceBitmap;
        ImgProcess filter;

        private void btnSelectImage_Click(object sender, RoutedEventArgs e) {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".bmp";
            dialog.Filter = "BMP Files (*.bmp)|*.bmp";

            Nullable<bool> result = dialog.ShowDialog();

            if (result == true) {
                string filename = dialog.FileName;
                textblockTimer.Text = filename;
                sourceBitmap = new Bitmap(filename);
            }
        }

        private void btnStart_Click(object sender, RoutedEventArgs e) {


            Bitmap es = filter.EdgeDetection(sourceBitmap, (int) sliderThreads.Value);
            imgFilter.Source = filter.BitmapToImage(es);

        }
    }
}
