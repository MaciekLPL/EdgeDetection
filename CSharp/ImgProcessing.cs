/* ******************************************
 * Temat: Wykrywanie krawędzi - Operator Sobela
 * Autor: Maciej Lejczak, Informatyka Katowice, semestr 5, grupa 2
 * Prowadządzy: mgr inż. Krzysztof Hanzel
 * Rok akademicki: 2021/2022
 * ******************************************/

using System;
using System.Threading.Tasks;

namespace CSharp {
    public static class ImgProcessing {

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

        /*
         * C# implementation
         * ptrOriginal - pointer to original image
         * ptrResult - pointer to result image
         * width - width of images
         * height - height of images
         * stride - stride of images
         * threads - number of threads selected by user
         */
        private static void CSharp(byte* ptrOriginal, byte* ptrResult, int width, int height, int stride, int threads) {

            sbyte[,] kX = new sbyte[,] { { 1, 0, -1 }, { 2, 0, -2 }, { 1, 0, -1 } };                                    //Kernels definitions
            sbyte[,] kY = new sbyte[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };

            _ = Parallel.For(1, height - 1, new ParallelOptions { MaxDegreeOfParallelism = threads }, y => {            //Row loop (parallel)
                for (int x = 1; x < width - 1; x++) {                                                                   //Column loop

                    int centerPixel = (y * stride) + (x * 4);                                                           //Central pixel (in matrix)
                    int rx = 0, ry = 0, gx = 0, gy = 0, bx = 0, by = 0;                                                 //Thread safe variables initialization

                    for (int matY = 0; matY < 3; matY++) {                                                              //Matrix loop (Y)
                        for (int matX = 0; matX < 3; matX++) {                                                          //Matrix loop (X)

                            byte* currentPixel = ptrOriginal + ((y + matY - 1) * stride) + ((x + matX - 1) * 4);        //Pixel currently being computed 
                            sbyte vx = kX[matY, matX];                                                                  //Get values from kernels
                            sbyte vy = kY[matY, matX];

                            bx += vx * *currentPixel;                                                                   //Multiplying and adding to result (X kernel)
                            gx += vx * *(currentPixel + 1);
                            rx += vx * *(currentPixel + 2);

                            by += vy * *currentPixel;                                                                   //Multiplying and adding to result (Y kernel)
                            gy += vy * *(currentPixel + 1);
                            ry += vy * *(currentPixel + 2);

                        }
                    }

                    double magB = Math.Sqrt((bx * bx) + (by * by));                                                     //Gradient magnitude
                    double magG = Math.Sqrt((gx * gx) + (gy * gy));
                    double magR = Math.Sqrt((rx * rx) + (ry * ry));

                    ptrResult[centerPixel] = magB > 255 ? (byte)255 : (byte)magB;                                       //Enter values into output array
                    ptrResult[centerPixel + 1] = magG > 255 ? (byte)255 : (byte)magG;                                   //255 or gradient magnitude
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
