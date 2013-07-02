namespace Nicomiso
{
    using System;
    using System.Globalization;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Media;

    /// <summary>
    /// The font col converter.
    /// </summary>
    internal class FontColConverter : IValueConverter
    {
        #region Public Methods and Operators

        /// <summary>
        /// The convert.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var item = (ListViewItem)value;

            var listView = ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            var mvinfo = listView.ItemsSource as NicoMovieInfo[];

            // Get the index of a ListViewItem
            int index = listView.ItemContainerGenerator.IndexFromContainer(item);

            TimeSpan ts = DateTime.Now - mvinfo[index].Date;
            if (ts.TotalHours <= 24)
            {
                return Brushes.Red;
            }
            else if (ts.TotalDays < 3)
            {
                return Brushes.DarkRed;
            }
            else if (ts.TotalDays < 7)
            {
                return Brushes.DarkRed;
            }
            else if (ts.TotalDays < 30)
            {
                return Brushes.DarkGreen;
            }
            else if (ts.TotalDays < 365)
            {
                return Brushes.Gray;
            }
            else
            {
                return Brushes.Gray;
            }
        }

        /// <summary>
        /// The convert back.
        /// </summary>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="targetType">
        /// The target type.
        /// </param>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// <param name="culture">
        /// The culture.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        /// <exception cref="NotImplementedException">
        /// </exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
