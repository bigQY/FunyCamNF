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
    unsafe class ConvexFilterMultithread : BaseInPlacePartialFilter
    {

        private static readonly object Lock = new object();

        byte[] tempDataByte = null;

        int centerX;
        int centerY;

        byte* srcData;

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
            int srcLength = rect.Width * rect.Height * pixelSize;
            byte* basePtr = (byte*)image.ImageData.ToPointer();
            srcData = basePtr;
            tempDataByte = new byte[srcLength];
            centerX = rect.Width / 2;
            centerY = rect.Height / 2;
            Task[] tasks = new Task[4]
            {
                Task.Factory.StartNew(()=>pixelProcess(0,0,centerX,centerY)),
                Task.Factory.StartNew(()=>pixelProcess(centerX,0,rect.Width,centerY)),
                Task.Factory.StartNew(()=>pixelProcess(0,centerY,centerX,rect.Height)),
                Task.Factory.StartNew(()=>pixelProcess(centerX,centerY,rect.Width,rect.Height)),
            };
            Task.WaitAll(tasks);
            WriteBytesToPtr(image.ImageData, tempDataByte);
        }

        private unsafe void pixelProcess( int startW,int startH,int endW, int endH)
        {
            
            int length=tempDataByte.Length;
            //扭曲区间
            int R;
            lock (Lock)
            {
                R = (int)(Math.Sqrt(4 * centerX * centerX + 4 * centerY * centerY) / 2);
            }

            for (int j = startH; j < endH; j++)
            {
                for (int i = startW; i < endW; i++)
                {
                    double dis;
                    lock (Lock)
                    {
                        dis = Math.Sqrt((centerX - i) * (centerX - i) + (centerY - j) * (centerY - j));
                    }

                    int newX = (int)((i - centerX) * dis / R + centerX);
                    int newY = (int)((j - centerY) * dis / R + centerY);

                    int tempB = i * 3 + j * centerX*2 * 3;
                    int tempG = i * 3 + j * centerX * 2 * 3 + 1;
                    int tempR = i * 3 + j * centerX * 2 * 3 + 2;
                    int srcB = newX * 3 + newY * centerX * 2 * 3;
                    int srcG = newX * 3 + newY * centerX * 2 * 3 + 1;
                    int srcR = newX * 3 + newY * centerX * 2 * 3 + 2;
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
