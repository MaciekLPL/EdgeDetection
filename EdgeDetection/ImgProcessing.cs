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
    class ImgProcessing {
         
        public static unsafe Bitmap EdgeDetection(Bitmap inputBmp, int threads) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;

            int bpp = Image.GetPixelFormatSize(inputBmp.PixelFormat) / 8;

            sbyte[,] kX = new sbyte[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            sbyte[,] kY = new sbyte[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, inputBmp.PixelFormat);
            byte* ptrOriginal = (byte*)inputBmpData.Scan0.ToPointer();

            Bitmap resultBmp = new Bitmap(width, height, inputBmp.PixelFormat);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, resultBmp.PixelFormat);
            byte* ptrResult = (byte*)resultBmpData.Scan0.ToPointer();

            int stride = inputBmpData.Stride;

            _ = Parallel.For(1, height - 1, new ParallelOptions { MaxDegreeOfParallelism = threads }, y => {
                for (int x = 1; x < width - 1; x++) {

                    int centerPixel = (y * stride) + (x * bpp);
                    int rx = 0, ry = 0, gx = 0, gy = 0, bx = 0, by = 0;

                    for (int matY = 0; matY < 3; matY++) {
                        for (int matX = 0; matX < 3; matX++) {

                            byte* currentPixel = ptrOriginal + ((y + matY - 1) * stride) + ((x + matX - 1) * bpp);
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


        /*public static unsafe Bitmap EdgeDetection2(Bitmap inputBmp, int threads) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;
            int bpp = Image.GetPixelFormatSize(inputBmp.PixelFormat) / 8;

            int[,] kX = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
            int[,] kY = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, inputBmp.PixelFormat);
            
            int stride = inputBmpData.Stride;
            int bytes = stride * height;

            byte[] pixelBuffer = new byte[bytes];
            byte[] resultBuffer = new byte[bytes];

            Marshal.Copy(inputBmpData.Scan0, pixelBuffer, 0, bytes);
            inputBmp.UnlockBits(inputBmpData);

            _ = Parallel.For(1, height - 1, new ParallelOptions { MaxDegreeOfParallelism = threads }, y => {
                for (int x = 1; x < width - 1; x++) {

                    int rx = 0, ry = 0, gx = 0, gy = 0, bx = 0, by = 0;
                    int pixel = (y * stride) + (x * bpp);

                    for (int matY = 0; matY < 3; matY++) {
                        for (int matX = 0; matX < 3; matX++) {

                            int currentPixel = pixel + ((matX - 1) * bpp) + ((matY - 1) * stride);
                            int wx = kX[matY, matX];
                            int wy = kY[matY, matX];

                            bx += pixelBuffer[currentPixel] * wx;
                            gx += pixelBuffer[currentPixel + 1] * wx;
                            rx += pixelBuffer[currentPixel + 2] * wx;

                            by += pixelBuffer[currentPixel] * wy;
                            gy += pixelBuffer[currentPixel + 1] * wy;
                            ry += pixelBuffer[currentPixel + 2] * wy;
                        }
                    }

                    double magB = Math.Sqrt((bx * bx) + (by * by));
                    double magG = Math.Sqrt((gx * gx) + (gy * gy));
                    double magR = Math.Sqrt((rx * rx) + (ry * ry));

                    resultBuffer[pixel] = magB > 255 ? (byte)255 : (byte)magB;
                    resultBuffer[pixel + 1] = magG > 255 ? (byte)255 : (byte)magG;
                    resultBuffer[pixel + 2] = magR > 255 ? (byte)255 : (byte)magR;
                }
            });

            Bitmap resultBmp = new Bitmap(width, height, inputBmp.PixelFormat);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
            Marshal.Copy(resultBuffer, 0, resultBmpData.Scan0, resultBuffer.Length);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }*/
    }
}
