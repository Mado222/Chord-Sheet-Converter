using PhidgetLib;
using System;
using System.ComponentModel;
using System.Threading;

namespace Insight_Manufacturing5_net8
{
    public class CInsightModuleTesterV1
    {
        public const double Uoff_High_mV = 300;
        public const double Uoff_Low_mV = 30;
        private CPhidgetLib22 phglib;
        BackgroundWorker bgPhidget = null;
        private const int Phidg_Uoff_Polarity_OUT1 = 1;        //
        private const int Phidg_ICD_on_off_OUT2 = 2;        //On = 0
        private const int Phidg_Uoff_Low_High_OUT3 = 3;   //
        private const int Phidg_Uoff_On_Off_OUT6 = 6;   //
        private const int Phidg_EEG_On_Off_Out7 = 7;         //

        public bool PhidgetIsConnected
        {
            get
            {
                if (phglib != null)
                    return phglib.ph_attached;
                return false;
            }
        }

        #region Events
        public event CPhidgetLib22.ReportStatusEventHandler ReportMeasurementProgress;
        protected virtual void OnReportModuleTesterStatus(string text, System.Drawing.Color col)
            => ReportMeasurementProgress?.Invoke(this, text, col);
        #endregion

        public delegate void PhidgetConnectedEventHandler(object sender);
        public event PhidgetConnectedEventHandler PhidgetConnected; // event
        protected virtual void OnPhidgetConnected()
        {
            //if ProcessCompleted is not null then call delegate
            PhidgetConnected?.Invoke(this);
        }

        public static CInsightModuleTester_Settings Get_Default_Setting()
        {
            CInsightModuleTester_Settings InsightModuleTester =
            new CInsightModuleTester_Settings(
            CInsightModuleTester_Settings.enICD.ICD_Connected,
            CInsightModuleTester_Settings.enEEG.EEG_Off,
            CInsightModuleTester_Settings.enUoff.Uoff_Off,
            CInsightModuleTester_Settings.enUoffLevel.UoffLevel_Low,
            CInsightModuleTester_Settings.enUoffPolarity.Polarity_Plus);
            return InsightModuleTester;
        }

        public CInsightModuleTesterV1()
        {
            phglib = new CPhidgetLib22();
            phglib.ReportMeasurementProgress += Phglib_ReportMeasurementProgress;
            SetDefaultValues();
        }

        private void Phglib_ReportMeasurementProgress(object sender, string text, System.Drawing.Color col)
        {
            OnReportModuleTesterStatus(text, col);
        }

        private void Start_Phidget_Search()
        {
            if (bgPhidget == null)
            {
                bgPhidget = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true
                };
                bgPhidget.DoWork += BgPhidget_DoWork;
            }
            if (!bgPhidget.IsBusy)
                bgPhidget.RunWorkerAsync();
        }

        const int cretryPhidget = 5;
        private void BgPhidget_DoWork(object sender, DoWorkEventArgs e)
        {
            int retryPhidget = cretryPhidget;
            while (phglib.Open() == false)
            {
                if (bgPhidget.CancellationPending)
                    break;
                Thread.Sleep(2500);
                if (retryPhidget-- <= 0)
                    bgPhidget.CancelAsync();
            }
            if (phglib.Open())
                OnPhidgetConnected();
            else
                OnReportModuleTesterStatus("Phidget searching stopped after " + cretryPhidget.ToString() + " retries", System.Drawing.Color.Violet);
        }


        public bool Open()
        {
            if (phglib.Open())
            {
                OnPhidgetConnected();
                return true;
            }
            else
            {
                //Start Phidget Searching
                Start_Phidget_Search();
            }
            return false;
        }

        public void Close()
        {
            if (bgPhidget != null)
                bgPhidget.CancelAsync();
            phglib.Close();
        }

        public void SetDefaultValues()
        {
            if (phglib != null)
            {
                //Set Default Values
                if (phglib.ph_attached)
                {
                    Init(CInsightModuleTesterV1.Get_Default_Setting());
                }
            }
        }

