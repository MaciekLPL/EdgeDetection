using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EdgeDetection {
    unsafe class ImgProcessing {

        [DllImport(@"C:\Users\Maciek\source\repos\EdgeDetection\x64\Release\Asm.dll")]
        static extern void mainSobel(byte* input, byte* output, int rows, int cols);

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
                CSharp(ptrOriginal, ptrResult, width, height, stride, threads);
            else 
                Asm(ptrOriginal, ptrResult, width, height, threads);


            inputBmp.UnlockBits(inputBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }

        private static void CSharp(byte* ptrOriginal, byte* ptrResult, int width, int height, int stride, int threads) {

            sbyte[,] kX = new sbyte[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };
            sbyte[,] kY = new sbyte[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

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

                    ptrResult[centerPixel] = magB > 255 ? (byte)255 : (byte)magB;
                    ptrResult[centerPixel + 1] = magG > 255 ? (byte)255 : (byte)magG;
                    ptrResult[centerPixel + 2] = magR > 255 ? (byte)255 : (byte)magR;
                }
            });
        }

        private static void Asm(byte* ptrOriginal, byte* ptrResult, int width, int height, int threads) {

            _ = Parallel.For(1, height - 1, new ParallelOptions { MaxDegreeOfParallelism = threads }, y =>
            {
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
