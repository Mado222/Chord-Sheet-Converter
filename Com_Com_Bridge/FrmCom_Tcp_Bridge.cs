using ComponentsLib_GUI;
using System.IO.Ports;

namespace Com_Com_Bridge
{
    public partial class FrmCom_Tcp_Bridge : Form
    {
        //private ComToTcpBridge? bridge;
        private CSerialPortBridge? portBridge;
        public FrmCom_Tcp_Bridge()
        {
            InitializeComponent();
            ucComPortSelectorIn.Init("");
            ucComPortSelectorOut.Init("");
        }

        //open com
        private void cToggleButton1_ToState2(object sender, EventArgs e)
        {
            portBridge = new CSerialPortBridge(ucComPortSelectorIn.Text, 115200, ucComPortSelectorOut.Text, 115200);
            portBridge.StatusReported += PortBridge_StatusReported;
            portBridge.DataReported += PortBridge_DataReported;
            portBridge.Start();

        }

        private void PortBridge_DataReported(object? sender, DataEventArgs e)
        {
            // Check if we need to marshal the call to the UI thread
            if (txtStatus.InvokeRequired)
            {
                // Use Invoke to call the method on the UI thread
                txtStatus.Invoke(new Action(() => PortBridge_DataReported(sender, e)));
                return; // Exit this instance of the method, as it will be called again on the UI thread
            }

            string s = e.FromPort.ToString() + " -> " + e.ToPort.ToString() + ": ";
            s += e.Data;
            Color col = Color.Blue;
            if (e.FromPort.ToString() == ucComPortSelectorIn.Text)
            {
                col = Color.Orange;
            }
            txtStatus.AddStatusStringNoDateTime(s, col);
        }

        private void PortBridge_StatusReported(object? sender, string e)
        {
            txtStatus.AddStatusStringNoDateTime(e, Color.Black);
        }

        //close com
        private void cToggleButton1_ToState1(object sender, EventArgs e)
        {
            portBridge?.Stop();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
