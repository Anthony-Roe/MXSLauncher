using System;
using System.Windows;
using System.Net.Http;
using HtmlAgilityPack;
using System.Diagnostics;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace MxSimLauncher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ServerInfo info = new ServerInfo();
        Server currentServer = new Server();
        HttpClient client = new HttpClient();
        bool isLoading = false;
        public MainWindow()
        {
            InitializeComponent();
        }

        void SetupGrid()
        {
            serverGrid.Items.Clear();
            RefreshServers("https://mxsimulator.com/servers");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SetupGrid();
        }

        public class Server
        {
            public string ip { get; set; }
            public string lastRace { get; set; }
            public string playerNum { get; set; }
        }

        public string timeSince(DateTime date)
        {
            long dt = DateTime.Now.Ticks;
            TimeSpan ts = DateTime.Now.Subtract(date);
            if (ts.TotalMinutes < 1)
                return (int)ts.TotalSeconds + " S. ago";
            else if (ts.TotalHours < 1)
                return (int)ts.TotalMinutes + " M. ago";
            else if (ts.TotalDays < 1)
                return (int)ts.TotalHours + " H. ago";
            else
                return (int)ts.TotalDays + " D. ago";
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public async void RefreshServers(string url)
        {
            double amountLoaded = 0;
            isLoading = true;
            using (HttpResponseMessage response = await client.GetAsync(url))
            {
                using (HttpContent content = response.Content)
                {
                    Console.WriteLine("Figuring out response");
                    string webContent = await content.ReadAsStringAsync();
                        
                    int start = webContent.IndexOf("<TABLE CLASS=\"laptimes\">");
                    int end = webContent.IndexOf("</TABLE>", start);
                    string table = webContent.Substring(start, end - start + 8);

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(table);

                    var htmlNodes = htmlDoc.DocumentNode.SelectNodes("//tr");

                    foreach (var node in htmlNodes)
                    {
                        if (amountLoaded >= Math.Floor(serverCount.Value) + 1) break;
                        amountLoaded++;

                        var ip = node.FirstChild.InnerText.ToString(); // Table Row > 
                        if (ip.ToUpper() != "SERVER")
                        {
                            var lastRaceOutput = node.ChildNodes[1].InnerText.ToString();

                            int indexOfOpen = lastRaceOutput.IndexOf("<!--");
                            int indexOfClose = lastRaceOutput.IndexOf("how_long_ago(", indexOfOpen + 1);


                            string results = lastRaceOutput.Substring(0, indexOfOpen) + lastRaceOutput.Substring(indexOfClose + 13);

                            int indexOfOpenSecond = results.IndexOf("))//-->");
                            int indexOfCloseSecond = results.IndexOf("UTC", indexOfOpenSecond + 1);

                            string resultsSecond = results.Substring(0, indexOfOpenSecond) + results.Substring(indexOfCloseSecond + 3);
                            var timePart = timeSince(UnixTimeStampToDateTime(Convert.ToDouble(resultsSecond.Substring(0, 11))));
                            var lastRace = timePart + " " + resultsSecond.Substring(8 + 6);

                            using (HttpResponseMessage sResponse = await client.GetAsync("https://mxsimulator.com/servers/" + ip))
                            {
                                using (HttpContent sContent = sResponse.Content)
                                {
                                    string sWebContent = await sContent.ReadAsStringAsync();
                                    int sStart = sWebContent.IndexOf("<DIV CLASS=\"main\">");
                                    int sEnd = sWebContent.IndexOf("</DIV>", sStart);
                                    string sTable = sWebContent.Substring(sStart, sEnd - sStart + 6);

                                    var sHtmlDoc = new HtmlDocument();
                                    sHtmlDoc.LoadHtml(sTable);
                                    var sHtmlNodes = sHtmlDoc.DocumentNode.SelectNodes("//table");
                                    var LastRace = sHtmlNodes[0].ChildNodes[1].InnerHtml;
                                    int lStart = LastRace.LastIndexOf("<td>");
                                    int lEnd = LastRace.IndexOf("</td>", lStart);
                                    string lTable = LastRace.Substring(lStart, lEnd - lStart + 5);
                                    int rStart = lTable.LastIndexOf("(");
                                    int rEnd = lTable.IndexOf(" riders)", rStart);
                                    string rTable = lTable.Substring(rStart + 1, rEnd - rStart);
                                    serverGrid.Items.Add(new Server { ip = ip, lastRace = lastRace, playerNum = rTable });
                                }
                            }
                        }
                            
                    }

                }
            }
            isLoading = false;
        }

        private void Join_Server(object sender, RoutedEventArgs e)
        {
            if (currentServerIP.Text != "")
            {
                Process.Start("mxsimulator:" + currentServer.ip);
                WindowState = WindowState.Minimized;
            }
        }

        private void Refresh_List(object sender, RoutedEventArgs e)
        {
            SetupGrid();
        }

        private void Server_Info(object sender, RoutedEventArgs e)
        {
            if (info.IsVisible)
            { 
                info.Hide();
            } else {
                info.SetStuffUp(currentServer.ip, currentServer.lastRace, currentServer.playerNum);
                info.Show();
            }
            

        }

        private void serverGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

            if (serverGrid.SelectedItems.Count > 0)
            {
                var grid = sender as DataGrid;
                var selected = grid.SelectedItems;
                var rowIndex = serverGrid.SelectedCells[0].Item;
                var server = selected[0] as Server;
                currentServerIP.Text = server.ip;
                currentServer.ip = server.ip;
                currentServer.lastRace = server.lastRace;
                currentServer.playerNum = server.playerNum;
                info.SetStuffUp(currentServer.ip, currentServer.lastRace, currentServer.playerNum);
            }
        }

        private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (isLoading) return;
            DragMove();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            info.Close();
        }

        private void currentServerIP_TextChanged(object sender, TextChangedEventArgs e)
        {
            currentServer.ip = currentServerIP.Text;
            info.SetStuffUp(currentServer.ip);
        }

        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Console.WriteLine(Math.Floor(serverCount.Value));

        }

        private void serverCount_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            coun.Content = Math.Floor(serverCount.Value).ToString();
        }
    }
}
