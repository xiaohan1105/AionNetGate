using AionCommons.Network.Packet;
using AionNetGate.Configs;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace AionNetGate.Netwok.Client
{
    public class CM_PICTURE_INFO : AbstractClientPacket
    {
        protected override void readImpl()
        {
            AionConnection ac = (AionConnection)getConnection();
            short sw = readH();
            short sh = readH();

            // 修复：检查图像尺寸有效性
            if (sw <= 0 || sh <= 0 || sw > 4096 || sh > 4096)
            {
                System.Diagnostics.Debug.WriteLine("无效的图像尺寸: " + sw + "x" + sh);
                return;
            }

            if (ac.image == null || ac.image.Width != sw || ac.image.Height != sh)
            {
                // 使用SetImage方法安全地替换图像，自动释放旧资源
                ac.SetImage(new Bitmap(sw, sh));
            }

            lock (ac.image)
            {
                using (Graphics g = Graphics.FromImage(ac.image))
                {
                    int size = readD();

                    // 修复：检查分块数量的合理性
                    if (size < 0 || size > 1000)
                    {
                        System.Diagnostics.Debug.WriteLine("无效的图像分块数量: " + size);
                        return;
                    }

                    for (int i = 0; i < size; i++)
                    {
                        int x = readC();
                        int y = readC();
                        int blockSize = readD();

                        // 修复：检查分块大小的合理性
                        if (blockSize <= 0 || blockSize > 1000000) // 1MB最大分块
                        {
                            System.Diagnostics.Debug.WriteLine("无效的图像分块大小: " + blockSize);
                            continue;
                        }

                        byte[] bs = readB(blockSize);

                        // 修复：使用默认分块大小如果配置未设置
                        int blockWidth = Config.image_width > 0 ? Config.image_width : 100;
                        int blockHeight = Config.image_height > 0 ? Config.image_height : 100;

                        int posX = x * blockWidth;
                        int posY = y * blockHeight;

                        // 修复：检查绘制位置是否超出边界
                        if (posX >= 0 && posY >= 0 && posX < ac.image.Width && posY < ac.image.Height)
                        {
                            Image blockImage = byteArrayToImage(bs);
                            if (blockImage != null)
                            {
                                try
                                {
                                    g.DrawImageUnscaled(blockImage, posX, posY);
                                }
                                finally
                                {
                                    blockImage.Dispose(); // 修复：及时释放分块图像
                                }
                            }
                        }
                    }
                }
            }
        }

        protected override void runImpl()
        {
            AionConnection ac = (AionConnection)getConnection();

            // 修复：检查deskForm是否为null以及是否已释放
            if (ac.deskForm != null && !ac.deskForm.IsDisposed)
            {
                try
                {
                    ac.deskForm.ShowImage(ac.image);
                }
                catch (Exception ex)
                {
                    // 如果显示图像失败，记录错误但不中断连接
                    System.Diagnostics.Debug.WriteLine("显示桌面图像失败: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// byte[]转换成Image
        /// </summary>
        /// <param name="byteArrayIn">二进制图片流</param>
        /// <returns>Image</returns>
        public static Image byteArrayToImage(byte[] byteArrayIn)
        {
            if (byteArrayIn == null || byteArrayIn.Length == 0)
                return null;

            try
            {
                using (MemoryStream ms = new MemoryStream(byteArrayIn))
                {
                    // 修复：验证流的有效性
                    if (ms.Length == 0)
                        return null;

                    Image returnImage = Image.FromStream(ms);
                    return returnImage;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("图像转换失败: " + ex.Message);
                return null;
            }
        }
    }
}
