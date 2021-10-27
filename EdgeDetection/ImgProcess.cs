using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EdgeDetection {
    class ImgProcess {
        private static double[,] KernelX => new double[,]
                    {
                        {-1, 0, 1},
                        {-2, 0, 2},
                        {-1, 0, 1}
                    };

        private static double[,] KernelY => new double[,]
                    {
                        { 1, 2, 1},
                        { 0, 0, 0},
                        {-1,-2,-1}
                    };

        public void grayscale() {



        }

    }
}
