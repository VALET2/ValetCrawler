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
        public bool isRun;
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

        public void SetRun_uiLock(bool b)
        {
            if (b)
            {

            }
            else
            {

            }
        }

        private void CrawlingStart(object sender, RoutedEventArgs e)
        {

        }

        private void Run()
        {
            new Thread(new ThreadStart(() =>
            {
                isRun = true;
                SetRun_uiLock(isRun);

                //Sql Connect
                MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection("Server=14.63.175.76;Database=valet2;Uid=root;Pwd=valet2;");
                connection.Open();

                string result = "";

                int start_col = int.Parse(StartCol_TB.Text);
                int end_col = int.Parse(EndCol_TB.Text);
                int start_row = int.Parse(StartRow_TB.Text);
                int end_row = int.Parse(EndRow_TB.Text);
                int zoomlv = int.Parse(ZoomLv_TB.Text);

                DateTime startdate = StartDate_DP.SelectedDate.Value;
                DateTime enddate = StartDate_DP.SelectedDate.Value;
                string startt = startdate.ToString();

                //Get Data
                for (int c = start_col; c <= end_col; c++)
                {
                    for (int r = start_row; r < end_row; r++)
                    {
                        //Link Open, Get Result
                        string link = @"https://www.crimereports.com/v3/crime_reports/map/search_by_tile.json?start_date=2014/01/01&end_date=2015/02/04&incident_type_ids=100,104,98,103,99,101,170,8,97,148,9,149,150&row=" + r.ToString() + @"&column=" + c.ToString() + @"&zoom=" + zoomlv.ToString() + "&include_sex_offenders=true&_=1423084677870";
                        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(link);
                        HttpWebResponse oGetResponse = (HttpWebResponse)oRequest.GetResponse();
                        StreamReader oStreamReader = new StreamReader(oGetResponse.GetResponseStream());
                        string now = oStreamReader.ReadToEnd();

                        //if Empty Result
                        if (now == "{\"crimes\":[],\"offenders\":[]}" || now == "")
                        {
                            //Skip
                            //System.Diagnostics.Debug.Print(i.ToString() + " / " + j.ToString());
                        }
                        else
                        {
                            //Add
                            if (result != "")
                                result += "\r\n," + now;
                            else
                                result += now;
                        }
                    }
                }

                //Parse "result" Date -> Json
                dynamic jsonData = JsonConvert.DeserializeObject("{ \"datas\" : [" + result + "] }");
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
                        if (SI_Crime_CB.IsChecked.Value)
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
                        if (SI_Offender_CB.IsChecked.Value)
                        {
                            string sqlvalue = string.Format("\"{0}\", {1}, {2}, \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", {13}, \"{14}\", \"{15}\"", id, lat, lng, address, city, state, zip, name, photoUrl, height, weight, eyeColor, hairColor, age, race, sex);
                            MySqlHelper.ExecuteNonQuery(connection, "INSERT INTO sex_offenders (`identifier`, `latitude`, `longitude`, `address`, `city`, `state`, `zip`, `name`, `photourl`, `height`, `weight`, `eyecolor`, `haircolor`, `age`, `race`, `sex` ) VALUE (" + sqlvalue + ");");
                        }
                    }

                }

                isRun = false;
                SetRun_uiLock(isRun);
            })).Start();

        }

        private void OpenReferWeb(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("http://www.maptiler.org/google-maps-coordinates-tile-bounds-projection/");
            }
            catch { }
        }

    }
}
