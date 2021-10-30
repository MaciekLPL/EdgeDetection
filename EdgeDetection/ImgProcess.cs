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
        private static int[,] KernelX => new int[,]
                    {
                        {-1, 0, 1},
                        {-2, 0, 2},
                        {-1, 0, 1}
                    };

        private static int[,] KernelY => new int[,]
                    {
                        { 1, 2, 1},
                        { 0, 0, 0},
                        {-1,-2,-1}
                    };

        unsafe public Bitmap EdgeDetection(Bitmap inputBmp, int limit) {

            int width = inputBmp.Width;
            int height = inputBmp.Height;

            int bpp = Image.GetPixelFormatSize(inputBmp.PixelFormat) / 8;

            BitmapData inputBmpData = inputBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, inputBmp.PixelFormat);

            Bitmap resultBmp = new Bitmap(width, height, inputBmp.PixelFormat);
            BitmapData resultBmpData = resultBmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadWrite, resultBmp.PixelFormat);

            
            byte* ptrOriginal = (byte*)inputBmpData.Scan0.ToPointer();
            byte* ptrResult = (byte*)resultBmpData.Scan0.ToPointer();
            byte* currentPixel;

            for (int y = 1; y < height - 1; y++) {
                for(int x = 1; x < width - 1; x++) {
                    
                    int gx = 0, gy = 0;
                    int currPixel = (y * inputBmpData.Stride) + (x * bpp);

                    for (int matY = 0; matY < 3; matY++) {
                        for (int matX = 0; matX < 3; matX++) {

                            currentPixel = ptrOriginal + ((y + matY - 1) * inputBmpData.Stride) + ((x + matX - 1) * bpp);
                            gx += KernelX[matY, matX] * (*currentPixel);
                            gy += KernelX[matY, matX] * (*currentPixel);

                        }
                    }

                    if ((gx * gx) + (gy * gy) > limit * limit)
                        ptrResult[currPixel] = ptrResult[currPixel + 1] = ptrResult[currPixel + 2] = 255;
                    else
                        ptrResult[currPixel] = ptrResult[currPixel + 1] = ptrResult[currPixel + 2] = 0;
                }
            }


            inputBmp.UnlockBits(inputBmpData);
            resultBmp.UnlockBits(resultBmpData);
            return resultBmp;
        }

        public BitmapImage BitmapToImage(Bitmap bitmap) {

            using (MemoryStream memory = new MemoryStream()) {
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
}
