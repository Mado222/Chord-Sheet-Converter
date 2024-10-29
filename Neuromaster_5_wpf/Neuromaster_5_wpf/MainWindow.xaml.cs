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

namespace Neuromaster_5_wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void miConnection_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                // Set the clicked item as checked
                clickedItem.IsChecked = true;

                // Uncheck the other items
                miD2xx.IsChecked = clickedItem == miD2xx;
                miVirtualCom.IsChecked = clickedItem == miVirtualCom;
                miSDCard.IsChecked = clickedItem == miSDCard;
            }
        }
        private void miDisplayChannels_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem clickedItem)
            {
                // Set the clicked item as checked
                clickedItem.IsChecked = true;

                // Uncheck the other items
                miSelectedModule.IsChecked = clickedItem == miSelectedModule;
                miRawChannels.IsChecked = clickedItem == miRawChannels;
                miPredefinedChannels.IsChecked = clickedItem == miPredefinedChannels;
            }
            //SetupFlowChart();
        }
    }
}