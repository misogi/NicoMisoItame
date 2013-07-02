using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Net;
using System.Data.SQLite;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NicoLogin
{
    public class CookieChecker
    {
        private string _filepath;
        private string _session_id;
        public MylistInfo[] _mylistgroup;

        public void GetCookieChrome()
        {

            string strbase = System.Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            _filepath = strbase + "\\Google\\Chrome\\User Data\\Default\\Cookies";
            string temp = Path.GetTempFileName();
            File.Copy(_filepath, temp, true);
            SQLiteConnection conn = new SQLiteConnection("Data Source=" + temp);
            conn.Open();

            SQLiteCommand command = conn.CreateCommand();
            command.Connection = conn;
            command.CommandText = "SELECT value, name, host_key, path, expires_utc  FROM cookies WHERE host_key = '.nicovideo.jp' AND name = 'user_session';";
            SQLiteDataReader sdr = command.ExecuteReader();

            while (sdr.Read())
            {

                for (int i = 0; i < sdr.FieldCount; i++)
                {
                    //Console.WriteLine(sdr[i] + " " + i);
                }
                _session_id = sdr[0].ToString();
            }
            conn.Close();

        }

        public void GetMylistGroup()
        {

            //HTTP Web リクエストの作成
            string url = "http://www.nicovideo.jp/api/mylistgroup/list";
            string repstr = AccessHTTP(url);
            if (repstr == null) {
                return;
            }
            JObject js = JObject.Parse(repstr);
            JArray mylist = js["mylistgroup"] as JArray;

            //Console.WriteLine(mylist[0]);
            int mylistcount = mylist.Count;
            _mylistgroup = new MylistInfo[mylistcount];
            for (int i = 0; i < mylistcount; i++)
            {
                JToken m = mylist[i];
                _mylistgroup[i] = JsonConvert.DeserializeObject<MylistInfo>(m.ToString());
                _mylistgroup[i].public_flag = m.Value<string>("public");
            }
        }
        public void GetAllMylist()
        {
            for (int i = 0; i < _mylistgroup.Count(); i++)
            {
                string url = "http://www.nicovideo.jp/mylist/" + _mylistgroup[i].id + "?rss=2.0";
                _mylistgroup[i].xmlstr = AccessHTTP(url);
            }
        }

        private string AccessHTTP(string url){ 
            string repstr = null;
            if (_session_id != null)
            {
                HttpWebRequest wreq = (HttpWebRequest)WebRequest.Create(url);
                CookieContainer cc = new CookieContainer();
                wreq.CookieContainer = cc;
                Cookie cc_login = new Cookie("user_session", _session_id);
                cc_login.Domain = "nicovideo.jp";
                wreq.CookieContainer.Add(cc_login);

                WebResponse wrep = wreq.GetResponse();
                Stream s = wrep.GetResponseStream();
                StreamReader sr = new StreamReader(s);
                repstr = "";
                while (!sr.EndOfStream)
                {
                    repstr += sr.ReadLine();
                }
            }
            else
            {
                Console.WriteLine("セッション情報がありません");
            }
            return repstr;
        }
    }
}
