using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace nicomiso
{
    /// <summary>
    /// App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        System.Threading.Mutex mutex;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /* 名前を指定してMutexを生成 */
            mutex = new System.Threading.Mutex(false, "misogi.nicomiso");

            /* 二重起動をチェック */
            if (!mutex.WaitOne(0, false))
            {
                /* 二重起動の場合はエラーを表示して終了 */
                MessageBox.Show("すでに起動されています");

                /* Mutexを破棄 */
                mutex.Close();
                mutex = null;

                /* 起動を中止してプログラムを終了 */
                this.Shutdown();
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (mutex != null)
            {
                /* Mutexを解放 */
                mutex.ReleaseMutex();

                /* Mutexを破棄 */
                mutex.Close();
            }
        }
    }
}
