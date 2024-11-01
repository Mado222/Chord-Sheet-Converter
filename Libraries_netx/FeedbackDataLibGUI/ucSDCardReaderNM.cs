using FeedbackDataLib;
using FeedbackDataLib.Modules;
using System.Diagnostics;
using WindControlLib;

namespace FeedbackDataLib_GUI
{
    public partial class SDCardReaderNM : UserControl
    {
        /// <summary>
        /// Ringpuffer for Status Messages (used from tmrStatusMessages)
        /// </summary>
        private readonly CRingpuffer StatusMessages = new(2048);

        //Handles the Deecoding
        private readonly CSDCardImport SDCardImporter = new();

        public SDCardReaderNM()
        {
            InitializeComponent();

            //To get back some information about the import process
            SDCardImporter.ImportInfo += new CSDCardImport.ImportInfoEventHandler(SDCardImporter_ImportInfo);
        }


        //Initialises the Import
        private void BtReadBack_Click(object sender, EventArgs e)
        {
            //Start Import and get Configuration
            SDCardImporter.Sync_Packet_has_4_bytes = cbSync_has_4bytes.Checked;
            string sourcePath = txtSourcePath.Text;
            char[] charsToTrim = ['*', ' ', '\'', '\"'];
            sourcePath = sourcePath.Trim(charsToTrim);

            List<CModuleBase> mi = SDCardImporter.StartImport(sourcePath);
            if (cbUseSourcePath.Checked)
            {
                string destPath = sourcePath;
                //Make fielname
                string[] ss = destPath.Split(new char[] { '\\' }, System.StringSplitOptions.RemoveEmptyEntries);
                string fn = ss[^2] + "_" + ss[^1];
                fn = fn.Replace("-", "");

                /*
                 * Auswertung f Hanusch-Pia Daten
                string fn = ss[6] + "_2021";
                string[] dt = ss[8].Split(new char[] { '-' });
                fn += dt[1] + dt[2];
                */
                destPath += @"\" + fn + ".txt";
                txtDestination.Text = destPath;
            }

            //Update GUI
            if (mi != null)
            {
                AddStatusString("DeviceConfig received", Color.Green);  //Meldung anzeigen

                //Kunfiguartion an die Anzeige übergeben
                cChannelsControlV2x11.SetModuleInfos(mi);
                cChannelsControlV2x11.Refresh();
            }
            else
            {
                AddStatusString("Error during DeviceConfig", Color.Red);
            }

            //Put Importing to extra Thread
            backgroundWorker1.RunWorkerAsync();
            AddStatusString("Job Started");
        }


        //Does the importing work
        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            //Variables for progress report
            long cnt_progress = 0;
            long cnt_runs = 0;

            //To measure execution time
            Stopwatch stopw = new();

            try
            {
                stopw.Start();  //Start Stopwatch
                long ten_percent_filelength = SDCardImporter.DataFileLength / 10;    //Steps to report progress

                //Schreibfile öffnen
                if (OpenFile())
                {
                    while (!backgroundWorker1.CancellationPending)
                    {
                        //Progress report
                        if (SDCardImporter.BytesRead >= cnt_progress)
                        {
                            cnt_progress += ten_percent_filelength;
                            double d = SDCardImporter.BytesRead;
                            d = d / SDCardImporter.DataFileLength * 100;
                            backgroundWorker1.ReportProgress((int)d);
                        }

                        cnt_runs++;
                        //if (cnt_runs == 1498)
                        //    cnt_runs += 10;
                        CSDCardImport.CDataIn_Scaled cdis;
                        //Read next Data Point
                        cdis = SDCardImporter.GetNextValue();
                        if (cdis != null)
                        {
                            cdis.VirtualID = cChannelsControlV2x11.GetModuleInfo(cdis.HW_cn).SWChannels[cdis.SW_cn].VirtualID;
                            //Data Point is valid, do something ... write to file
                            sw.WriteLine(
                                cdis.DT_relative.ToLongTimeString() + "," +
                                cdis.DT_relative.Millisecond.ToString("000") + "\t" +
                                cdis.Value_Scaled.ToString() + "\t" +
                                cdis.HW_cn.ToString() + "\t" +
                                cdis.SW_cn.ToString() + "\t" +
                                cdis.Resync.ToString() + "\t" +
                                cdis.Value.ToString("X"));
                        }
                        else
                        {
                            //Cdis is null, no more bytes in file
                            backgroundWorker1.ReportProgress(100);
                            backgroundWorker1.CancelAsync();
                        }
                    }
                }
                else
                {
                    //File to write to could not be opened
                    backgroundWorker1.CancelAsync();
                }
            }

