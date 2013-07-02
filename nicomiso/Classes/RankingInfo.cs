// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RankingInfo.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Nicomiso
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Windows.Media.Imaging;
    using System.Xml;

    using HtmlAgilityPack;

    /// <summary>
    /// The ranking info.
    /// </summary>
    internal class RankingInfo
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RankingInfo"/> class.
        /// </summary>
        /// <param name="q">
        /// The q.
        /// </param>
        /// <param name="n">
        /// The n.
        /// </param>
        public RankingInfo(string q, string n)
        {
            this.query = q;
            this.name = n;
            this.mvinfo = new NicoMovieInfo[100];
            int i = 0;

            // System.Net.Cache.RequestCachePolicy _policy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
            for (i = 0; i < 100; i++)
            {
                this.mvinfo[i] = new NicoMovieInfo();
                this.mvinfo[i].Thumbnail = new BitmapImage();
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the mvinfo.
        /// </summary>
        public NicoMovieInfo[] mvinfo { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Gets or sets the query.
        /// </summary>
        public string query { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// XMLファイルを読み込んで、動画情報を100個配列に読み込む
        /// </summary>
        /// <param name="isimgload">
        /// 画像をnetからダウンロードするか。true=読む false=読まない
        /// </param>
        public void LoadFile(bool isimgload)
        {
            if (!Directory.Exists("cache/"))
            {
                Directory.CreateDirectory("cache");
            }

            string filename = "cache/" + this.query + ".xml";
            if (!File.Exists(filename))
            {
                return;
            }

            var reader = new XmlTextReader(filename);
            int i = 0;
            bool isstart = true;
            string str;
            var htdocs = new HtmlDocument();
            while (reader.Read())
            {
                if (!reader.IsStartElement())
                {
                    continue;
                }

                if (reader.LocalName.Equals("item"))
                {
                    if (isstart)
                    {
                        isstart = false;
                    }
                    else
                    {
                        i++;
                    }
                }
                else if (reader.LocalName.Equals("title"))
                {
                    this.mvinfo[i].Title = reader.ReadString();
                }
                else if (reader.LocalName.Equals("link"))
                {
                    this.mvinfo[i].Link = reader.ReadString();
                }
                else if (reader.LocalName.Equals("pubDate"))
                {
                    if (reader.IsStartElement())
                    {
                        // mvinfo[i].pubdate = DateTime.ParseExact( reader.ReadString() ,"yyyy mm:ss",null);
                    }
                }
                else if (reader.LocalName.Equals("strong"))
                {
                    str = reader.ReadString();
                }
                else if (reader.LocalName.Equals("description"))
                {
                    str = reader.ReadString();
                    htdocs.LoadHtml(str);
                    HtmlNodeCollection htn;
                    htn = htdocs.DocumentNode.SelectNodes("//strong");
                    if (htn != null)
                    {
                        this.mvinfo[i].Pts = htn[0].InnerText;
                        this.mvinfo[i].ViewNum = int.Parse(htn[4].InnerText, NumberStyles.AllowThousands);
                        this.mvinfo[i].ViewStr = "再生: " + htn[4].InnerText + "\nコメ: " + htn[5].InnerText + "\nマイ: "
                                                 + htn[6].InnerText;
                        this.mvinfo[i].Date = DateTime.ParseExact(htn[2].InnerText, "yyyy年MM月dd日 HH：mm：ss", null);
                        this.mvinfo[i].DateStr = this.mvinfo[i].Date.ToString("yyyy/MM/dd HH:mm:ss");
                        this.mvinfo[i].InfoStr = htn[0].InnerText + " Pts.\n" + htn[1].InnerText + "\n"
                                                 + this.mvinfo[i].DateStr;
                    }

                    htn = htdocs.DocumentNode.SelectNodes(@"//p//img");
                    if (htn != null)
                    {
                        str = htn[0].Attributes["src"].Value;
                        this.mvinfo[i].ThambnailUrl = str;
                        if (isimgload)
                        {
                            this.mvinfo[i].Thumbnail.BeginInit();
                            this.mvinfo[i].Thumbnail.UriSource = new Uri(str);
                            this.mvinfo[i].Thumbnail.EndInit();

                            // mvinfo[i].thumbnail.Freeze();
                        }
                    }
                }
            }
        }

        /// <summary>
        ///     画像を読み込む。予めサムネイルが保存されてる必要あり
        /// </summary>
        public void LoadImage()
        {
            foreach (NicoMovieInfo m in this.mvinfo)
            {
                if (m.ThambnailUrl != null && m.Thumbnail.UriSource == null)
                {
                    m.Thumbnail.BeginInit();
                    m.Thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    m.Thumbnail.UriSource = new Uri(m.ThambnailUrl);
                    m.Thumbnail.EndInit();
                }
            }
        }

        #endregion
    }
}