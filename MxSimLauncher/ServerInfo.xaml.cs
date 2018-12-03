using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MxSimLauncher
{
    /// <summary>
    /// Interaction logic for ServerInfo.xaml
    /// </summary>
    public partial class ServerInfo : Window
    {
        public ServerInfo()
        {
            InitializeComponent();
        }

        public void SetStuffUp(string ip = "", string lastRace = "", string playerCount = "0")
        {
            serverIP.Text = ip;
            LastRace.Text = lastRace;
            PlayerCount.Text = playerCount;
        }

        private void Info_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }
    }
}
