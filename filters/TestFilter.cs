using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FunyCamNF.filters
{
    class TestFilter : BaseInPlaceFilter
    {
        private Opening opening = new Opening();
        private Subtract subtract = new Subtract();

        // private format translation dictionary
        private Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        /// <summary>
        /// Format translations dictionary.
        /// </summary>
        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopHat"/> class.
        /// </summary>
        /// 
        public TestFilter()
        {
            // initialize format translation dictionary
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format48bppRgb;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TopHat"/> class.
        /// </summary>
        /// 
        /// <param name="se">Structuring element to pass to <see cref="Opening"/> operator.</param>
        /// 
        public TestFilter(short[,] se) : this()
        {
            opening = new Opening(se);
        }

        /// <summary>
        /// Process the filter on the specified image.
        /// </summary>
        /// 
        /// <param name="image">Source image data.</param>
        ///
        protected override unsafe void ProcessFilter(UnmanagedImage image)
        {
            Random rd = new Random();
            int width = image.Width;
            int height = image.Height;
            // do the job
            byte* src = (byte*)image.ImageData.ToPointer();
            // allign pointer to the first pixel to process

            byte* dst = src;
            for (int i = 0; i < 3 * width * height; i++)
            {
                if (rd.Next(1, 100)>0.1*rd.Next(1, 100))
                {
                   *src = (byte)rd.Next(1, 255);
                    //Pixel a = new Pixel((i / 3) % width, (i / 3) % height, dst, width, height);
                    //Pixel b = new Pixel((i / 3)*2 % width, (i / 3)*2 % height, dst, width, height);
                    //a.CopyPixel(b);
                    //*(src) = (byte)rd.Next(1, 255);
                }
                src += 1;
            }
        }
        public class Pixel
        {
            public unsafe Pixel(int x, int y, byte* src, int width, int heigth)
            {
                this.X = x;
                this.Y = y;
                this.src = src;
                this.heigth = heigth;
                this.width = width;
                this.r = src + 3 * (x * heigth + y);
                this.g = src + 3 * (x * heigth + y) + 1;
                this.b = src + 3 * (x * heigth + y) + 2;
            }
            private unsafe byte* src;
            private int X { get; set; }
            private int Y { get; set; }
            private int width, heigth;
            private unsafe byte* r;
            private unsafe byte* g;
            private unsafe byte* b;
            public unsafe byte R
            {
                get { return *r; }
                set { *r = value; }
            }
            public unsafe byte G
            {
                get { return *g; }
                set { *g = value; }
            }
            public unsafe byte B
            {
                get { return *b; }
                set { *b = value; }
            }

            public unsafe void CopyPixel(Pixel pixel)
            {
                this.R = pixel.R;
                this.G = pixel.G;
                this.B = pixel.B;
            }
        }
    }
}
