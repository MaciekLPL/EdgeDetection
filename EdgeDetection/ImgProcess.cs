using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace EdgeDetection {
    class ImgProcess {

        private static int[,] KernelX => new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
        private static int[,] KernelY => new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

        public static unsafe Bitmap EdgeDetection(Bitmap inputBmp) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;

            int bpp = Image.GetPixelFormatSize(inputBmp.PixelFormat) / 8;

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, inputBmp.PixelFormat);

            Bitmap resultBmp = new Bitmap(width, height, inputBmp.PixelFormat);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, resultBmp.PixelFormat);

            byte* ptrOriginal = (byte*)inputBmpData.Scan0.ToPointer();
            byte* ptrResult = (byte*)resultBmpData.Scan0.ToPointer();
            byte* currentPixel;

            int centerPixel, rx = 0, ry = 0, gx = 0, gy = 0, bx = 0, by = 0;

            for (int y = 1; y < height - 1; y++) {
                for (int x = 1; x < width - 1; x++) {

                    rx = ry = gx = gy = bx = by = 0;

                    for (int matY = 0; matY < 3; matY++) {
                        for (int matX = 0; matX < 3; matX++) {

                            currentPixel = ptrOriginal + ((y + matY - 1) * inputBmpData.Stride) + ((x + matX - 1) * bpp);                
                            bx += KernelX[matY, matX] * *currentPixel;
                            gx += KernelX[matY, matX] * *(currentPixel + 1);
                            rx += KernelX[matY, matX] * *(currentPixel + 2);

                            by += KernelY[matY, matX] * *currentPixel;
                            gy += KernelY[matY, matX] * *(currentPixel + 1);
                            ry += KernelY[matY, matX] * *(currentPixel + 2);

                        }
                    }

                    double magB = Math.Sqrt((bx * bx) + (by * by));
                    double magG = Math.Sqrt((gx * gx) + (gy * gy));
                    double magR = Math.Sqrt((rx * rx) + (ry * ry));

                    centerPixel = (y * inputBmpData.Stride) + (x * bpp);
                    ptrResult[centerPixel] = magB > 255 ? (byte)255 : (byte)magB;
                    ptrResult[centerPixel + 1] = magG > 255 ? (byte)255 : (byte)magG;
                    ptrResult[centerPixel + 2] = magR > 255 ? (byte)255 : (byte)magR;
                }
            }

            inputBmp.UnlockBits(inputBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }

        public static BitmapImage BitmapToImage(Bitmap bitmap) {

            using MemoryStream memory = new MemoryStream(); bitmap.Save(memory, ImageFormat.Bmp);
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
