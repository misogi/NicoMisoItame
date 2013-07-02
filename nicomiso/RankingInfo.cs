﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Xml;
using HtmlAgilityPack;
using System.Windows.Media.Imaging;
using System.Windows; 

namespace nicomiso
{
    class RankingInfo
    {
        public string name { get; set; }
        public string query { get; set; }
        public nicoMovieInfo[] mvinfo { get; set; }

        public RankingInfo(string q, string n)
        {
            query = q;
            name = n;
            mvinfo = new nicoMovieInfo[100];
            int i = 0;
            //System.Net.Cache.RequestCachePolicy _policy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);

            for (i = 0; i < 100; i++)
            {
                mvinfo[i] = new nicoMovieInfo();
                mvinfo[i].thumbnail = new BitmapImage();
            }
        }
        /// <summary>
        /// XMLファイルを読み込んで、動画情報を100個配列に読み込む
        /// </summary>
        /// <param name="isimgload">画像をnetからダウンロードするか。true=読む false=読まない</param>
        public void LoadFile(bool isimgload)
        {
            if (!System.IO.Directory.Exists( "cache/"))
            {
                System.IO.Directory.CreateDirectory("cache");
            }
            String filename = "cache/" + query + ".xml";
            if( !System.IO.File.Exists(filename)){
                return;
            }
            XmlTextReader reader = new XmlTextReader(filename);
            int i = 0;
            bool isstart = true;
            string str;
            HtmlDocument htdocs = new HtmlDocument();
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
                }else if (reader.LocalName.Equals("title"))
                {
                    mvinfo[i].title = reader.ReadString();
                }
                else if (reader.LocalName.Equals("link"))
                {
                    mvinfo[i].link = reader.ReadString();
                }
                else if (reader.LocalName.Equals("pubDate"))
                {
                    if (reader.IsStartElement())
                    {
                        // mvinfo[i].pubdate = DateTime.ParseExact( reader.ReadString() ,"yyyy mm:ss",null);
                    }
                }
                else if(reader.LocalName.Equals("strong")){
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
                        mvinfo[i].pts = htn[0].InnerText;
                        mvinfo[i].view_num = int.Parse(htn[4].InnerText, System.Globalization.NumberStyles.AllowThousands);
                        mvinfo[i].view_str = "再生: " + htn[4].InnerText + "\nコメ: " + htn[5].InnerText + "\nマイ: " + htn[6].InnerText;
                        mvinfo[i].date = DateTime.ParseExact( htn[2].InnerText , "yyyy年MM月dd日 HH：mm：ss" , null);
                        mvinfo[i].date_str = mvinfo[i].date.ToString("yyyy/MM/dd HH:mm:ss");
                        mvinfo[i].info_str = htn[0].InnerText + " Pts.\n" + htn[1].InnerText + "\n" + mvinfo[i].date_str;
                    }
                    htn = htdocs.DocumentNode.SelectNodes(@"//p//img");
                    if (htn != null)
                    {
                        str = htn[0].Attributes["src"].Value;
                        mvinfo[i].thambnail_url = str;
                        if (isimgload)
                        {
                            mvinfo[i].thumbnail.BeginInit();
                            mvinfo[i].thumbnail.UriSource = new Uri(str);
                            mvinfo[i].thumbnail.EndInit();
                            //mvinfo[i].thumbnail.Freeze();
                        }
                    }
                }
            }
        }
        /// <summary>
        /// 画像を読み込む。予めサムネイルが保存されてる必要あり
        /// </summary>
        public void LoadImage() { 
            foreach(nicoMovieInfo m in mvinfo ){
                if (m.thambnail_url != null && m.thumbnail.UriSource == null)
                {
                    m.thumbnail.BeginInit();
                    m.thumbnail.CacheOption = BitmapCacheOption.OnLoad;
                    m.thumbnail.UriSource = new Uri(m.thambnail_url);
                    m.thumbnail.EndInit();
                }
            }
        }
    }
}