using AForge.Imaging;
using AForge.Imaging.Filters;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace FunyCamNF.filters
{
    class ConvexFilterMultithread : BaseInPlacePartialFilter
    {

        private static readonly object Lock = new object();

        byte[] tempDataByte = null;

        public Dictionary<PixelFormat, PixelFormat> formatTranslations = new Dictionary<PixelFormat, PixelFormat>();

        public override Dictionary<PixelFormat, PixelFormat> FormatTranslations
        {
            get { return formatTranslations; }
        }

        public ConvexFilterMultithread()
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
            tempDataByte = new byte[srcLength];
            Task[] tasks = new Task[4]
            {
                Task.Factory.StartNew(()=>pixelProcess(basePtr,0,0,rect.Width/2,rect.Height/2)),
                Task.Factory.StartNew(()=>pixelProcess(basePtr,rect.Width/2,0,rect.Width,rect.Height/2)),
                Task.Factory.StartNew(()=>pixelProcess(basePtr,0,rect.Height/2,rect.Width/2,rect.Height)),
                Task.Factory.StartNew(()=>pixelProcess(basePtr,rect.Width/2,rect.Height/2,rect.Width,rect.Height)),
            };
            Task.WaitAll(tasks);
            WriteBytesToPtr(image.ImageData, tempDataByte);
        }

        private unsafe void pixelProcess(byte* srcData, int startW,int startH,int endW, int endH)
        {
            int oldX = 0;
            int oldY = 0;
            int centerX = endW -startW;
            int centerY = endH -startH;
            int length=tempDataByte.Length;
            //扭曲区间
            int R;
            lock (Lock)
            {
                R = (int)(Math.Sqrt(4 * centerX * centerX + 4 * centerY * centerY) / 2);
            }

            for (int j = 0; j < endH; j++)
            {
                for (int i = 0; i < endW; i++)
                {
                    oldX = i;
                    oldY = j;
                    double dis;
                    lock (Lock)
                    {
                        dis = Math.Sqrt((centerX - i) * (centerX - i) + (centerY - j) * (centerY - j));
                    }

                    int newX = (int)((oldX - centerX) * dis / R + centerX);
                    int newY = (int)((oldY - centerY) * dis / R + centerY);

                    int tempB = i * 3 + j * endW * 3;
                    int tempG = i * 3 + j * endW * 3 + 1;
                    int tempR = i * 3 + j * endW * 3 + 2;
                    int srcB = newX * 3 + newY * endW * 3;
                    int srcG = newX * 3 + newY * endW * 3 + 1;
                    int srcR = newX * 3 + newY * endW * 3 + 2;
                    if ((tempB <= length) && (tempG <= length) && (tempR <= length) && (srcB <= length) && (srcG <= length) && (srcR <= length))
                    {
                        Monitor.Enter(Lock);
                        tempDataByte[tempB] = srcData[srcB];
                        tempDataByte[tempG] = srcData[srcG];
                        tempDataByte[tempR] = srcData[srcR];
                        Monitor.Exit(Lock);
                    }
                }
            }
        }
    }
}
