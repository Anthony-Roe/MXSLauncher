using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MxSimLauncher
{
    class Server
    {
        private string ip;
        private string lastRace;

        public Server(string ip, string lastRace)
        {
            this.ip = ip;
            this.lastRace = lastRace;
        }

        public string IP
        {
            get { return ip; }
            set { ip = value; }
        }

        public string LastRace
        {
            get { return lastRace; }
            set { lastRace = value; }
        }
    }
}
