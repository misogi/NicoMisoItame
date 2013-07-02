using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Net;
using System.Xml;
using System.Configuration;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Xml.Linq;

using NicoLogin;

namespace nicomiso
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        RankingInfo[] rankinfo = {
            new RankingInfo("all","all"),
            new RankingInfo("g_ent","総合_エンタ"),
            new RankingInfo("ent","エンタメ"),
            new RankingInfo("music","音楽"),
            new RankingInfo("sport","スポーツ"),
            new RankingInfo("g_life","総合_生活"),
            new RankingInfo("animal","動物"),
            new RankingInfo("cooking","料理"),
            new RankingInfo("diary","日記"),
            new RankingInfo("nature","自然"),
            new RankingInfo("science","科学"),
            new RankingInfo("history","歴史"),
            new RankingInfo("radio","ラジオ"),
            new RankingInfo("lecture","動画講座"),
            new RankingInfo("politics","政治"),
            new RankingInfo("g_try","総合_やってみた"),
            new RankingInfo("sing","歌ってみた"),
            new RankingInfo("play","演奏してみた"),
            new RankingInfo("dance","踊ってみた"),
            new RankingInfo("draw","描いてみた"),
            new RankingInfo("tech","技術部"),
            new RankingInfo("g_culture","総合_アニメ"),
            new RankingInfo("anime","アニメ"),
            new RankingInfo("game","ゲーム"),
            new RankingInfo("g_popular","総合_殿堂入り"),
            new RankingInfo("imas","アイマス"),
            new RankingInfo("toho","東方"),
            new RankingInfo("vocaloid","VOCALOID"),
            new RankingInfo("are","例のアレ"),
            new RankingInfo("other","その他")
        };
        string _logstr = "";
        NicoMovieInfo[] srmvinfo;
        NicoMovieInfo[] _mvinfo_mylist;
        CookieChecker _login;
        SearchInfo[] srinfo = {
                              new SearchInfo {sortword = "新着コメント", sortquery = ""},
                              new SearchInfo {sortword = "再生数", sortquery = "v"},
                              new SearchInfo {sortword = "コメント数", sortquery = "r"},
                              new SearchInfo {sortword = "マイリスト数", sortquery = "m"},
                              new SearchInfo {sortword = "投稿日時(新しい)", sortquery = "f"},
                              new SearchInfo {sortword = "再生時間", sortquery = "l"}
                              };

        #region [ ---------------------------------------- 初期化処理 ------------------------------ ]
        public MainWindow()
        {
            InitializeComponent();
        }
        /// <summary>
        /// ウインドウがロードされたら、ランキング情報を読み込む
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach( RankingInfo r in rankinfo){
                listBox_RankCategory.Items.Add( r.name );
            }
            foreach (SearchInfo si in srinfo) {
                comboSearchSort.Items.Add( si.sortword );
            }
            srmvinfo = new NicoMovieInfo[100];
            for (int i = 0; i < 100; i++) {
                srmvinfo[i] = new NicoMovieInfo();
            }
            comboSearchSort.SelectedIndex = 0;
            progressBar_LoadRanking.Minimum = 0;
            progressBar_LoadRanking.Maximum = rankinfo.Count();
            progressBar_LoadRanking.Value = 0;
            expander1.IsExpanded = true;
            listBox_RankCategory.SelectedIndex = 0;
            //マルチスレッド処理
            rankinfo[0].LoadFile(false);
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += BackGroundInit;
            bw.RunWorkerCompleted += BackGroundInitCompleted;
            bw.RunWorkerAsync();

        }
        /// <summary>
        /// バックグラウンド処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackGroundInit(object sender, DoWorkEventArgs e)
        {
            DownloadRankingXML();
            Log("ランキングデータをダウンロードしたXMLから読み込んでいます... ");
            LoadRankingFromXML(false);
        }
        /// <summary>
        /// バックグラウンド処理が終わったらする処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackGroundInitCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Log("バックグラウンド初期化終了しました。");
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_Ranking.ItemsSource);
            dataView.Refresh();
            listBox_RankCategory.IsEnabled = true;
        }
        #endregion

        #region [ ---------------------------------------- メソッド --------------------------------------------- ]
        /// <summary>
        /// ランキングのXMLをニコニコAPIからダウンロード
        /// </summary>
        private void DownloadRankingXML()
        {
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar_LoadRanking.SetValue);
            double prog = 0;
            string urlpre = "http://www.nicovideo.jp/ranking/mylist/daily/";

            //Parallel.ForEach(rankinfo, (r) =>
            //{
            DateTime n = DateTime.Now;
            DateTime today6 = new DateTime(n.Year, n.Month, n.Day, 6, 0, 0);
            foreach (RankingInfo r in rankinfo)
            {
                //System.Threading.Thread.Sleep(500);
                string file = "cache/" + r.query + ".xml";
                if (System.IO.File.Exists(file))
                {
                    DateTime filedate = System.IO.File.GetLastWriteTime(file);
                    if (filedate > today6)
                    {
                        Log("既に新しいデータが存在します。: " + file);
                        continue;
                    }
                }
                WebClient wc = new WebClient();
                string url = urlpre + r.query + "?rss=2.0";
                Uri u_url = new Uri(url);
                try
                {
                    wc.DownloadFile(u_url, file);
                }
                catch (WebException)
                {

                }
                prog += 1;
                Dispatcher.Invoke(updatePbDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, prog });
                Log("xml " + url + "を読み込みました");

            }
            //});
            Log("ランキングデータの読み込みに成功しました。");
        }
        /// <summary>
        /// XMLファイルから、配列に動画データを読み込む
        /// </summary>
        private void LoadRankingFromXML(bool imgflag)
        {
            if (imgflag)
            {
                Log("画像を読み込みます。");
            }
            UpdateProgressBarDelegate updatePbDelegate = new UpdateProgressBarDelegate(progressBar_LoadRanking.SetValue);
            double prog = 0;

            foreach (RankingInfo r in rankinfo)
            {
                r.LoadFile(imgflag);
                prog += 1;
                Dispatcher.Invoke(updatePbDelegate,
                    System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { ProgressBar.ValueProperty, prog });
            }
            /*
            Parallel.ForEach(rankinfo, (r) =>
            {
                r.LoadFile();
            });
            */
        }

        private void LoadMylist(int ind)
        {
            XElement xe = XElement.Parse(_login._mylistgroup[ind].xmlstr);
            var query = from p in xe.Descendants("item") select p;
            int i = 0;
            int count = query.Count();
            _mvinfo_mylist = new NicoMovieInfo[count];
            foreach (var item in query)
            {
                _mvinfo_mylist[i] = new NicoMovieInfo();
                Console.WriteLine("{0} : {1}", item.Element("title").Value, item.Element("pubDate").Value);
                _mvinfo_mylist[i++].Title = item.Element("title").Value;
            }
            listView_Mylist.ItemsSource = _mvinfo_mylist;
        }
        #endregion

        #region [ -------------------------------------------- ツール ----------------------------　]
        /// <summary>
        /// ログを吐き出す処理
        /// </summary>
        /// <param name="msg"></param>
        delegate void WriteLineDelegate(string msg);
        private void Log(string log)
        {
            _logstr += log + Environment.NewLine;
            textBox_Log.Dispatcher.BeginInvoke(new WriteLineDelegate(Log_WriteLine), _logstr);
        }
        private void Log_WriteLine(string msg)
        {
            textBox_Log.Text = msg;
            textBox_Log.ScrollToEnd();
        }
        //デリゲート
        private delegate void UpdateProgressBarDelegate(System.Windows.DependencyProperty dp, Object value);
        private delegate void ReadImageDelegate();
        #endregion

        #region [ -------------------------------------------- イベントハンドラ ----------------------- ]
        /// <summary>
        /// ランキングの要素がダブルクリックされたら、ブラウザで動画ページを開く
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listRanking_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string urlstr = rankinfo[listBox_RankCategory.SelectedIndex].mvinfo[listView_Ranking.SelectedIndex].Link;
            try
            {
                System.Diagnostics.Process.Start( urlstr );
            }catch(InvalidOperationException){
                Log("リンクの読み込みに失敗しました。: " + urlstr);
            }
        }


        private void listViewSearch_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Regex urlreg = new Regex("sm[0-9]+");
            int ind = listViewSearch.SelectedIndex;
            if( ind < 0 ){return;};
            Match m = urlreg.Match( srmvinfo[ind].Link );
            string urlstr;
            string infostr;
            System.Collections.Hashtable ha = new System.Collections.Hashtable();
            string _n = Environment.NewLine;
            string[] infoparam = {"video_id","title","description","thumbnail_url","first_recieve",
                                     "length","movie_type","size_high","size_low","view_counter",
                                 "comment_num","mylist_counter","lase_res_body","watch_url",
                                 "tag","user_id"};
            if (m.Success) {
                urlstr = "http://ext.nicovideo.jp/api/getthumbinfo/" + m.Value;
                XmlTextReader reader = new XmlTextReader(urlstr);
                while (reader.Read())
                {
                    if (!reader.IsStartElement())
                    {
                        continue;
                    }
                    foreach (string key in infoparam) {
                        if (reader.LocalName.Equals(key)) {
                            ha[key] = reader.ReadString();
                        }
                    }
                }
                infostr = String.Format("{0}\n{1}\n再生:{2} コメ:{3} マイ:{4} ファイルサイズ {5} {6}",
                    ha["title"], ha["description"],ha["view_counter"],ha["comment_num"],
                    ha["mylist_counter"],ha["size_high"],ha["movie_type"]);
                textBox_Searchmvinfo.Text = infostr;
            }
        }

        private void buttonSearch_Click(object sender, RoutedEventArgs e)
        {
            string searchstr;
            string sortstr = "?sort=" + srinfo[comboSearchSort.SelectedIndex].sortquery;
            if ( (bool)checkSearchOrder.IsChecked ) {
                sortstr += "&order=a";
            }
            searchstr = "http://www.nicovideo.jp/tag/" + Uri.EscapeUriString(textSearchWord.Text) + sortstr + "&rss=2.0";
            //listViewSearch.Items.Clear();
            XmlTextReader reader = new XmlTextReader(searchstr);
            reader.WhitespaceHandling = WhitespaceHandling.None;
            int i = 0;
            int mvnum = 0;
            string str;
            HtmlDocument htdocs = new HtmlDocument();
            if (!reader.HasLineInfo()) { return; }
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
                    srmvinfo[i].Title = reader.ReadString();
                }
                else if (reader.LocalName.Equals("link"))
                {
                    srmvinfo[i].Link = reader.ReadString();
                    if ((bool)checkBox_narrow.IsChecked)
                    {
                        srmvinfo[i].GetMovieInfoDetail(srmvinfo[i].Link);
                    }
                    else {
                        srmvinfo[i].ViewNum = 0;
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
                        srmvinfo[i].Date = DateTime.ParseExact(htn[1].InnerText, "yyyy年MM月dd日 HH：mm：ss", null);
                        srmvinfo[i].Desc = srmvinfo[i].Date.ToString("yyyy/MM/dd HH:mm:ss");
                    }
                }
            }
            mvnum = i;
            listViewSearch.ItemsSource = srmvinfo;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listViewSearch.ItemsSource);
            dataView.Refresh();
        }

        private void textSearchWord_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) {
                this.buttonSearch_Click(sender, e);
            }
        }

        private void listViewSearch_HeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection dire;
            dire = ListSortDirection.Descending;
            string header = headerClicked.Column.Header as string;
            //listViewSearch.Items.SortDescriptions.Clear();
            SortDescription sd = new SortDescription( header , dire );
            //listViewSearch.Items.SortDescriptions.Add(sd);
            //listViewSearch.Items.Refresh();
            //listViewSearch.Items.Clear();
            listViewSearch.ItemsSource = srmvinfo;
            ICollectionView dataView = CollectionViewSource.GetDefaultView( listViewSearch.ItemsSource );
            dataView.SortDescriptions.Clear();
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        private void listViewSearch_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Diagnostics.Process.Start( rankinfo[listBox_RankCategory.SelectedIndex].mvinfo[listViewSearch.SelectedIndex].Link);
        }

        private void checkBox_narrow_Checked(object sender, RoutedEventArgs e)
        {
            groupBox_narrow.IsEnabled = true;
        }

        private void checkBox_narrow_Unchecked(object sender, RoutedEventArgs e)
        {
            groupBox_narrow.IsEnabled = false;
        }


        private void button_load_Click(object sender, RoutedEventArgs e)
        {
            bool imgflag = (bool)toolBarItem_IsImageLoad.IsChecked;
            LoadRankingFromXML( imgflag );
        }


        /// <summary>
        /// イベントハンドラ:ランキングのカテゴリを変更したとき
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox_RankCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            listView_Ranking.ItemsSource = rankinfo[listBox_RankCategory.SelectedIndex].mvinfo;
            if ((bool)toolBarItem_IsImageLoad.IsChecked)
            {
                rankinfo[listBox_RankCategory.SelectedIndex].LoadImage();
            }
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_Ranking.ItemsSource);
            dataView.Refresh();
        }

        private void expander1_Expanded(object sender, RoutedEventArgs e)
        {
            Grid_Logbar.Height = new GridLength(1, GridUnitType.Star);
        }

        private void expander1_Collapsed(object sender, RoutedEventArgs e)
        {
            Grid_Logbar.Height = new GridLength(22);
        }
        /// <summary>
        /// メニューのXML読み込み
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_ReadXML_Click(object sender, RoutedEventArgs e)
        {
            DownloadRankingXML();
        }

        private void MenuItem_ReadSqlite_Click(object sender, RoutedEventArgs e)
        {
            listView_Ranking.ItemsSource = rankinfo[listBox_RankCategory.SelectedIndex].mvinfo;
            ICollectionView dataView = CollectionViewSource.GetDefaultView(listView_Ranking.ItemsSource);
            dataView.Refresh();
        
        }

        private void toolBarItem_IsImageLoad_Click(object sender, RoutedEventArgs e)
        {
            bool imageflag = (bool)toolBarItem_IsImageLoad.IsChecked;
            menuitem_Thumbnail.IsChecked = imageflag;
            if (imageflag)
            {
                rankinfo[listBox_RankCategory.SelectedIndex].LoadImage();
                datagridRanking.Columns.Clear();
                GridViewColumn gc1 = new GridViewColumn();
                gc1.DisplayMemberBinding = new Binding("title");
                gc1.Header = "Title";
                GridViewColumn gc2 = new GridViewColumn();
                gc2.DisplayMemberBinding = new Binding("info_str");
                gc2.Header = "Info";
                GridViewColumn gc3 = new GridViewColumn();
                gc3.DisplayMemberBinding = new Binding("view_str");
                gc3.Header = "View";
                GridViewColumn gc4 = new GridViewColumn();
                gc4.CellTemplate = Resources["template_thumb"] as DataTemplate;
                gc4.Header = "Image";
                datagridRanking.Columns.Add(gc4);
                datagridRanking.Columns.Add(gc1);
                datagridRanking.Columns.Add(gc2);
                datagridRanking.Columns.Add(gc3);

            }
            else
            {
                datagridRanking.Columns.Clear();
                GridViewColumn gc = new GridViewColumn();
                gc.DisplayMemberBinding = new Binding("title");
                gc.Header = "Title";
                GridViewColumn gc3 = new GridViewColumn();
                gc3.CellTemplate = Resources["template_pts"] as DataTemplate;
                gc3.Width = 50;
                gc3.Header = "Pts";
                GridViewColumn gc2 = new GridViewColumn();
                gc2.DisplayMemberBinding = new Binding("date_str");
                gc2.Header = "Date";
                datagridRanking.Columns.Add(gc);
                datagridRanking.Columns.Add(gc3);
                datagridRanking.Columns.Add(gc2);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            ModifierKeys mod = Keyboard.Modifiers;
            if (e.Key == Key.F1)
            {
                tabControl1.SelectedIndex = 0;
            }
            else if (e.Key == Key.F2)
            {
                tabControl1.SelectedIndex = 1;
            }else if( mod == ModifierKeys.Control && e.Key == Key.I ){
                toolBarItem_IsImageLoad.IsChecked = ! toolBarItem_IsImageLoad.IsChecked;
                toolBarItem_IsImageLoad_Click(sender,e);
            }

        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
                toolBarItem_IsImageLoad.IsChecked = menuitem_Thumbnail.IsChecked;
                toolBarItem_IsImageLoad_Click(sender,e);

        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            listView_Ranking.Width = this.Width - 132;
            textBox_Log.Width = this.Width - 20;
        }

        private void MenuItem_LoginChrome_Click(object sender, RoutedEventArgs e)
        {
            _login = new NicoLogin.CookieChecker();
            _login.GetCookieChrome();
            _login.GetMylistGroup();
            _login.GetAllMylist();

            foreach (MylistInfo m in _login._mylistgroup) {
                comboBox_MylistGroup.Items.Add(m.name);
            }
            comboBox_MylistGroup.SelectedIndex = 0;
        }

        private void comboBox_MylistGroup_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox cb = sender as ComboBox;
            LoadMylist(cb.SelectedIndex);
        }
        #endregion
    }
}
