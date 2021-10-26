using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunyCamNF.frame;

namespace FunyCamNF.filters
{
    class DynamicRotateFilter : BaseInPlaceFilter
    {
        public DynamicRotateFilter()
        {
            // initialize format translation dictionary
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format48bppRgb;
        }
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
        /// <param name="se">Structuring element to pass to <see cref="Opening"/> operator.</param>
        /// 
        public DynamicRotateFilter(short[,] se) : this()
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
            Pixel[,] newFrame = new Pixel[width, height];
            byte[] newFrameSrc = new byte[3 * width * height];
            byte* newFrameSrcp;
            fixed (byte* t = newFrameSrc)
            {
                newFrameSrcp = t;
            }
            //copy
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Pixel pixel = new Pixel(x, y, newFrameSrcp, width, height);
                    pixel.B = 0;
                    pixel.G = 0;
                    pixel.R = 0;
                    newFrame[x, y] = pixel;
                }
            }
            //transform

            //x = (x'- rx0)*cos(RotaryAngle) - (y' - ry0)*sin(RotaryAngle) + rx0;

            //y = (x'- rx0)*sin(RotaryAngle) + (y' - ry0)*cos(RotaryAngle) + ry0;
            double angle = 20/180 * Math.PI;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                    double x1 = 0.5*(x - (width / 2) * Math.Cos(angle)) + (y - (height / 2) * Math.Sin(angle) + (width / 2));
                    double y1 = 0.5*-(x - (width / 2) * Math.Sin(angle)) + (y -  (height / 2) * Math.Cos(angle) + (height / 2));
                    if (x1 >= width || x1 < 0 || y1 >= height || y1 < 0)
                    {
                        
                    }
                    else
                    {
                        Pixel pixel = newFrame[(int)x1, (int)y1];
                        Pixel pixel1 = new Pixel(x, y, src, width, height);
                        pixel.B = pixel1.B;
                        pixel.G = pixel1.G;
                        pixel.R = pixel1.R;
                    }
  
                }
            }
            //apply
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Pixel pixel = newFrame[x, y];
                    if (pixel != null)
                    {
                        Pixel t = new Pixel(x, y, src, width, height);
                        t.B = pixel.B;
                        t.G = pixel.G;
                        t.R = pixel.R;
                    }
                }
            }

        }
    }
}
