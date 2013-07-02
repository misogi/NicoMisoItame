// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="">
//   
// </copyright>
// <summary>
//   MainWindow.xaml の相互作用ロジック
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace nicomiso
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Data;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Xml;
    using System.Xml.Linq;

    using HtmlAgilityPack;

    using NicoLogin;

    /// <summary>
    ///     MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Fields

        /// <summary>
        /// The rankinfo.
        /// </summary>
        private readonly RankingInfo[] rankinfo =
            {
                new RankingInfo("all", "all"), new RankingInfo("g_ent2", "総合_エンタ"), 
                new RankingInfo("ent", "エンタメ"), new RankingInfo("music", "音楽"), 
                new RankingInfo("sport", "スポーツ"), new RankingInfo("g_life2", "総合_生活"),
                new RankingInfo("animal", "動物"), new RankingInfo("cooking", "料理"), 
                new RankingInfo("diary", "日記"), new RankingInfo("nature", "自然"), 
                new RankingInfo("science", "科学"), new RankingInfo("history", "歴史"), 
                new RankingInfo("radio", "ラジオ"), new RankingInfo("lecture", "動画講座"), 
                new RankingInfo("g_politics", "政治"), 
                new RankingInfo("g_tech", "総合_科学"), 
                new RankingInfo("sing", "歌ってみた"), new RankingInfo("play", "演奏してみた"), 
                new RankingInfo("dance", "踊ってみた"), new RankingInfo("draw", "描いてみた"), 
                new RankingInfo("tech", "技術部"), 
                new RankingInfo("g_culture2", "総合_アニメ"), 
                new RankingInfo("anime", "アニメ"), new RankingInfo("game", "ゲーム"), 
                new RankingInfo("g_other", "総合_その他"), 
                new RankingInfo("imas", "アイマス"), new RankingInfo("toho", "東方"), 
                new RankingInfo("vocaloid", "VOCALOID"), 
                new RankingInfo("are", "例のアレ"), new RankingInfo("other", "その他")
            };

        /// <summary>
        /// The srinfo.
        /// </summary>
        private readonly SearchInfo[] srinfo =
            {
                new SearchInfo { sortword = "新着コメント", sortquery = string.Empty }, 
                new SearchInfo { sortword = "再生数", sortquery = "v" }, 
                new SearchInfo { sortword = "コメント数", sortquery = "r" }, 
                new SearchInfo { sortword = "マイリスト数", sortquery = "m" }, 
                new SearchInfo { sortword = "投稿日時(新しい)", sortquery = "f" }, 
                new SearchInfo { sortword = "再生時間", sortquery = "l" }
            };

        /// <summary>
        /// The _login.
        /// </summary>
        private CookieChecker _login;

        /// <summary>
        /// The _logstr.
        /// </summary>
        private string _logstr = string.Empty;

        /// <summary>
        /// The _mvinfo_mylist.
        /// </summary>
        private NicoMovieInfo[] _mvinfo_mylist;

        /// <summary>
        /// The srmvinfo.
        /// </summary>
        private NicoMovieInfo[] srmvinfo;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Delegates

        /// <summary>
        /// The read image delegate.
        /// </summary>
        private delegate void ReadImageDelegate();

        /// <summary>
        /// The update progress bar delegate.
        /// </summary>
        /// <param name="dp">
        /// The dp.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        private delegate void UpdateProgressBarDelegate(DependencyProperty dp, object value);

        /// <summary>
        ///     ログを吐き出す処理
        /// </summary>
        /// <param name="msg"></param>
        private delegate void WriteLineDelegate(string msg);

        #endregion

        #region Methods

        /// <summary>
        /// バックグラウンド処理
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void BackGroundInit(object sender, DoWorkEventArgs e)
        {
            this.DownloadRankingXML();
            this.Log("ランキングデータをダウンロードしたXMLから読み込んでいます... ");
            this.LoadRankingFromXML(false);
        }

        /// <summary>
        /// バックグラウンド処理が終わったらする処理
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void BackGroundInitCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Log("バックグラウンド初期化終了しました。");
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.listView_Ranking.ItemsSource);
            dataView.Refresh();
            this.listBox_RankCategory.IsEnabled = true;
        }

        /// <summary>
        ///     ランキングのXMLをニコニコAPIからダウンロード
        /// </summary>
        private void DownloadRankingXML()
        {
            UpdateProgressBarDelegate updatePbDelegate = this.progressBar_LoadRanking.SetValue;
            double prog = 0;
            string urlpre = "http://www.nicovideo.jp/ranking/mylist/daily/";

            // Parallel.ForEach(rankinfo, (r) =>
            // {
            DateTime n = DateTime.Now;
            var today6 = new DateTime(n.Year, n.Month, n.Day, 6, 0, 0);
            foreach (RankingInfo r in this.rankinfo)
            {
                // System.Threading.Thread.Sleep(500);
                string file = "cache/" + r.query + ".xml";
                if (File.Exists(file))
                {
                    DateTime filedate = File.GetLastWriteTime(file);
                    if (filedate > today6)
                    {
                        this.Log("既に新しいデータが存在します。: " + file);
                        continue;
                    }
                }

                var wc = new WebClient();
                string url = urlpre + r.query + "?rss=2.0";
                var u_url = new Uri(url);
                try
                {
                    wc.DownloadFile(u_url, file);
                }
                catch (WebException)
                {
                }

                prog += 1;
                this.Dispatcher.Invoke(
                    updatePbDelegate, DispatcherPriority.Background, new object[] { RangeBase.ValueProperty, prog });
                this.Log("xml " + url + "を読み込みました");
            }

            // });
            this.Log("ランキングデータの読み込みに成功しました。");
        }

        /// <summary>
        /// The load mylist.
        /// </summary>
        /// <param name="ind">
        /// The ind.
        /// </param>
        private void LoadMylist(int ind)
        {
            XElement xe = XElement.Parse(this._login._mylistgroup[ind].xmlstr);
            IEnumerable<XElement> query = from p in xe.Descendants("item") select p;
            int i = 0;
            int count = query.Count();
            this._mvinfo_mylist = new NicoMovieInfo[count];
            foreach (XElement item in query)
            {
                this._mvinfo_mylist[i] = new NicoMovieInfo();
                Console.WriteLine("{0} : {1}", item.Element("title").Value, item.Element("pubDate").Value);
                this._mvinfo_mylist[i++].Title = item.Element("title").Value;
            }

            this.listView_Mylist.ItemsSource = this._mvinfo_mylist;
        }

        /// <summary>
        /// XMLファイルから、配列に動画データを読み込む
        /// </summary>
        /// <param name="imgflag">
        /// The imgflag.
        /// </param>
        private void LoadRankingFromXML(bool imgflag)
        {
            if (imgflag)
            {
                this.Log("画像を読み込みます。");
            }

            UpdateProgressBarDelegate updatePbDelegate = this.progressBar_LoadRanking.SetValue;
            double prog = 0;

            foreach (RankingInfo r in this.rankinfo)
            {
                r.LoadFile(imgflag);
                prog += 1;
                this.Dispatcher.Invoke(
                    updatePbDelegate, DispatcherPriority.Background, new object[] { RangeBase.ValueProperty, prog });
            }

            /*
            Parallel.ForEach(rankinfo, (r) =>
            {
                r.LoadFile();
            });
            */
        }

        /// <summary>
        /// The log.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        private void Log(string log)
        {
            this._logstr += log + Environment.NewLine;
            this.textBox_Log.Dispatcher.BeginInvoke(new WriteLineDelegate(this.Log_WriteLine), this._logstr);
        }

        /// <summary>
        /// The log_ write line.
        /// </summary>
        /// <param name="msg">
        /// The msg.
        /// </param>
        private void Log_WriteLine(string msg)
        {
            this.textBox_Log.Text = msg;
            this.textBox_Log.ScrollToEnd();
        }

        /// <summary>
        /// The menu item_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.toolBarItem_IsImageLoad.IsChecked = this.menuitem_Thumbnail.IsChecked;
            this.toolBarItem_IsImageLoad_Click(sender, e);
        }

        /// <summary>
        /// The menu item_ login chrome_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MenuItem_LoginChrome_Click(object sender, RoutedEventArgs e)
        {
            this._login = new CookieChecker();
            this._login.GetCookieChrome();
            this._login.GetMylistGroup();
            this._login.GetAllMylist();

            foreach (MylistInfo m in this._login._mylistgroup)
            {
                this.comboBox_MylistGroup.Items.Add(m.name);
            }

            this.comboBox_MylistGroup.SelectedIndex = 0;
        }

        /// <summary>
        /// The menu item_ read sqlite_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void MenuItem_ReadSqlite_Click(object sender, RoutedEventArgs e)
        {
            this.listView_Ranking.ItemsSource = this.rankinfo[this.listBox_RankCategory.SelectedIndex].mvinfo;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.listView_Ranking.ItemsSource);
            dataView.Refresh();
        }

        /// <summary>
        /// メニューのXML読み込み
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void MenuItem_ReadXML_Click(object sender, RoutedEventArgs e)
        {
            this.DownloadRankingXML();
        }

        /// <summary>
        /// The window_ key down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ModifierKeys mod = Keyboard.Modifiers;
            if (e.Key == Key.F1)
            {
                this.tabControl1.SelectedIndex = 0;
            }
            else if (e.Key == Key.F2)
            {
                this.tabControl1.SelectedIndex = 1;
            }
            else if (mod == ModifierKeys.Control && e.Key == Key.I)
            {
                this.toolBarItem_IsImageLoad.IsChecked = ! this.toolBarItem_IsImageLoad.IsChecked;
                this.toolBarItem_IsImageLoad_Click(sender, e);
            }
        }

        /// <summary>
        /// ウインドウがロードされたら、ランキング情報を読み込む
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (RankingInfo r in this.rankinfo)
            {
                this.listBox_RankCategory.Items.Add(r.name);
            }

            foreach (SearchInfo si in this.srinfo)
            {
                this.comboSearchSort.Items.Add(si.sortword);
            }

            this.srmvinfo = new NicoMovieInfo[100];
            for (int i = 0; i < 100; i++)
            {
                this.srmvinfo[i] = new NicoMovieInfo();
            }

            this.comboSearchSort.SelectedIndex = 0;
            this.progressBar_LoadRanking.Minimum = 0;
            this.progressBar_LoadRanking.Maximum = this.rankinfo.Count();
            this.progressBar_LoadRanking.Value = 0;
            this.expander1.IsExpanded = true;
            this.listBox_RankCategory.SelectedIndex = 0;

            // マルチスレッド処理
            this.rankinfo[0].LoadFile(false);
            var bw = new BackgroundWorker();
            bw.DoWork += this.BackGroundInit;
            bw.RunWorkerCompleted += this.BackGroundInitCompleted;
            bw.RunWorkerAsync();
        }

        /// <summary>
        /// The window_ size changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.listView_Ranking.Width = this.Width - 132;
            this.textBox_Log.Width = this.Width - 20;
        }

        // デリゲート

        /// <summary>
        /// The button search_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchstr;
            string sortstr = "?sort=" + this.srinfo[this.comboSearchSort.SelectedIndex].sortquery;
            if ((bool)this.checkSearchOrder.IsChecked)
            {
                sortstr += "&order=a";
            }

            searchstr = "http://www.nicovideo.jp/tag/" + Uri.EscapeUriString(this.textSearchWord.Text) + sortstr
                        + "&rss=2.0";

            // listViewSearch.Items.Clear();
            var reader = new XmlTextReader(searchstr);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            int i = 0;
            int mvnum = 0;
            string str;
            var htdocs = new HtmlDocument();
            if (!reader.HasLineInfo())
            {
                return;
            }

            while (reader.Read())
            {
                if (!reader.IsStartElement())
                {
                    continue;
                }

                if (reader.LocalName.Equals("item"))
                {
                    i++;
                }
                else if (reader.LocalName.Equals("title"))
                {
                    this.srmvinfo[i].Title = reader.ReadString();
                }
                else if (reader.LocalName.Equals("link"))
                {
                    this.srmvinfo[i].Link = reader.ReadString();
                    if ((bool)this.checkBox_narrow.IsChecked)
                    {
                        this.srmvinfo[i].GetMovieInfoDetail(this.srmvinfo[i].Link);
                    }
                    else
                    {
                        this.srmvinfo[i].ViewNum = 0;
                    }
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
                        this.srmvinfo[i].Date = DateTime.ParseExact(htn[1].InnerText, "yyyy年MM月dd日 HH：mm：ss", null);
                        this.srmvinfo[i].Desc = this.srmvinfo[i].Date.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
            }

            mvnum = i;
            this.listViewSearch.ItemsSource = this.srmvinfo;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.listViewSearch.ItemsSource);
            dataView.Refresh();
        }

        /// <summary>
        /// The button_load_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            var imgflag = (bool)this.toolBarItem_IsImageLoad.IsChecked;
            this.LoadRankingFromXML(imgflag);
        }

        /// <summary>
        /// The check box_narrow_ checked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void checkBox_narrow_Checked(object sender, RoutedEventArgs e)
        {
            this.groupBox_narrow.IsEnabled = true;
        }

        /// <summary>
        /// The check box_narrow_ unchecked.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void checkBox_narrow_Unchecked(object sender, RoutedEventArgs e)
        {
            this.groupBox_narrow.IsEnabled = false;
        }

        /// <summary>
        /// The combo box_ mylist group_ selection changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void comboBox_MylistGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var cb = sender as ComboBox;
            this.LoadMylist(cb.SelectedIndex);
        }

        /// <summary>
        /// The expander 1_ collapsed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {
            this.Grid_Logbar.Height = new GridLength(22);
        }

        /// <summary>
        /// The expander 1_ expanded.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void expander1_Expanded(object sender, RoutedEventArgs e)
        {
            this.Grid_Logbar.Height = new GridLength(1, GridUnitType.Star);
        }

        /// <summary>
        /// イベントハンドラ:ランキングのカテゴリを変更したとき
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void listBox_RankCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.listView_Ranking.ItemsSource = this.rankinfo[this.listBox_RankCategory.SelectedIndex].mvinfo;
            if ((bool)this.toolBarItem_IsImageLoad.IsChecked)
            {
                this.rankinfo[this.listBox_RankCategory.SelectedIndex].LoadImage();
            }

            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.listView_Ranking.ItemsSource);
            dataView.Refresh();
        }

        /// <summary>
        /// ランキングの要素がダブルクリックされたら、ブラウザで動画ページを開く
        /// </summary>
        /// <param name="sender">
        /// </param>
        /// <param name="e">
        /// </param>
        private void listRanking_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string urlstr =
                this.rankinfo[this.listBox_RankCategory.SelectedIndex].mvinfo[this.listView_Ranking.SelectedIndex].Link;
            try
            {
                Process.Start(urlstr);
            }
            catch (InvalidOperationException)
            {
                this.Log("リンクの読み込みに失敗しました。: " + urlstr);
            }
        }

        /// <summary>
        /// The list view search_ header click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void listViewSearch_HeaderClick(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection dire;
            dire = ListSortDirection.Descending;
            var header = headerClicked.Column.Header as string;

            // listViewSearch.Items.SortDescriptions.Clear();
            var sd = new SortDescription(header, dire);

            // listViewSearch.Items.SortDescriptions.Add(sd);
            // listViewSearch.Items.Refresh();
            // listViewSearch.Items.Clear();
            this.listViewSearch.ItemsSource = this.srmvinfo;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(this.listViewSearch.ItemsSource);
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        /// <summary>
        /// The list view search_ mouse double click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void listViewSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Process.Start(
                this.rankinfo[this.listBox_RankCategory.SelectedIndex].mvinfo[this.listViewSearch.SelectedIndex].Link);
        }

        /// <summary>
        /// The list view search_ selection changed.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void listViewSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var urlreg = new Regex("sm[0-9]+");
            int ind = this.listViewSearch.SelectedIndex;
            if (ind < 0)
            {
                return;
            }

            ;
            Match m = urlreg.Match(this.srmvinfo[ind].Link);
            string urlstr;
            string infostr;
            var ha = new Hashtable();
            string _n = Environment.NewLine;
            string[] infoparam =
                {
                    "video_id", "title", "description", "thumbnail_url", "first_recieve", "length", 
                    "movie_type", "size_high", "size_low", "view_counter", "comment_num", 
                    "mylist_counter", "lase_res_body", "watch_url", "tag", "user_id"
                };
            if (m.Success)
            {
                urlstr = "http://ext.nicovideo.jp/api/getthumbinfo/" + m.Value;
                var reader = new XmlTextReader(urlstr);
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                    {
                        continue;
                    }

                    foreach (string key in infoparam)
                    {
                        if (reader.LocalName.Equals(key))
                        {
                            ha[key] = reader.ReadString();
                        }
                    }
                }

                infostr = string.Format(
                    "{0}\n{1}\n再生:{2} コメ:{3} マイ:{4} ファイルサイズ {5} {6}", 
                    ha["title"], 
                    ha["description"], 
                    ha["view_counter"], 
                    ha["comment_num"], 
                    ha["mylist_counter"], 
                    ha["size_high"], 
                    ha["movie_type"]);
                this.textBox_Searchmvinfo.Text = infostr;
            }
        }

        /// <summary>
        /// The text search word_ key down.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void textSearchWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                this.buttonSearch_Click(sender, e);
            }
        }

        /// <summary>
        /// The tool bar item_ is image load_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void toolBarItem_IsImageLoad_Click(object sender, RoutedEventArgs e)
        {
            var imageflag = (bool)this.toolBarItem_IsImageLoad.IsChecked;
            this.menuitem_Thumbnail.IsChecked = imageflag;
            if (imageflag)
            {
                this.rankinfo[this.listBox_RankCategory.SelectedIndex].LoadImage();
                this.datagridRanking.Columns.Clear();
                var gc1 = new GridViewColumn();
                gc1.DisplayMemberBinding = new Binding("Title");
                gc1.Header = "Title";
                var gc2 = new GridViewColumn();
                gc2.DisplayMemberBinding = new Binding("InfoStr");
                gc2.Header = "Info";
                var gc3 = new GridViewColumn();
                gc3.DisplayMemberBinding = new Binding("ViewStr");
                gc3.Header = "View";
                var gc4 = new GridViewColumn();
                gc4.CellTemplate = this.Resources["template_thumb"] as DataTemplate;
                gc4.Header = "Image";
                this.datagridRanking.Columns.Add(gc4);
                this.datagridRanking.Columns.Add(gc1);
                this.datagridRanking.Columns.Add(gc2);
                this.datagridRanking.Columns.Add(gc3);
            }
            else
            {
                this.datagridRanking.Columns.Clear();
                var gc = new GridViewColumn();
                gc.DisplayMemberBinding = new Binding("Title");
                gc.Header = "Title";
                var gc3 = new GridViewColumn();
                gc3.CellTemplate = this.Resources["template_pts"] as DataTemplate;
                gc3.Width = 50;
                gc3.Header = "Pts";
                var gc2 = new GridViewColumn();
                gc2.DisplayMemberBinding = new Binding("DateStr");
                gc2.Header = "Date";
                this.datagridRanking.Columns.Add(gc);
                this.datagridRanking.Columns.Add(gc3);
                this.datagridRanking.Columns.Add(gc2);
            }
        }

        #endregion
    }
}