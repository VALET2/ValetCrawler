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
            string result = "";
            int px = 8472;
            int py = 12354;
            int len = 3;
            for (int i = px - len; i < px + len; i++)
            {
                for (int j = py - len; j < py + len; j++)
                {
                    string link = @"https://www.crimereports.com/v3/crime_reports/map/search_by_tile.json?start_date=2014/01/04&end_date=2015/02/04&incident_type_ids=100,104,98,103,99,101,170,8,97,148,9,149,150&row=" + j.ToString() + @"&column=" + i.ToString() + @"&zoom=15&include_sex_offenders=true&_=1423084677870";

                    HttpWebRequest oRequest = (HttpWebRequest)WebRequest.Create(link);
                    HttpWebResponse oGetResponse = (HttpWebResponse)oRequest.GetResponse();
                    StreamReader oStreamReader = new StreamReader(oGetResponse.GetResponseStream());

                    string now = oStreamReader.ReadToEnd();
                    if (now == "{\"crimes\":[],\"offenders\":[]}")
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
                    string identifi = crime["ccn"];
                    string date = crime["incident_date_time"];
                    int policeId = crime["org_id"];
                    string policeName = crime["org_name"];
                    int type = crime["incident_type_id"];
                    string address = crime["address"];
                    string description = crime["description"];
                    double lng = crime["lng"];
                    double lat = crime["lat"];
                }
                foreach (dynamic offender in data["offenders"])
                {
                    string address = offender["address"];
                    string city = offender["city"];
                    string state = offender["state"];
                    string zip = offender["zip"];
                    string name = offender["name"];
                    string photoUrl = offender["photoUrl"];
                    double lng = offender["lng"];
                    double lat = offender["lat"];
                    string race = offender["race"];
                    string sex = offender["sex"];
                    string height = offender["height"];
                    string weight = offender["weight"];
                    string eyeColor = offender["eyeColor"];
                    string hairColor = offender["hairColor"];
                    int age = offender["age"];
                }


            }

        }
    }
}