            catch (Exception ee)
            {
                AddStatusString("Ended with exception: " + ee.Message + Environment.NewLine + "Runs: " + cnt_runs.ToString());
            }
            finally
            {
                AddStatusString("Thread ended");
                CloseFile();
                SDCardImporter.StopImport();        //Close SDCardImporter
                stopw.Stop();

                //Perfomance report
                double d = stopw.ElapsedMilliseconds;
                d /= 1000;
                string report = SDCardImporter.DataFileLength.ToString() + " byte processed in " + d.ToString("F2") + " seconds";
                //report = d.ToString("F2") + " seconds";
                AddStatusString(report, Color.Green);
            }

        }



        #region AddStatusString
        private void AddStatusString(string text)
        {
            AddStatusString(text, Color.Black);
        }

        private void AddStatusString(string text, Color Col)
        {
            AddStatusString(txtStatus, text, Col);
        }

        private void AddStatusString(RichTextBox txtStatus, string text, Color Col)
        {
            CTextCol tc = new(text, Col);
            StatusMessages.Push(tc);
        }

        private class CTextCol(string text, Color Col)
        {
            public string text = text;
            public Color col = Col;
        }

        private void tmrStatusMessages_Tick(object sender, EventArgs e)
        {
            while (StatusMessages.StoredObjects > 0)
            {
                /*
                if (txtStatus.Lines.Length == 12)
                    txtStatus.Clear();*/
                CTextCol tc = StatusMessages.Pop() as CTextCol;

                txtStatus.AppendText(Environment.NewLine);
                txtStatus.SelectionColor = tc.col;
                txtStatus.AppendText(tc.text);
            }
        }
        #endregion

        void SDCardImporter_ImportInfo(object sender, string Info, Color col)
        {
            AddStatusString(Info, col);
        }


        private void btCancleJob_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        ///////////////////
        /// Routiners for file handling
        /// ///////////////////

        //For file to write to
        private StreamWriter? sw;
        private bool OpenFile()
        {
            try
            {
                sw = new StreamWriter(txtDestination.Text, false);

                DateTime dt = DateTime.Now;
                sw.Write("File Exported: " + DateTime.Now.ToString() + Environment.NewLine);

                sw.WriteLine("-------------");
                //sw.WriteLine(txtComment.Text);
                sw.WriteLine("-------------");

                sw.WriteLine();

                sw.WriteLine("Time" + "\t" +
                    "Value" + "\t" +
                    "HW CHannel number" + "\t" +
                    "SW CHannel number" + "\t" +
                    "Resync" + "\t" +
                    "Value [hex]");
            }
            catch (Exception ee)
            {
                AddStatusString(ee.Message, Color.Red);
                return false;
            }
            return true;
        }

        private void CloseFile()
        {
            //Stop saving
            sw?.Close();
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            saveFileDialog_data.FileName = System.IO.Path.GetFileName(txtDestination.Text);
            saveFileDialog_data.InitialDirectory = System.IO.Path.GetDirectoryName(txtDestination.Text);
            if (saveFileDialog_data.ShowDialog() == DialogResult.OK)
            {
                txtDestination.Text = saveFileDialog_data.FileName;
            }
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(txtSourcePath.Text))
                folderBrowserDialog1.SelectedPath = txtSourcePath.Text;

            if (folderBrowserDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //Check if Path is valid
                if (CSDCardImport.Get_DateTime_FromDirectory(folderBrowserDialog1.SelectedPath) != DateTime.MinValue)
                {

                    txtSourcePath.Text = folderBrowserDialog1.SelectedPath;
                }
                else
                    AddStatusString("Invalid Source Path", Color.Red);
            }
        }


    }
}
