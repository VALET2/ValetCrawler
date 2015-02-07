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
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.IO;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MySql.Data.MySqlClient;

using System.Threading;

namespace Valet2Crawler
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isRun;
        public System.Timers.Timer timer;
        public DateTime StartTime;
        Thread th;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void NumberOnly_KeyDown(object sender, KeyEventArgs e)
        {
            if (Char.IsDigit((Char)KeyInterop.VirtualKeyFromKey(e.Key)))
            {

            }
            else
            {
                e.Handled = true;
            }
        }

        public void SetRun_Func(bool b)
        {
            isRun = b;
            if (isRun)
            {
                //현재시간저장
                StartTime = DateTime.Now;

                //타이머시작
                timer = new System.Timers.Timer();
                timer.Elapsed += one_Timer_Elapsed;
                timer.Interval = 1000;
                timer.Start();

                UISetable(false);

                //ui init

                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    view_rowTB.Text = "00000/00000";
                    view_colTB.Text = "00000/00000";

                    view_perTB.Text = "0.00%";

                    pgbar.Value = 0;
                }));
            }
            else
            {
                //타이머종료
                timer.Stop();
                timer.Dispose();

                UISetable(true);
            }
        }
        private void UISetable(bool b)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
            {
                StartDate_DP.IsEnabled = b;
                EndDate_DP.IsEnabled = b;
                StartCol_TB.IsEnabled = b;
                EndCol_TB.IsEnabled = b;
                StartRow_TB.IsEnabled = b;
                EndRow_TB.IsEnabled = b;
                ZoomLv_TB.IsEnabled = b;
                RunButton.IsEnabled = b;
                SI_Crime_CB.IsEnabled = b;
                SI_Offender_CB.IsEnabled = b;
            }));
        }

        void one_Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
            {
                TimeSpan time = DateTime.Now - StartTime;
                view_timeTB.Text = time.Hours + ":" + time.Minutes + ":" + time.Seconds;
            }));

        }

        private void CrawlingStart(object sender, RoutedEventArgs e)
        {
            Run();
        }

        private void Run()
        {
            th = new Thread(new ThreadStart(() =>
            {
                if (isRun)
                    return;

                SetRun_Func(true);

                //Sql Connect
                MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection("Server=14.63.175.76;Database=valet2;Uid=root;Pwd=valet2;");
                connection.Open();

                int start_col = 0;
                int end_col = 0;
                int start_row = 0;
                int end_row = 0;
                int zoomlv = 0;
                DateTime startdate;
                DateTime enddate;
                string startt = "";
                bool InsertCrimes = false;
                bool InsertOffenders = false;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Send, new Action(delegate
                {
                    start_col = int.Parse(StartCol_TB.Text);
                    end_col = int.Parse(EndCol_TB.Text);
                    start_row = int.Parse(StartRow_TB.Text);
                    end_row = int.Parse(EndRow_TB.Text);
                    zoomlv = int.Parse(ZoomLv_TB.Text);

                    startdate = StartDate_DP.SelectedDate.Value;
                    enddate = StartDate_DP.SelectedDate.Value;
                    startt = startdate.ToString();

                    InsertCrimes = SI_Crime_CB.IsChecked.Value;
                    InsertOffenders = SI_Offender_CB.IsChecked.Value;
                }));

                int maxcount = (end_col - start_col + 1) * (end_row - start_row + 1);
                //Get Data
                for (int c = start_col; c <= end_col; c++)
                {
                    for (int r = start_row; r <= end_row; r++)
                    {
                        //Link Open, Get Result
                        string link = @"https://www.crimereports.com/v3/crime_reports/map/search_by_tile.json?start_date=2014/01/01&end_date=2015/02/04&incident_type_ids=100,104,98,103,99,101,170,8,97,148,9,149,150&row=" + r.ToString() + @"&column=" + c.ToString() + @"&zoom=" + zoomlv.ToString() + "&include_sex_offenders=true&_=1423084677870";
                        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(link);
                        HttpWebResponse oGetResponse = (HttpWebResponse)oRequest.GetResponse();
                        StreamReader oStreamReader = new StreamReader(oGetResponse.GetResponseStream());
                        string webdata = oStreamReader.ReadToEnd();

                        //if Empty Result
                        if (webdata == "{\"crimes\":[],\"offenders\":[]}" || webdata == "")
                        {
                            //Skip
                        }
                        else
                        {
                            #region Parse "result" Date -> Json
                            dynamic jsonData = JsonConvert.DeserializeObject("{ \"datas\" : [" + webdata + "] }");
                            //In Datas
                            foreach (dynamic data in jsonData["datas"])
                            {
                                //in Crimes
                                foreach (dynamic crime in data["crimes"])
                                {
                                    string identifi = MySqlHelper.EscapeString(crime["ccn"].ToString());
                                    double lat = crime["lat"];
                                    double lng = crime["lng"];
                                    int type = crime["incident_type_id"];
                                    int policeId = crime["org_id"];
                                    string policeName = MySqlHelper.EscapeString(crime["org_name"].ToString());
                                    string address = MySqlHelper.EscapeString(crime["address"].ToString());
                                    string date = crime["incident_date_time"].ToString("yyyy-MM-dd HH:mm:ss");
                                    string description = MySqlHelper.EscapeString(crime["description"].ToString());

                                    //Json
                                    if (InsertCrimes)
                                    {
                                        string sqlvalue = string.Format(@"'{0}', {1}, {2}, {3}, {4}, '{5}', '{6}', '{7}', '{8}'", identifi, lat, lng, type, policeId, policeName, address, date, description);
                                        MySqlHelper.ExecuteNonQuery(connection, "INSERT INTO crimes (`identifier`, `latitude`, `longitude`, `type`, `police_id`, `police_name`, `address`, `date`, `description` ) VALUES (" + sqlvalue + ");");
                                    }
                                }

                                //in Offenders
                                foreach (dynamic offender in data["offenders"])
                                {
                                    string id = offender["id"];
                                    double lat = offender["lat"];
                                    double lng = offender["lng"];
                                    string address = MySqlHelper.EscapeString(offender["address"].ToString());
                                    string city = MySqlHelper.EscapeString(offender["city"].ToString());
                                    string state = MySqlHelper.EscapeString(offender["state"].ToString());
                                    string zip = MySqlHelper.EscapeString(offender["zip"].ToString());
                                    string name = MySqlHelper.EscapeString(offender["name"].ToString());
                                    string photoUrl = MySqlHelper.EscapeString(offender["photoUrl"].ToString());
                                    string height = MySqlHelper.EscapeString(offender["height"].ToString());
                                    string weight = MySqlHelper.EscapeString(offender["weight"].ToString());
                                    string eyeColor = MySqlHelper.EscapeString(offender["eyeColor"].ToString());
                                    string hairColor = MySqlHelper.EscapeString(offender["hairColor"].ToString());
                                    int age = offender["age"];
                                    string race = MySqlHelper.EscapeString(offender["race"].ToString());
                                    string sex = MySqlHelper.EscapeString(offender["sex"].ToString());

                                    //Json
                                    if (InsertOffenders)
                                    {
                                        string sqlvalue = string.Format("\"{0}\", {1}, {2}, \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", {13}, \"{14}\", \"{15}\"", id, lat, lng, address, city, state, zip, name, photoUrl, height, weight, eyeColor, hairColor, age, race, sex);
                                        MySqlHelper.ExecuteNonQuery(connection, "INSERT INTO sex_offenders (`identifier`, `latitude`, `longitude`, `address`, `city`, `state`, `zip`, `name`, `photourl`, `height`, `weight`, `eyecolor`, `haircolor`, `age`, `race`, `sex` ) VALUE (" + sqlvalue + ");");
                                    }
                                }

                            }//In Datas (root)
                            #endregion

                        }

                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate
                        {
                            view_rowTB.Text = r.ToString("00000") + "/" + end_row.ToString("00000");
                            view_colTB.Text = c.ToString("00000") + "/" + end_col.ToString("00000");

                            int count = ((c - start_col) * (end_row - start_row + 1) + (r - start_row + 1));
                            double per = (double)count / (double)maxcount;
                            view_perTB.Text = (per * 100).ToString("F2") + "%";

                            pgbar.Value = (per * 100);
                        }));
                    }
                }

                if(MessageBox.Show("Done!","Valer2 Crawler") != MessageBoxResult.None)
                {
                    SetRun_Func(false);
                }
            }));
            th.Start();

        }

        private void OpenReferWeb(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/");
            }
            catch { }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (isRun)
            {
                if (MessageBox.Show("The Crawler is Running\nAre you sure?", "Valer2 Crawler - Warring", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    if (th != null)
                        th.Abort();
                    System.Diagnostics.Process.GetCurrentProcess().Close();
                }
                else
                    e.Cancel = true;
            }
        }

    }
}
