using AForge.Imaging;
using AForge.Imaging.Filters;
using FunyCamNF.frame;
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
                    Pixel pixel1 = new Pixel(x, y, src, width, height);
                    pixel.B = pixel1.B;
                    pixel.G = pixel1.G;
                    pixel.R = pixel1.R;
                    newFrame[x, y] = pixel;
                }
            }
            //transform
            int range = 400;
            for (int x = 0; x < width; x++)
            {
                double temp = (width - range) * Math.Sin(((2 * Math.PI * x) / height) / 2);
                if (temp < 0)
                {
                    temp = 0;
                }

                for (int j = (int)temp; j < height - temp; j++)
                {
                    double distance = height - temp;
                    double ratio = distance / height;
                    double stepsize = 1.0 / ratio;
                    Pixel pixel = newFrame[x, j];
                    Pixel pixel1 = new Pixel(x, (int)((int)(j - temp) * stepsize), src, width, height);
                    pixel.B = pixel1.B;
                    pixel.G = pixel1.G;
                    pixel.R = pixel1.R;
                }
            }


            /*       for (int x = 0; x < width; x++)
                   {
                       for (int y = 0; y < height; y++)
                       {
                           Pixel pixel = newFrame[x, y];
                           //Math.Abs((int)(y * Math.Sin(y)))
                           Pixel pixel1 = new Pixel(Math.Abs((int)(x *Math.Sin(x))),y , src, width, height);
                           pixel.B = pixel1.B;
                           pixel.G = pixel1.G;
                           pixel.R = pixel1.R;
                       }
                   }*/

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


            /*            for(int x = 0; x < width; x++)
                        {
                            for(int y = 0; y < height; y++)
                            {
                                *//*if (x <= 500)
                                {
                                    Pixel a = new Pixel(x, y, dst, width, height);
                                    a.B = 0;
                                    a.G = 0;
                                    a.R = 255;
                                }
                                if (y <=200)
                                {
                                    Pixel a = new Pixel(x, y, dst, width, height);
                                    a.B = 0;
                                    a.G = 255;
                                    a.R = 0;
                                }*//*

                            }
                        }*/
        }
    }
}
