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
            int len = 10;
            for (int i = px - len; i < px + len; i++)
                for (int j = py - len; j < py + len; j++)
                {
                    string link = @"https://www.crimereports.com/v3/crime_reports/map/search_by_tile.json?start_date=2015/01/04&end_date=2015/02/04&incident_type_ids=100,104,98,103,99,101,170,8,97,148,9,149,150&row=" + j.ToString() + @"&column=" + i.ToString() + @"&zoom=15&include_sex_offenders=true&_=1423084677870";

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
                        result += now + "," + "\r\n";
                    }
                }

        }
    }
}
