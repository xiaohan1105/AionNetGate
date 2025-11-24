using System;
using System.Collections.Generic;
using System.Text;

namespace AionLanucher.Utilty
{
    class AionFile
    {
        private string filename;
        private string Md5;
        private int End;
        private int Start;
        private int Or_length;

        internal AionFile(string fileName, string md5, int start, int end, int or_length)
        {
            this.filename = fileName;
            this.Md5 = md5;
            this.Start = start;
            this.End = end;
            this.Or_length = or_length;
        }
        /// <summary>
        /// 文件相对名字,如：data\china\items.pak
        /// </summary>
        internal string fileName
        {
            get { return filename; }
            set { filename = value; }
        }
        /// <summary>
        /// 文件MD5值
        /// </summary>
        internal string md5
        {
            get { return Md5; }
            set { Md5 = value; }
        }

        /// <summary>
        /// 文件位于下载文件中的起始位置
        /// </summary>
        internal Int32 end
        {
            get { return End; }
            set { End = value; }
        }
        /// <summary>
        /// 文件位于下载文件中的结束位置
        /// </summary>
        internal Int32 start
        {
            get { return Start; }
            set { Start = value; }
        }
        /// <summary>
        /// ZIP压缩前文件原长度
        /// </summary>
        internal Int32 or_length
        {
            get { return Or_length; }
            set { Or_length = value; }
        }
        //文件长度
        internal Int32 length
        {
            get { return End - (Start - 1); }
        }


        internal string toString()
        {
            return filename + "|" + Md5 + "|" + Start.ToString() + "|" + End.ToString() + "\r\n";
        }
    }
}
