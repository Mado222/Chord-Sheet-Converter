using System;
using System.Linq;
using System.Windows.Forms;
namespace ComponentsLibGUI
{
    using System;
    using System.Linq;
    using System.Windows.Forms;

    public class DualMonitorPlacement
    {
        public void MoveWindowToCorrectMonitor(Form window, bool placeOnPrimary)
        {
            var screens = Screen.AllScreens;

            if (screens.Length >= 2)
            {
                // Select the appropriate screen
                Screen targetScreen = placeOnPrimary ? Screen.PrimaryScreen : screens.FirstOrDefault(screen => !screen.Primary);

                if (targetScreen != null)
                {
                    // Use the window's Invoke method to move it on the UI thread
                    if (window.InvokeRequired)
                    {
                        window.Invoke(new Action(() =>
                        {
                            MoveWindow(window, targetScreen);
                        }));
                    }
                    else
                    {
                        MoveWindow(window, targetScreen);
                    }
                }
            }
            else
            {
                // Display the message on the UI thread
                if (window.InvokeRequired)
                {
                    window.Invoke(new Action(() =>
                    {
                        MessageBox.Show("Only one monitor detected. Two monitors are required for this operation.", "Monitor Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
                else
                {
                    MessageBox.Show("Only one monitor detected. Two monitors are required for this operation.", "Monitor Check", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void MoveWindow(Form window, Screen targetScreen)
        {
            // Move the window to the selected screen
            window.StartPosition = FormStartPosition.Manual;
            window.Location = targetScreen.Bounds.Location;
        }
    }


}
