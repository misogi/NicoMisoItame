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
        public int ComNum { get; set; }

        /// <summary>
        /// Gets or sets the date.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the date_str.
        /// </summary>
        public string DateStr { get; set; }

        /// <summary>
        /// Gets or sets the desc.
        /// </summary>
        public string Desc { get; set; }

        /// <summary>
        /// Gets or sets the info_str.
        /// </summary>
        public string InfoStr { get; set; }

        /// <summary>
        /// Gets or sets the link.
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets the mylist_num.
        /// </summary>
        public int MylistNum { get; set; }

        /// <summary>
        /// Gets or sets the myliststr.
        /// </summary>
        public string Myliststr { get; set; }

        /// <summary>
        /// Gets or sets the pts.
        /// </summary>
        public string Pts { get; set; }

        /// <summary>
        /// Gets or sets the resstr.
        /// </summary>
        public string Resstr { get; set; }

        /// <summary>
        /// Gets or sets the tagstr.
        /// </summary>
        public string Tagstr { get; set; }

        /// <summary>
        /// Gets or sets the thambnail_url.
        /// </summary>
        public string ThambnailUrl { get; set; }

        /// <summary>
        /// Gets or sets the thumbnail.
        /// </summary>
        public BitmapImage Thumbnail { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the view_num.
        /// </summary>
        public int ViewNum { get; set; }

        /// <summary>
        /// Gets or sets the view_str.
        /// </summary>
        public string ViewStr { get; set; }

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
            this.Tagstr = string.Empty;
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
                        this.ViewNum = int.Parse(reader.ReadString());
                    }
                    else if (reader.LocalName.Equals("comment_num"))
                    {
                        this.ComNum = int.Parse(reader.ReadString());
                    }
                    else if (reader.LocalName.Equals("mylist_counter"))
                    {
                        this.MylistNum = int.Parse(reader.ReadString());
                    }
                    else if (reader.LocalName.Equals("title"))
                    {
                        this.Title = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("link"))
                    {
                        link = reader.ReadString();
                    }
                    else if (reader.LocalName.Equals("tag"))
                    {
                        this.Tagstr = this.Tagstr + " " + reader.ReadString();
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
                            this.Date = DateTime.ParseExact(str, "yyyy-MM-ddTHH:mm:ssK", null);
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