using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunyCamNF.frame
{
    public class Pixel
    {
        public unsafe Pixel(int x, int y, byte* src, int width, int heigth)
        {
            this.X = x;
            this.Y = y;
            this.src = src;
            this.heigth = heigth;
            this.width = width;
            this.r = src[3 * (x * heigth + y)];
            this.g = src[+3 * (x * heigth + y) + 1];
            this.b = src[+3 * (x * heigth + y) + 2];
        }
        private unsafe byte* src;
        private int X { get; set; }
        private int Y { get; set; }
        private int width, heigth;
        private unsafe byte r;
        private unsafe byte g;
        private unsafe byte b;
        public unsafe byte B
        {
            get { return src[3 * (Y * width + X)]; }
            set { src[3 * (Y * width + X)] = value; }
        }
        public unsafe byte G
        {
            get { return src[3 * (Y * width + X) + 1]; }
            set { src[3 * (Y * width + X) + 1] = value; }
        }
        public unsafe byte R
        {
            get { return src[3 * (Y * width + X) + 2]; }
            set { src[3 * (Y * width + X) + 2] = value; }
        }

        public unsafe void CopyPixel(Pixel pixel)
        {
            this.B = pixel.B;
            this.G = pixel.G;
            this.R = pixel.R;
        }
    }
}