        public bool Init(CInsightModuleTester_Settings InsightModuleTester_Settings)
        {
            bool ret = false;
            int repeat_setting = 10;
            if (phglib.ph_attached)
            {
                while (repeat_setting > 0)
                {
                    ret = true;
                    switch (InsightModuleTester_Settings.EEG)
                    {
                        case CInsightModuleTester_Settings.enEEG.EEG_On:
                            setret(ref ret, EEG_Connect());
                            break;
                        default:
                            setret(ref ret, EEG_DisConnect());
                            break;
                    }
                    switch (InsightModuleTester_Settings.ICD_State)
                    {
                        case CInsightModuleTester_Settings.enICD.ICD_Connected:
                            setret(ref ret, ICD_Connect());
                            break;
                        case CInsightModuleTester_Settings.enICD.ICD_DisConnected:
                            setret(ref ret, ICD_DisConnect());
                            break;
                    }

                    switch (InsightModuleTester_Settings.Uoff)
                    {
                        case CInsightModuleTester_Settings.enUoff.Uoff_On:
                            setret(ref ret, Uoff_On());
                            break;
                        default:
                            setret(ref ret, Uoff_Off());
                            break;
                    }

                    switch (InsightModuleTester_Settings.UoffLevel)
                    {
                        case CInsightModuleTester_Settings.enUoffLevel.UoffLevel_High:
                            setret(ref ret, Uoff_Level_High());
                            break;
                        default:
                            setret(ref ret, Uoff_Level_Low());
                            break;
                    }

                    switch (InsightModuleTester_Settings.UoffPolarity)
                    {
                        case CInsightModuleTester_Settings.enUoffPolarity.Polarity_Plus:
                            setret(ref ret, Uoff_Polarity_Plus());
                            break;
                        default:
                            setret(ref ret, Uoff_Polarity_Minus());
                            break;
                    }

                    repeat_setting--;
                    if (ret) break;
                    else WindControlLib.CDiverses.Wait(100);
                }
            }
            return ret;
        }

        private void setret(ref bool ret, bool val)
        {
            if (val == false)
                ret = val;
        }

        public bool ICD_Connect()
        {
            return SetCheckOutput(Phidg_ICD_on_off_OUT2, true);
        }
        public bool ICD_DisConnect()
        {
            return SetCheckOutput(Phidg_ICD_on_off_OUT2, false);
        }
        public bool EEG_Connect()
        {
            return SetCheckOutput(Phidg_EEG_On_Off_Out7, true);
        }
        public bool EEG_DisConnect()
        {
            return SetCheckOutput(Phidg_EEG_On_Off_Out7, false);
        }
        public bool Uoff_On()
        {
            return SetCheckOutput(Phidg_Uoff_On_Off_OUT6, true);
        }
        public bool Uoff_Off()
        {
            return SetCheckOutput(Phidg_Uoff_On_Off_OUT6, false);
        }
        public bool Uoff_Polarity_Plus()
        {
            return SetCheckOutput(Phidg_Uoff_Polarity_OUT1, false);
        }
        public bool Uoff_Polarity_Minus()
        {
            return SetCheckOutput(Phidg_Uoff_Polarity_OUT1, true);
        }
        public bool Uoff_Level_High()
        {
            return SetCheckOutput(Phidg_Uoff_Low_High_OUT3, false);
        }
        public bool Uoff_Level_Low()
        {
            return SetCheckOutput(Phidg_Uoff_Low_High_OUT3, true);
        }



        public bool SetCheckOutput(int output_number, bool value)
        {
            phglib.SetOutPut(output_number, value);
            WindControlLib.CDiverses.Wait(20);             //Wait for Relais to Set
            if (phglib.GetOutPut(output_number) != value)
                return false;
            return true;
        }
    }

    public class CInsightModuleTester_Settings : ICloneable
    {
        public enum enUoffPolarity
        {
            Polarity_Plus,
            Polarity_Minus
        }
        public enum enICD
        {
            ICD_Connected,
            ICD_DisConnected
        }
        public enum enUoffLevel
        {
            UoffLevel_Low,
            UoffLevel_High
        }
        public enum enUoff
        {
            Uoff_On,
            Uoff_Off
        }
        public enum enEEG
        {
            EEG_On,
            EEG_Off
        }

        public enUoffPolarity UoffPolarity { get; set; }
        public enICD ICD_State { get; set; }
        public enUoffLevel UoffLevel { get; set; }
        public enUoff Uoff { get; set; }
        public enEEG EEG { get; set; }

        public CInsightModuleTester_Settings()
        { }

        public CInsightModuleTester_Settings(enICD ICD_State, enEEG EEG = enEEG.EEG_Off, enUoff Uoff = enUoff.Uoff_Off, enUoffLevel UoffLevel=enUoffLevel.UoffLevel_Low, enUoffPolarity polarity=enUoffPolarity.Polarity_Plus)
        {
            UoffPolarity = polarity;
            this.ICD_State = ICD_State;
            this.UoffLevel = UoffLevel;
            this.Uoff = Uoff;
            this.EEG = EEG;
        }

        public object Clone()
        {
            return new CInsightModuleTester_Settings(ICD_State, EEG, Uoff, UoffLevel, UoffPolarity);
        }
    }
}
