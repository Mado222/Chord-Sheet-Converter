using FeedbackDataLib;
using FeedbackDataLib.Modules;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace WPFLib
{
    public class ColorToBrushConverter : IValueConverter
    {
        public static System.Windows.Media.Color ConvertDrawingColorToMediaColor(System.Drawing.Color drawingColor)
        {
            return System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Drawing.Color drawingColor)
            {
                return new SolidColorBrush(System.Windows.Media.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B));
            }
            else if (value is System.Windows.Media.Color mediaColor)
            {
                return new SolidColorBrush(mediaColor);
            }
            else
            {
                // Default to White if the value isn't recognized
                return new SolidColorBrush(System.Windows.Media.Colors.White);
            }
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
        public ObservableCollection<CSWChannel> SWChannels { get; set; } = [];


        public CChannelsControl_wpf()
        {
            InitializeComponent();
            Modules.Clear();
            Modules.Add(new CModuleBase
            {
                ModuleColor = System.Drawing.Color.Aqua,
                ModuleName = "Name1"
            });

            Modules.Add(new CModuleBase
            {
                ModuleColor = System.Drawing.Color.Red,
                ModuleName = "Name2"
            });

            SWChannels.Clear();
            SWChannels.Add(new CSWChannel
            {
                SampleInt = 100,
                SWChannelName = "SW1",
                SWChannelColor = System.Drawing.Color.Yellow,
                SaveChannel = true,
                SendChannel = true,
                SkalMax = 100,
                SkalMin = 0,
                SWChannelNumber = 0,
            });

            SWChannels.Add(new CSWChannel
            {
                SampleInt = 50,
                SWChannelName = "SW2",
                SWChannelColor = System.Drawing.Color.Violet,
                SaveChannel = false,
                SendChannel = true,
                SkalMax = 1e-6,
                SkalMin = -1e-6,
                SWChannelNumber = 1,
            });

            cModuleInfoDataGridView.DataContext = this;
            sWChannelsDataGridView.DataContext = this;
        }

        
        private void SWChannelsDataGridView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // Example: Change the background color of the second row (index 1)
            /*
            if (e.Row.GetIndex() == 1)
            {
                e.Row.Background = new SolidColorBrush(Colors.LightBlue);
            }*/
        }
        private void CModuleInfoDataGridView_LoadingRow(object sender, DataGridRowEventArgs e)
        {
        }
    }
}
