using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AionLanucher.Network
{
    #region AbstractClientPacket
    abstract class AbstractClientPacket : BasePacket
    {
        internal AbstractClientPacket()
            : base(PacketType.CLIENT)
        {

        }

        protected byte[] readB(int length)
        {
            byte[] result = new byte[length];
            Read(result, 0, length);
            return result;
        }

        protected byte readC()
        {
            return readB(1)[0];
        }

        protected int readD()
        {
            return BitConverter.ToInt32(readB(4), 0);
        }

        protected double readDF()
        {
            return BitConverter.ToDouble(readB(8), 0);
        }

        protected float readF()
        {
            return BitConverter.ToSingle(readB(4), 0);
        }

        protected short readH()
        {
            return BitConverter.ToInt16(readB(2), 0);
        }

        protected ushort ReadUH()
        {
            return BitConverter.ToUInt16(readB(2), 0);
        }

        protected long readQ()
        {
            return BitConverter.ToInt64(readB(8), 0);
        }

        protected long readL()
        {
            return BitConverter.ToInt64(readB(8), 0);
        }

        protected string readS()
        {
            string result = "";
            // loop the stream until end, will be broken in middle if string end found
            while ((Length - Position) > 2)
            {
                char c = BitConverter.ToChar(readB(2), 0);
                if (c == 0)
                    break;
                else
                    result += c;
            }
            return result;
        }

        protected int RemainingBytes()
        {
            return (int)(Length - Position);
        }

        protected abstract void readImpl();

        protected abstract void runImpl();

        internal void ProcessData()
        {
            readImpl();
            runImpl();
        }
    }
    #endregion

    #region AbstractServerPacket
    abstract class AbstractServerPacket : BasePacket
    {

        /// <param name="opCode"> </param>
        internal AbstractServerPacket()
            : base(PacketType.SERVER)
        {

        }

        internal void writeC(byte b)
        {
            WriteByte(b);
        }
        internal void writeB(byte[] bytes)
        {
            Write(bytes, 0, bytes.Length);
        }

        internal void writeD(int value)
        {
            writeB(BitConverter.GetBytes(value));
        }

        internal void writeH(short value)
        {
            writeB(BitConverter.GetBytes(value));
        }

        public void writeUH(ushort value)
        {
            writeB(BitConverter.GetBytes(value));
        }

        internal void writeQ(long value)
        {
            writeB(BitConverter.GetBytes(value));
        }

        internal void writeF(float value)
        {
            writeB(BitConverter.GetBytes(value));
        }

        internal void WriteName(string str, int length)
        {
            writeS(str);
            byte[] unknown = new byte[length - (str.Length * 2 + 2)]; // What on earth is this nonsense?
            writeB(unknown);
        }

        internal void writeS(string str)
        {
            foreach (char c in str)
            {
                byte[] tmp = BitConverter.GetBytes(c);
                writeB(tmp);
            }
            writeH(0);
        }

        internal int ProcessData(Session con)
        {
            connection = con;
            Position = 0;
           // Opcode = AionPackets.GetServerPacketOpcode(GetType());
            return write();
        }

        internal int write()
        {
            writeD(0);
            writeC((byte)Opcode); // Opcode
            writeImpl(); // Write contents of packet

            int length = (int)Position;

            Position = 0;
            writeD(length);
            Write(ToArray(), 4, length - 4);

            return length;
        }

        /// <param name="cHandler"> </param>
        /// <param name="buf"> </param>
        protected abstract void writeImpl();

    }
    #endregion

    #region BasePacket ,PacketType
    abstract class BasePacket : MemoryStream, IDisposable
    {
        internal PacketType Type;

        internal Session connection;

        internal short Opcode;

        internal BasePacket(short opcode)
        {
            Opcode = opcode;
            Type = PacketType.SERVER;
        }

        internal BasePacket(PacketType direction)
        {
            Type = direction;
        }

        internal Session getConnection()
        {
            return connection;
        }
    }

    internal enum PacketType
    {
        CLIENT, SERVER
    }
    #endregion

    #region Session
    /// <summary> 
    /// 客户端与服务器之间的会话类 By 灰色枫叶 
    /// 
    /// 
    /// 说明: 
    /// 会话类包含远程通讯端的状态,这些状态包括Socket,报文内容, 
    /// 客户端退出的类型(正常关闭,强制退出两种类型) 
    /// </summary> 
    class Session : IDisposable
    {
        #region 字段

        /// <summary>
        /// 是否正在处理CM数据包
        /// </summary>
        internal bool ReadInterestEnabled = false;
        /// <summary>
        /// 是否正在发送SM封包
        /// </summary>
        internal bool WriteInterestEnabled = false;

        internal bool Ready = true;

        /// <summary>
        /// 缓冲区
        /// </summary>
        private ByteBuffer buf;
        /// <summary> 
        /// 会话ID 
        /// </summary> 
        private int _id;
        /// <summary> 
        /// 客户端发送到服务器的报文 
        /// 注意:在有些情况下报文可能只是报文的片断而不完整 
        /// </summary> 
        private byte[] _datagram;
        /// <summary> 
        /// 客户端的Socket 
        /// </summary> 
        private Socket _cliSock;
        /// <summary> 
        /// 客户端的退出类型 
        /// </summary> 
        private ExitType _exitType;
        /// <summary> 
        /// 退出类型枚举 
        /// </summary> 
        internal enum ExitType
        {
            NormalExit,
            ExceptionExit
        }

        private string _ip;

        private string _port;

        #endregion

        #region 属性

        /// <summary> 
        /// 返回会话的ID 
        /// </summary> 
        internal int ID
        {
            get
            {
                return _id;
            }
        }

        /// <summary> 
        /// 存取会话的报文 
        /// </summary> 
        internal byte[] Datagram
        {
            get
            {
                return _datagram;
            }
            set
            {
                _datagram = value;
            }
        }

        /// <summary> 
        /// 获得与客户端会话关联的Socket对象 
        /// </summary> 
        internal Socket ClientSocket
        {
            get
            {
                return _cliSock;
            }
        }

        internal ByteBuffer Buf
        {
            get
            {
                return buf;
            }
        }
        /// <summary> 
        /// 存取客户端的退出方式 
        /// </summary> 
        internal ExitType TypeOfExit
        {
            get
            {
                return _exitType;
            }
            set
            {
                _exitType = value;
            }
        }
        #endregion

        #region 方法

        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="cliSock">会话使用的Socket连接</param> 
        internal Session(Socket cliSock)
        {
            _cliSock = cliSock;
            _id = (int)cliSock.Handle;
            _datagram = new byte[8192];
            buf = new ByteBuffer();
            try
            {
                _ip = ((IPEndPoint)_cliSock.RemoteEndPoint).Address.ToString();
                _port = ((IPEndPoint)_cliSock.RemoteEndPoint).Port.ToString();
            }
            catch
            {

            }

        }

        /// <summary> 
        /// 关闭会话
        /// </summary> 
        internal virtual void Close()
        {
            try
            {
                buf = null;
                _datagram = null;
                //关闭数据的接受和发送 
                if (_cliSock.Connected)
                    _cliSock.Shutdown(SocketShutdown.Both);
                //清理资源 
                _cliSock.Close();
            }
            catch (Exception)
            {

            }
        }

        /// <summary>
        /// 获取远程连接的IP
        /// </summary>
        /// <returns></returns>
        internal virtual string GetIP()
        {
            return _ip;
        }

        /// <summary>
        /// 获取远程连接的端口
        /// </summary>
        /// <returns></returns>
        internal virtual string GetPort()
        {
            return _port;
        }

        /// <summary> 
        /// 返回两个Session是否代表同一个客户端 
        /// </summary> 
        /// <param name="obj"></param> 
        /// <returns></returns> 
        public override bool Equals(object obj)
        {
            Session rightObj = (Session)obj;
            return (int)_cliSock.Handle == (int)rightObj.ClientSocket.Handle;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return (int)_cliSock.Handle;
        }

        /// <summary> 
        /// 重载ToString()方法,返回Session对象的特征 
        /// </summary> 
        /// <returns></returns> 
        public override string ToString()
        {
            string result = string.Format("Session:{0},IP:{1}", _id, _cliSock.RemoteEndPoint.ToString());
            return result;
        }

        #region IDisposable Support
        private bool disposedValue = false; // 若要检测冗余调用

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 处置托管的状态(托管的对象)。
                }

                // TODO: 释放未托管的资源(未托管的对象)并在以下内容中替代终结器。
                // TODO: 将大型字段设置为 null。

                disposedValue = true;
            }
        }

        // TODO: 仅当以上 Dispose(bool disposing) 拥有用于释放未托管资源的代码时才替代终结器。
        // ~Session() {
        //   // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing)中。
        //   Dispose(false);
        // }

        // 添加此代码以正确实现可处置模式。
        public void Dispose()
        {
            // 请勿更改此代码。将清理代码放入以上 Dispose(bool disposing)中。
            Dispose(true);
            // TODO: 如果在以上内容中替代了终结器，则取消注释以下行。
            // GC.SuppressFinalize(this);
        }
        #endregion

        #endregion
    }
    #endregion

    #region NetEventArgs
    /// <summary> 
    /// 网络通讯事件模型委托 
    /// </summary> 
    delegate void NetEvent(object sender, NetEventArgs e);

    class NetEventArgs : EventArgs
    {
        #region 字段
        /// <summary> 
        /// 客户端与服务器之间的会话 
        /// </summary> 
        private Session _client;
        #endregion

        #region 构造函数
        /// <summary> 
        /// 构造函数 
        /// </summary> 
        /// <param name="client">客户端会话</param> 
        internal NetEventArgs(Session client)
        {
            if (null == client)
            {
                throw (new ArgumentNullException());
            }
            _client = client;
        }


        #endregion

        #region 属性
        /// <summary> 
        /// 获得激发该事件的会话对象 
        /// </summary> 
        internal Session Client
        {
            get
            {
                return _client;
            }
        }

        #endregion
    }
    #endregion

    #region ByteBuffer
    class ByteBuffer
    {
        //数组的最大长度 - 增加到64KB以支持大数据包
        private const int MAX_LENGTH = 65536;

        //固定长度的中间数组
        private byte[] TEMP_BYTE_ARRAY;

        //当前数组长度
        private int CURRENT_LENGTH = 0;

        //当前Pop指针位置
        private int CURRENT_POSITION = 0;

        //最后返回数组
        private byte[] RETURN_ARRAY;

        /// <summary>
        /// 默认构造函数
        /// </summary>
        internal ByteBuffer()
        {
            Initialize();
        }

        /// <summary>
        /// 重载的构造函数,用一个Byte数组来构造
        /// </summary>
        /// <param name="bytes">用于构造ByteBuffer的数组</param>
        internal ByteBuffer(byte[] bytes)
        {
            Initialize();
            writeByteArray(bytes);
        }


        /// <summary>
        /// 获取当前ByteBuffer的长度
        /// </summary>
        internal int Length
        {
            get
            {
                return CURRENT_LENGTH;
            }
        }

        /// <summary>
        /// 获取/设置当前出栈指针位置
        /// </summary>
        internal int Position
        {
            get
            {
                return CURRENT_POSITION;
            }
            set
            {
                CURRENT_POSITION = value;
            }
        }

        /// <summary>
        /// 获取ByteBuffer所生成的数组
        /// 长度必须小于 [MAXSIZE]
        /// </summary>
        /// <returns>Byte[]</returns>
        internal byte[] ToByteArray()
        {
            //分配大小
            RETURN_ARRAY = new byte[CURRENT_LENGTH];
            //调整指针
            Array.Copy(TEMP_BYTE_ARRAY, 0, RETURN_ARRAY, 0, CURRENT_LENGTH);
            return RETURN_ARRAY;
        }

        /// <summary>
        /// 初始化ByteBuffer的每一个元素,并把当前指针指向头一位
        /// </summary>
        internal void Initialize()
        {
            TEMP_BYTE_ARRAY = new byte[MAX_LENGTH];
            TEMP_BYTE_ARRAY.Initialize();
            CURRENT_LENGTH = 0;
            CURRENT_POSITION = 0;
        }

        /// <summary>
        /// 向ByteBuffer压入一个字节
        /// </summary>
        /// <param name="by">一位字节</param>
        internal void writeC(byte by)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = by;
        }

        /// <summary>
        /// 向ByteBuffer压入数组
        /// </summary>
        /// <param name="ByteArray">数组</param>
        internal void writeByteArray(byte[] ByteArray)
        {
            if (TEMP_BYTE_ARRAY == null)
            {
                Initialize();
            }
            //把自己CopyTo目标数组
            ByteArray.CopyTo(TEMP_BYTE_ARRAY, CURRENT_LENGTH);
            //调整长度
            CURRENT_LENGTH += ByteArray.Length;
        }

        /// <summary>
        /// 向ByteBuffer压入数组
        /// </summary>
        /// <param name="ByteArray">数组</param>
        internal void writeByteArray(byte[] ByteArray, int length)
        {
            if (TEMP_BYTE_ARRAY == null)
            {
                Initialize();
            }
            //把自己CopyTo目标数组
            Array.Copy(ByteArray, 0, TEMP_BYTE_ARRAY, CURRENT_LENGTH, length);
            //调整长度
            CURRENT_LENGTH += length;
        }
        /// <summary>
        /// 向ByteBuffer压入两字节的Short
        /// </summary>
        /// <param name="Num">2字节Short</param>
        internal void writeH(ushort Num)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0xff00) >> 8) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)((Num & 0x00ff) & 0xff);
        }

        /// <summary>
        /// 向ByteBuffer压入一个无符Int值
        /// </summary>
        /// <param name="Num">4字节UInt32</param>
        internal void writeD(uint Num)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0xff000000) >> 24) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x00ff0000) >> 16) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x0000ff00) >> 8) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)((Num & 0x000000ff) & 0xff);
        }

        /// <summary>
        /// 向ByteBuffer压入一个Long值
        /// </summary>
        /// <param name="Num">4字节Long</param>
        internal void writeQ(long Num)
        {
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0xff000000) >> 24) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x00ff0000) >> 16) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)(((Num & 0x0000ff00) >> 8) & 0xff);
            TEMP_BYTE_ARRAY[CURRENT_LENGTH++] = (byte)((Num & 0x000000ff) & 0xff);
        }

        /// <summary>
        /// 从ByteBuffer的当前位置弹出一个Byte,并提升一位
        /// </summary>
        /// <returns>1字节Byte</returns>
        internal byte readC()
        {
            byte ret = TEMP_BYTE_ARRAY[CURRENT_POSITION++];
            return ret;
        }

        /// <summary>
        /// 从ByteBuffer的当前位置弹出一个Short,并提升两位
        /// </summary>
        /// <returns>2字节Short</returns>
        internal ushort readH()
        {
            //溢出
            if (CURRENT_POSITION + 1 >= CURRENT_LENGTH)
            {
                return 0;
            }
            ushort ret = (ushort)(TEMP_BYTE_ARRAY[CURRENT_POSITION] << 8 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 1]);
            CURRENT_POSITION += 2;
            return ret;
        }

        /// <summary>
        /// 从ByteBuffer的当前位置弹出一个uint,并提升4位
        /// </summary>
        /// <returns>4字节UInt</returns>
        internal uint readD()
        {
            if (CURRENT_POSITION + 3 >= CURRENT_LENGTH)
                return 0;
            uint ret = (uint)(TEMP_BYTE_ARRAY[CURRENT_POSITION] << 24 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 1] << 16 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 2] << 8 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 3]);
            CURRENT_POSITION += 4;
            return ret;
        }

        /// <summary>
        /// 从ByteBuffer的当前位置弹出一个long,并提升4位
        /// </summary>
        /// <returns>4字节Long</returns>
        internal long PopLong()
        {
            if (CURRENT_POSITION + 3 >= CURRENT_LENGTH)
                return 0;
            long ret = (TEMP_BYTE_ARRAY[CURRENT_POSITION] << 24 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 1] << 16 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 2] << 8 | TEMP_BYTE_ARRAY[CURRENT_POSITION + 3]);
            CURRENT_POSITION += 4;
            return ret;
        }

        /// <summary>
        /// 从ByteBuffer的当前位置弹出长度为Length的Byte数组,提升Length位
        /// </summary>
        /// <param name="Length">数组长度</param>
        /// <returns>Length长度的byte数组</returns>
        internal byte[] readByteArray(int Length)
        {
            //溢出
            if (CURRENT_POSITION + Length > CURRENT_LENGTH)
            {
                return new byte[0];
            }
            byte[] ret = new byte[Length];
            Array.Copy(TEMP_BYTE_ARRAY, CURRENT_POSITION, ret, 0, Length);
            //提升位置
            CURRENT_POSITION += Length;
            return ret;
        }
    }
    #endregion
}
