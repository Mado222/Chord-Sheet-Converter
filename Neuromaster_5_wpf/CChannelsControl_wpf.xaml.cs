using FeedbackDataLib.Modules;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Neuromaster_5_wpf
{
    public class ColorToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Color color)
            {
                return new SolidColorBrush(color);
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    /// <summary>
    /// Interaction logic for CChannelsControl_wpf.xaml
    /// </summary>
    public partial class CChannelsControl_wpf : UserControl
    {
        public List<CModuleBase> Modules { get; set; } = [];

        public CChannelsControl_wpf()
        {
            InitializeComponent();
            Modules.Add(new CModuleBase());
            cModuleInfoDataGridView.DataContext = this;
        }

        private void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Example: Change the background color of the second row (index 1)
            if (e.Row.GetIndex() == 1)
            {
                e.Row.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }


        private void CModuleInfoDataGridView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Example: Change the background color of the second row (index 1)
            if (e.Row.GetIndex() == 1)
            {
                e.Row.Background = new SolidColorBrush(Colors.LightBlue);
            }
        }

    }
}
