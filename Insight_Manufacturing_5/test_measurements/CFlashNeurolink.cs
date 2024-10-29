using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Linq;
using System.Xml;
using WindControlLib;


namespace Insight_Manufacturing5_net8.tests_measurements
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="frmInsight_Manufacturing5.tests_measurements.uc_Base_tests_measurements"/>
    public class CFlashNeurolink : CBase_tests_measurements
    {
        public const string dir = @"C:\Insight Manufacturing\FTDI\"; //AppDomain.CurrentDomain.BaseDirectory + @"FTDI\";
        public const string pt = dir + @"ProgFTDI.bat";
        public const string confFile = @"cNeurolink.xml";
        public const string confFile_path = dir + confFile;

        public CFlashNeurolink()
        {
            Job_Message = "Flash Neurolink";
            ConnectedModuleType = FeedbackDataLib.enumModuleType.cNeurolink;
            Pre_Job_Message = "Neurolink mit Strom versorgen und USB mit PC verbinden";
        }

        public override bool Perform_Measurement(bool ignore_serial_number_check = false)
        {
            ModuleTestResult = enModuleTestResult.Fail;
            OnBeforeMeasurementStarts();
            /////////////////////////////

            if (File.Exists(confFile_path))
            {
                //Change Serial Number
                XmlDocument xmlDoc = new();
                xmlDoc.Load(confFile_path);
                XmlNode node = xmlDoc.SelectSingleNode(@"FT_EEPROM/USB_String_Descriptors/SerialNumber");
                node.InnerText = SerialNumber;
                xmlDoc.Save(confFile_path);
                ModuleTestResult = enModuleTestResult.Fail;

                OnReportMeasurementProgress("Programmiervorgang des Neurolinks: " + SerialNumber + " wird gestartet", Color.Blue);

                if (File.Exists(pt))
                {
                    try
                    {
                        string path = "\"" + pt + "\"";
                        System.Diagnostics.ProcessStartInfo psi =
                            new("cmd.exe", "/c " + path)
                            {
                                CreateNoWindow = true,
                                WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden,
                                UseShellExecute = false,
                                WorkingDirectory = dir
                            };

                        System.Diagnostics.Process batJob;

                        // *** Redirect the output ***
                        psi.RedirectStandardError = true;
                        psi.RedirectStandardOutput = true;

                        batJob = System.Diagnostics.Process.Start(psi);
                        batJob.WaitForExit(5000);
                        Application.DoEvents();

                        // *** Read the streams ***
                        // ToDo
                        string output = batJob.StandardOutput.ReadToEnd();
                        string error = batJob.StandardError.ReadToEnd();

                        int ExitCode = batJob.ExitCode;

                        output += ("output>>" + (String.IsNullOrEmpty(output) ? "(none)" : output));
                        output += ("error>>" + (String.IsNullOrEmpty(error) ? "(none)" : error));
                        output += ("ExitCode: " + ExitCode.ToString());
                        //OnReportMeasurementProgress(output, Color.Blue);

                        batJob.Close();

                        //Validy Check
                        if (output.Contains("programmed successfully!") && output.Contains("error>>(none)"))
                        {
                            ModuleTestResult = enModuleTestResult.OK;
                            OnReportMeasurementProgress("Neurulink " + SerialNumber + ": erfolgreich programmiert", Color.Green);
                        }
                    }
                    catch (Exception x)
                    {
                        OnReportMeasurementProgress("\n" + x.Message, Color.Red);
                    }
                }
                else
                {
                    OnReportMeasurementProgress("Error: File not found " + pt, Color.Red);
                }
            }
            else
            {
                OnReportMeasurementProgress("Error: File not found " + confFile_path, Color.Red);
            }


            if (ModuleTestResult == enModuleTestResult.OK)
                return true;
            return false;
        }

        /*
        public override void Save_Results_to_DB()
        {
            try
            {
                dsManufacturing _dsManufacturing = new dsManufacturing();
                dsManufacturingTableAdapters.NeurodevicesTableAdapter neurodevicesTableAdapter = new dsManufacturingTableAdapters.NeurodevicesTableAdapter();

                neurodevicesTableAdapter.FillBy_SerialNumber(_dsManufacturing.Neurodevices, SerialNumber);
                if (_dsManufacturing.Neurodevices.Count == 1)
                {
                    DateTime dt_prog = DateTime.Now;
                    _dsManufacturing.Neurodevices[0].Programmierdatum = dt_prog;
                    neurodevicesTableAdapter.Update(_dsManufacturing.Neurodevices);
                }
            }
            catch (Exception x)
            {
                OnReportMeasurementProgress("Device not found in DB / No Connection to DB\n" + x.ToString(), Color.Violet);
            }
        }*/
    }
}
