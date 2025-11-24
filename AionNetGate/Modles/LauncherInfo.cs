
using AionCommons.Network.Packet;
using AionNetGate.Netwok;
using System.Windows.Forms;

namespace AionNetGate.Modles
{
    class LauncherInfo
    {
        private int accountId;
        private int allowLogin;
        private string buzhu;
        private AionConnection con;
        private string hardID;
        private string[] info;
        private ListViewItem item;
        private int playerId;

        public LauncherInfo(AionConnection con)
        {
            this.con = con;
        }

        public override int GetHashCode()
        {
            return con.GetHashCode();
        }

        public void Send(AbstractServerPacket p)
        {
            con.SendPacket(p);
        }

        public int AccountId
        {
            get
            {
                return accountId;
            }
            set
            {
                accountId = value;
            }
        }

        public int AllowLogin
        {
            get
            {
                return allowLogin;
            }
            set
            {
                allowLogin = value;
            }
        }

        public string Buzhu
        {
            get
            {
                return buzhu;
            }
            set
            {
                buzhu = value;
            }
        }

        public AionConnection Connection
        {
            get
            {
                return con;
            }
        }

        public string HardInfo
        {
            get
            {
                return hardID;
            }
            set
            {
                hardID = value;
            }
        }

        public string[] Info
        {
            get
            {
                return this.info;
            }
            set
            {
                info = value;
                hardID = Info[2];
            }
        }

        public ListViewItem listViewItem
        {
            get
            {
                return item;
            }
            set
            {
                item = value;
            }
        }

        public int PlayerId
        {
            get
            {
                return playerId;
            }
            set
            {
                playerId = value;
            }
        }
    }
}
