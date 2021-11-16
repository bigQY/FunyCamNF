using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace FunyCamNF.filters
{
    class ConcaveFilter : BaseInPlacePartialFilter
    {
        public Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        public ConcaveFilter()
        {
            formatTranslations[PixelFormat.Format8bppIndexed] = PixelFormat.Format8bppIndexed;
            formatTranslations[PixelFormat.Format24bppRgb] = PixelFormat.Format24bppRgb;
            formatTranslations[PixelFormat.Format16bppGrayScale] = PixelFormat.Format16bppGrayScale;
            formatTranslations[PixelFormat.Format48bppRgb] = PixelFormat.Format48bppRgb;
        }

        private static void WriteBytesToPtr(IntPtr intPtr, byte[] bytes)
        {
            int j;
            for (j = 0; j < bytes.Length; j++)
            {
                Marshal.WriteByte(intPtr, j, bytes[j]);
            }
        }


        protected override unsafe void ProcessFilter(UnmanagedImage image, Rectangle rect)
        {
            int pixelSize = ((image.PixelFormat == PixelFormat.Format8bppIndexed) || (image.PixelFormat == PixelFormat.Format16bppGrayScale) ? 1 : 3);
            int startY = rect.Top;
            int stopY = startY + rect.Height;
            int startX = rect.Left * pixelSize;
            int stopX = startX + rect.Width * pixelSize;
            int srcLength = rect.Width * rect.Height * pixelSize;
            byte* basePtr = (byte*)image.ImageData.ToPointer();
            byte[] result = pixelProcess(basePtr, srcLength, rect.Width, rect.Height);
            WriteBytesToPtr(image.ImageData, result);
        }

        private unsafe byte[] pixelProcess(byte* srcData, int length, int w, int h)
        {
            int oldX = 0;
            int oldY = 0;
            int centerX = w / 2;
            int centerY = h / 2;
            byte[] tempData = new byte[length];
            //扭曲区间
            int R = 4;
            for (int j = 0; j < h; j++)
            {
                for (int i = 0; i < w; i++)
                {
                    oldX = i;
                    //oldY = j * 2;
                    oldY = j;
                    //R = ((R + 1) % 700);
                    double theta = Math.Atan2((double)(oldY - centerY), (double)(oldX - centerX));
                    double dis = Math.Sqrt((centerX - i) * (centerX - i) + (centerY - j) * (centerY - j));
                    /*
                                        int newX = centerX + (int)(R * Math.Cos(theta));
                                        int newY = centerY + (int)(R * Math.Sin(theta));*/

                    /* x = int(cx + (math.sqrt(math.sqrt(tx * tx + ty * ty)) * compress * math.cos(math.atan2(ty, tx))))
                     y = int(cy + (math.sqrt(math.sqrt(tx * tx + ty * ty)) * compress * math.sin(math.atan2(ty, tx))))
 */
                    int tempx = oldX - centerX;
                    int tempy = oldY - centerY;
                    int newX = centerX + (int)Math.Sqrt(dis* R * Math.Cos(Math.Atan2(tempy,tempx)));
                    int newY = centerY + (int)Math.Sqrt(dis * R * Math.Sin(Math.Atan2(tempy, tempx)));


                    if (newX < 0)
                        newX = 0;
                    else if (newX >= w)
                        newX = w - 1;

                    if (newY < 0)
                        newY = 0;
                    else if
                        (newY >= h) newY = h - 1;


                    int tempB = i * 3 + j * w * 3;
                    int tempG = i * 3 + j * w * 3 + 1;
                    int tempR = i * 3 + j * w * 3 + 2;
                    int srcB = newX * 3 + newY * w * 3;
                    int srcG = newX * 3 + newY * w * 3 + 1;
                    int srcR = newX * 3 + newY * w * 3 + 2;
                    if ((tempB <= length) && (tempG <= length) && (tempR <= length) && (srcB <= length) && (srcG <= length) && (srcR <= length))
                    {
                        tempData[tempB] = srcData[srcB];
                        tempData[tempG] = srcData[srcG];
                        tempData[tempR] = srcData[srcR];
                    }
                }
            }
            return tempData;
        }
    }
}
