using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EdgeDetection {
    unsafe class ImgProcessing {

        [DllImport(@"C:\Users\Maciek\source\repos\EdgeDetection\x64\Debug\Asm.dll")]
        static extern void mainSobel(byte* input, byte* output, int rows, int cols);

        public static Bitmap EdgeDetection(Bitmap inputBmp, int threads) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;

            sbyte[,] kX = new sbyte[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
            sbyte[,] kY = new sbyte[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptrOriginal = (byte*)inputBmpData.Scan0.ToPointer();

            Bitmap resultBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptrResult = (byte*)resultBmpData.Scan0.ToPointer();

            int stride = inputBmpData.Stride;

            _ = Parallel.For(1, height - 1, new ParallelOptions { MaxDegreeOfParallelism = threads }, y => {
                for (int x = 1; x < width - 1; x++) {

                    int centerPixel = (y * stride) + (x * 4);
                    int rx = 0, ry = 0, gx = 0, gy = 0, bx = 0, by = 0;

                    for (int matY = 0; matY < 3; matY++) {
                        for (int matX = 0; matX < 3; matX++) {

                            byte* currentPixel = ptrOriginal + ((y + matY - 1) * stride) + ((x + matX - 1) * 4);
                            sbyte vx = kX[matY, matX];
                            sbyte vy = kY[matY, matX];

                            bx += vx * *currentPixel;
                            gx += vx * *(currentPixel + 1);
                            rx += vx * *(currentPixel + 2);

                            by += vy * *currentPixel;
                            gy += vy * *(currentPixel + 1);
                            ry += vy * *(currentPixel + 2);

                        }
                    }

                    double magB = Math.Sqrt((bx * bx) + (by * by));
                    double magG = Math.Sqrt((gx * gx) + (gy * gy));
                    double magR = Math.Sqrt((rx * rx) + (ry * ry));

                    ptrResult[centerPixel] = magR > 255 ? (byte)255 : (byte)magR;
                    ptrResult[centerPixel + 1] = magG > 255 ? (byte)255 : (byte)magG;
                    ptrResult[centerPixel + 2] = magB > 255 ? (byte)255 : (byte)magB;
                }
            });

            inputBmp.UnlockBits(inputBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
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


        public static Bitmap EdgeDetectionAsm(Bitmap inputBmp, int threads) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptrOriginal = (byte*)inputBmpData.Scan0.ToPointer();

            Bitmap resultBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            byte* ptrResult = (byte*)resultBmpData.Scan0.ToPointer();

            mainSobel(ptrOriginal, ptrResult, height, width);

            inputBmp.UnlockBits(inputBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }

    }
}
