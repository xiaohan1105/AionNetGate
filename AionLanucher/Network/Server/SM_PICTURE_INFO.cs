using AionLanucher.Configs;
using AionLanucher.Utilty;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AionLanucher.Network.Server
{
    class SM_PICTURE_INFO : AbstractServerPacket
    {
        protected override void writeImpl()
        {
            AionConnection ac = (AionConnection)getConnection();
            if (ac.imageProcess == null)
                ac.imageProcess = new ImageProcess();
            List<Bitmap> bs = ac.imageProcess.ThreadDo(ac.image_width, ac.image_height);
            Size s = ImageProcess.GetDesktopBitmapSize();
            writeH((short)s.Width);
            writeH((short)s.Height);
            writeD(bs.Count);
            foreach (Bitmap b in bs)
            {
                byte[] CurrentBitmapBytes = ac.imageProcess.GetCompressed(b, ac.image_compress_rate);
                string[] sd = b.Tag.ToString().Split(',');
                b.Dispose();
                writeC(byte.Parse(sd[1]));
                writeC(byte.Parse(sd[0]));
                writeD(CurrentBitmapBytes.Length);
                writeB(CurrentBitmapBytes);
            }
            bs.Clear();
        }

    }
}
