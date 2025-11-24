using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace AionLanucher.Utilty
{
    class ImageProcess
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();

        [DllImport("gdi32.dll")]
        private static extern bool BitBlt
        (
            IntPtr hdcDest, //指向目标设备环境的句柄
            int nXDest, //指定目标矩形区域克上角的X轴逻辑坐标
            int nYDest, //指定目标矩形区域左上角的Y轴逻辑坐标
            int nWidth, //指定源和目标矩形区域的逻辑宽度
            int nHeight, //指定源和目标矩形区域的逻辑高度
            IntPtr hdcSrc, //指向源设备环境句柄
            int nXSrc, //指定源矩形区域左上角的X轴逻辑坐标
            int nYSrc, //指定源矩形区域左上角的Y轴逻辑坐标
            System.Int32 dwRop //指定光栅操作代码。这些代码将定义源矩形区域的颜色数据，如何与目标矩形区域的颜色数据组合以完成最后的颜色
        );

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int nIndex);


        internal static Size GetDesktopBitmapSize()
        {
            return new Size(GetSystemMetrics(0), GetSystemMetrics(1));
        }

        internal static Bitmap GetDesktopBitmap()
        {
            Size DesktopBitmapSize = GetDesktopBitmapSize();
            Graphics Graphic = Graphics.FromHwnd(GetDesktopWindow());//从窗口的指定句柄创建新的 Graphics 对象
            Bitmap MemImage = new Bitmap(DesktopBitmapSize.Width, DesktopBitmapSize.Height, Graphic);//生成图像
            Graphics MemGraphic = Graphics.FromImage(MemImage);//从指定的 Image 对象创建新 Graphics 对象
            IntPtr dc1 = Graphic.GetHdc();//获取与此 Graphics 对象关联的设备上下文的句柄
            IntPtr dc2 = MemGraphic.GetHdc();
            BitBlt(dc2, 0, 0, DesktopBitmapSize.Width, DesktopBitmapSize.Height, dc1, 0, 0, 0xCC0020);
            Graphic.ReleaseHdc(dc1);//释放通过以前对此 Graphics 对象的 GetHdc 方法的调用获得的设备上下文句柄
            MemGraphic.ReleaseHdc(dc2);
            Graphic.Dispose();
            MemGraphic.Dispose();
            return MemImage;
        }


        internal Dictionary<string, byte[]> sd;

        internal ImageProcess()
        {
            sd = new Dictionary<string, byte[]>();
        }

        /// <summary>
        /// 获取差异化图片数组
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        internal List<Bitmap> ThreadDo(int width, int height)
        {
            Bitmap bitmap = GetDesktopBitmap();
            int MaxColumn = (int)Math.Ceiling((decimal)bitmap.Width / width);
            int MaxRow = (int)Math.Ceiling((decimal)bitmap.Height / height);

            List<Bitmap> bsss = new List<Bitmap>();

            for (int i = 0; i < MaxRow; i++)
            {
                for (int j = 0; j < MaxColumn; j++)
                {

                    Bitmap bmp = new Bitmap(width, height);
                    bmp.Tag = i + "," + j;
                    using (Graphics newBmpGraphics = Graphics.FromImage(bmp))
                    {
                        Rectangle rect = new Rectangle(j * width, i * height, width, height);
                        newBmpGraphics.DrawImage(bitmap, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
                        newBmpGraphics.Save();
                    }
                    byte[] bs = MemoryCopy(bmp);

                    if (!sd.ContainsKey(bmp.Tag.ToString()))
                    {
                        sd.Add(bmp.Tag.ToString(), bs);
                        bsss.Add(bmp);
                    }
                    else
                    {
                        byte[] old = sd[bmp.Tag.ToString()];
                        for (int a = 0; a < bs.Length; a++)
                        {
                            if (bs[a] != old[a])
                            {
                                sd.Remove(bmp.Tag.ToString());
                                sd.Add(bmp.Tag.ToString(), bs);
                                bsss.Add(bmp);
                                break;
                            }
                        }
                    }

                }
            }
            return bsss;
        }

        /// <summary>
        /// 内存拷贝法
        /// </summary>
        /// <param name="curBitmap"></param>
        private byte[] MemoryCopy(Bitmap curBitmap)
        {
            int width = curBitmap.Width;
            int height = curBitmap.Height;
            Rectangle rect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = curBitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);//curBitmap.PixelFormat
            IntPtr ptr = bmpData.Scan0;
            int bytesCount = bmpData.Stride * bmpData.Height;
            byte[] arrDst = new byte[bytesCount];
            Marshal.Copy(ptr, arrDst, 0, bytesCount);
            curBitmap.UnlockBits(bmpData);
            return arrDst;
        }


        /// <summary>
        /// 压缩图片JPG格式
        /// </summary>
        /// <param name="iSource">图片源</param>
        /// <param name="flag">压缩率</param>
        /// <returns></returns>
        internal byte[] GetCompressed(Bitmap iSource, int flag)
        {
            using (MemoryStream compressed = new MemoryStream())
            {
                if (flag == 100)
                {
                    iSource.Save(compressed, ImageFormat.Png);
                    return compressed.ToArray();
                }

                ImageFormat tFormat = iSource.RawFormat;
                //以下代码为保存图片时，设置压缩质量 
                EncoderParameters ep = new EncoderParameters();
                long[] qy = new long[1] { flag };//设置压缩的比例1-100 
                EncoderParameter eParam = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, qy);
                ep.Param[0] = eParam;
                try
                {
                    ImageCodecInfo[] arrayICI = ImageCodecInfo.GetImageEncoders();
                    ImageCodecInfo jpegICIinfo = null;
                    for (int x = 0; x < arrayICI.Length; x++)
                    {
                        if (arrayICI[x].FormatDescription.Equals("JPEG"))
                        {
                            jpegICIinfo = arrayICI[x];
                            break;
                        }
                    }
                    if (jpegICIinfo != null)
                    {
                        iSource.Save(compressed, jpegICIinfo, ep);//dFile是压缩后的新路径 
                    }
                    else
                    {
                        iSource.Save(compressed, tFormat);
                    }
                    compressed.Seek(0, SeekOrigin.Begin);
                }
                catch
                {

                }
                finally
                {
                    iSource.Dispose();
                }

                return compressed.ToArray();
            }
        }
    }
}
