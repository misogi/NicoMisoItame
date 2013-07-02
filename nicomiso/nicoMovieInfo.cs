using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Text.RegularExpressions;
using System.Xml;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Data;
using System.Windows.Media;

namespace nicomiso
{
    class nicoMovieInfo
    {
        public string title { get; set; }
        public string link { get; set; }
        public string desc { get; set; }
        public string date_str { get; set; }
        public string info_str { get; set; }
        public string view_str { get; set; }
        public string resstr { get; set; }
        public string myliststr { get; set; }
        public string pts { get; set; }
        public string tagstr { get; set; }
        public string thambnail_url { get; set; }
        public DateTime date { get; set; }
        public int view_num {get; set;}
        public int com_num {get; set;}
        public int mylist_num {get; set;}
        public BitmapImage thumbnail {get; set;}
        public void GetMovieInfoDetail(string link)
        {
            Regex urlreg = new Regex("sm[0-9]+");
            Match m = urlreg.Match( link );
            tagstr = "";
            string str;
            if (m.Success)
            {
                string urlstr = "http://ext.nicovideo.jp/api/getthumbinfo/" + m.Value;
                XmlTextReader reader = new XmlTextReader(urlstr);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                    {
                        continue;
                    }
                    if (reader.LocalName.Equals("view_counter"))
                    {
                        view_num = int.Parse( reader.ReadString() );
                    }
                    else if (reader.LocalName.Equals("comment_num"))
                    {
                        com_num = int.Parse( reader.ReadString() );
                    }
                    else if (reader.LocalName.Equals("mylist_counter"))
                    {
                        mylist_num = int.Parse( reader.ReadString() );
                    }
                    else if (reader.LocalName.Equals("title"))
                    {
                        title = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("link"))
                    {
                        link = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("tag"))
                    {
                        tagstr = tagstr + " " + reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("strong"))
                    {
                        str = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("first_retrieve"))
                    {
                        str = reader.ReadString();
                        try
                        {
                            date = DateTime.ParseExact(str, "yyyy-MM-ddTHH:mm:ssK", null);
                        }catch(FormatException ){
                            Console.WriteLine("パース失敗 : " + str);
                        }
                    }
                    
                }
            }
        }

    }
    class searchInfo
    {
        public string sortword { get; set; }
        public string sortquery { get; set; }
    }

    class colConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;

            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            nicoMovieInfo[] mvinfo = listView.ItemsSource as nicoMovieInfo[];
            // Get the index of a ListViewItem
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);

            TimeSpan ts = DateTime.Now - mvinfo[index].date;
            if (ts.TotalHours <= 24)
            {
                return Brushes.LightCoral;
            }
            else if (ts.TotalDays < 3)
            {
                return Brushes.LightSalmon;
            }
            else if (ts.TotalDays < 7)
            {
                return Brushes.Khaki;
            }
            else if (ts.TotalDays < 30)
            {
                return Brushes.LightGreen;
            }
            else if (ts.TotalDays < 365)
            {
                return Brushes.White;
            }
            else
            {
                return Brushes.WhiteSmoke;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class FontColConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;

            ListView listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            nicoMovieInfo[] mvinfo = listView.ItemsSource as nicoMovieInfo[];
            // Get the index of a ListViewItem
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);

            TimeSpan ts = DateTime.Now - mvinfo[index].date;
            if (ts.TotalHours <= 24)
            {
                return Brushes.Red;
            }
            else if (ts.TotalDays < 3)
            {
                return Brushes.DarkRed;
            }
            else if (ts.TotalDays < 7)
            {
                return Brushes.DarkRed;
            }
            else if (ts.TotalDays < 30)
            {
                return Brushes.DarkGreen;
            }
            else if (ts.TotalDays < 365)
            {
                return Brushes.Gray;
            }
            else
            {
                return Brushes.Gray;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
