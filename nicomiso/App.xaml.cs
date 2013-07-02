// --------------------------------------------------------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="">
//   
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Nicomiso
{
    using System.Threading;
    using System.Windows;

    /// <summary>
    ///     App.xaml の相互作用ロジック
    /// </summary>
    public partial class App : Application
    {
        #region Fields

        /// <summary>
        /// The mutex.
        /// </summary>
        private Mutex mutex;

        #endregion

        #region Methods

        /// <summary>
        /// The application_ exit.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (this.mutex != null)
            {
                /* Mutexを解放 */
                this.mutex.ReleaseMutex();

                /* Mutexを破棄 */
                this.mutex.Close();
            }
        }

        /// <summary>
        /// The application_ startup.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /* 名前を指定してMutexを生成 */
            this.mutex = new Mutex(false, "misogi.nicomiso");

            /* 二重起動をチェック */
            if (!this.mutex.WaitOne(0, false))
            {
                /* 二重起動の場合はエラーを表示して終了 */
                MessageBox.Show("すでに起動されています");

                /* Mutexを破棄 */
                this.mutex.Close();
                this.mutex = null;

                /* 起動を中止してプログラムを終了 */
                this.Shutdown();
            }
        }

        #endregion
    }
}