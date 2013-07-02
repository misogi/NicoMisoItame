using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NicoLogin
{
    public struct MylistInfo
    {
        public string id { get; set; }
        public string user_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string public_flag { get; set; }
        public string default_sort { get; set; }
        public string create_time { get; set; }
        public string update_time { get; set; }
        public string sort_order { get; set; }
        public string icon_id { get; set; }
        public string xmlstr { get; set; }
        public override string ToString()
        {
            return name + " id:" + id + " user:" + user_id;
        }
    }
}
