// --------------------------------------------------------------------------------------------------------------------
// <copyright file="nicoMovieInfo.cs" company="">
//   
// </copyright>
// <summary>
//   The nico movie info.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace nicomiso
{
    using System;
    using System.Text.RegularExpressions;
    using System.Windows.Media.Imaging;
    using System.Xml;

    /// <summary>
    /// The nico movie info.
    /// </summary>
    internal class NicoMovieInfo
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the com_num.
        /// </summary>
        public int com_num { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime date { get; set; }

        /// <summary>
        /// Gets or sets the date_str.
        /// </summary>
        public string date_str { get; set; }

        /// <summary>
        /// Gets or sets the desc.
        /// </summary>
        public string desc { get; set; }

        /// <summary>
        /// Gets or sets the info_str.
        /// </summary>
        public string info_str { get; set; }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        public string link { get; set; }

        /// <summary>
        /// Gets or sets the mylist_num.
        /// </summary>
        public int mylist_num { get; set; }

        /// <summary>
        /// Gets or sets the myliststr.
        /// </summary>
        public string myliststr { get; set; }

        /// <summary>
        /// Gets or sets the pts.
        /// </summary>
        public string pts { get; set; }

        /// <summary>
        /// Gets or sets the resstr.
        /// </summary>
        public string resstr { get; set; }

        /// <summary>
        /// Gets or sets the tagstr.
        /// </summary>
        public string tagstr { get; set; }

        /// <summary>
        /// Gets or sets the thambnail_url.
        /// </summary>
        public string thambnail_url { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail.
        /// </summary>
        public BitmapImage thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// Gets or sets the view_num.
        /// </summary>
        public int view_num { get; set; }

        /// <summary>
        /// Gets or sets the view_str.
        /// </summary>
        public string view_str { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get movie info detail.
        /// </summary>
        /// <param name="link">
        /// The link.
        /// </param>
        public void GetMovieInfoDetail(string link)
        {
            var urlreg = new Regex("sm[0-9]+");
            Match m = urlreg.Match(link);
            this.tagstr = string.Empty;
            string str;
            if (m.Success)
            {
                string urlstr = "http://ext.nicovideo.jp/api/getthumbinfo/" + m.Value;
                var reader = new XmlTextReader(urlstr);
                reader.WhitespaceHandling = WhitespaceHandling.None;
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                    {
                        continue;
                    }

                    if (reader.LocalName.Equals("view_counter"))
                    {
                        this.view_num = int.Parse(reader.ReadString());
                    }
                    else if (reader.LocalName.Equals("comment_num"))
                    {
                        this.com_num = int.Parse(reader.ReadString());
                    }
                    else if (reader.LocalName.Equals("mylist_counter"))
                    {
                        this.mylist_num = int.Parse(reader.ReadString());
                    }
                    else if (reader.LocalName.Equals("title"))
                    {
                        this.title = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("link"))
                    {
                        link = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("tag"))
                    {
                        this.tagstr = this.tagstr + " " + reader.ReadString();
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
                            this.date = DateTime.ParseExact(str, "yyyy-MM-ddTHH:mm:ssK", null);
                        }
                        catch (FormatException)
                        {
                            Console.WriteLine("パース失敗 : " + str);
                        }
                    }
                }
            }
        }

        #endregion
    }


}