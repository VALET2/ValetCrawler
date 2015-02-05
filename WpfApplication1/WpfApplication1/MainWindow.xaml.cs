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

namespace WpfApplication1
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();


            new Thread(new ThreadStart(() =>
            {
                MySql.Data.MySqlClient.MySqlConnection connection = new MySql.Data.MySqlClient.MySqlConnection("Server=14.63.175.76;Database=valet2;Uid=root;Pwd=valet2;");
                connection.Open();

                string result = "";
                int px = 8472;
                int py = 12354;
                int len = 50;

                for (int i = px - len; i < px + len; i++)
                {
                    for (int j = py - len; j < py + len; j++)
                    {
                        string link = @"https://www.crimereports.com/v3/crime_reports/map/search_by_tile.json?start_date=2014/01/01&end_date=2015/02/04&incident_type_ids=100,104,98,103,99,101,170,8,97,148,9,149,150&row=" + j.ToString() + @"&column=" + i.ToString() + @"&zoom=15&include_sex_offenders=true&_=1423084677870";

                        HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(link);
                        HttpWebResponse oGetResponse = (HttpWebResponse)oRequest.GetResponse();
                        StreamReader oStreamReader = new StreamReader(oGetResponse.GetResponseStream());

                        string now = oStreamReader.ReadToEnd();
                        if (now == "{\"crimes\":[],\"offenders\":[]}" || now == "")
                        {
                            System.Diagnostics.Debug.Print(i.ToString() + " / " + j.ToString());
                        }
                        else
                        {
                            if (result != "")
                                result += "\r\n," + now;
                            else
                                result += now;
                        }
                    }
                }
                dynamic jsonData = JsonConvert.DeserializeObject("{ \"datas\" : [" + result + "] }");
                foreach (dynamic data in jsonData["datas"])
                {
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

                        string sqlvalue = string.Format(@"'{0}', {1}, {2}, {3}, {4}, '{5}', '{6}', '{7}', '{8}'", identifi, lat, lng, type, policeId, policeName, address, date, description);
                        MySqlHelper.ExecuteNonQuery(connection, "INSERT INTO crimes (`identifier`, `latitude`, `longitude`, `type`, `police_id`, `police_name`, `address`, `date`, `description` ) VALUES (" + sqlvalue + ");");
                    }
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

                        string sqlvalue = string.Format("\"{0}\", {1}, {2}, \"{3}\", \"{4}\", \"{5}\", \"{6}\", \"{7}\", \"{8}\", \"{9}\", \"{10}\", \"{11}\", \"{12}\", {13}, \"{14}\", \"{15}\"", id, lat, lng, address, city, state, zip, name, photoUrl, height, weight, eyeColor, hairColor, age, race, sex);
                        MySqlHelper.ExecuteNonQuery(connection, "INSERT INTO sex_offenders (`identifier`, `latitude`, `longitude`, `address`, `city`, `state`, `zip`, `name`, `photourl`, `height`, `weight`, `eyecolor`, `haircolor`, `age`, `race`, `sex` ) VALUE (" + sqlvalue + ");");
                    }

                }
            })).Start();


        }
    }
}
