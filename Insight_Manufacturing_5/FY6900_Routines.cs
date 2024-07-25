using ComponentsLib_GUI;

namespace Insight_Manufacturing5_net8
{
    public partial class frmInsight_Manufacturing5 : Form
    {
        private void cToggleButton_COM_ToState2(object sender, EventArgs e)
        {
            //OpenCOM
            Seriell32.Open();

            if (!Seriell32.IsOpen)
            {
                ((CToggleButton)sender).AcceptChange = false;
            }
        }

        private void cToggleButton_COM_ToState1(object sender, EventArgs e)
        {
            if (Seriell32 != null)
                Seriell32.Close();
        }

        private void btSetSinus_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: Sinus ";
            if (FY6900.SetSinus(true))
            {
                int i = FY6900.GetWaveform();
                if (i == 0)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_002_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_002 ";
            if (FY6900.SetArbitrary(1, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 36)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_003_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_003 ";
            if (FY6900.SetArbitrary(2, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 37)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_004_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_004 ";
            if (FY6900.SetArbitrary(3, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 38)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void btArtifECG_005_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: ArtifECG_005 ";
            if (FY6900.SetArbitrary(4, true))
            {
                int i = FY6900.GetWaveform();
                if (i == 39)
                    stat += "OK";
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        private void setFrequency_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: f ";
            if (FY6900.SetFrequency((double) numericUpDownFrequqency.Value, true))
            {
                double i = FY6900.GetFrequency();
                if (i > -1)
                    stat += "OK " + i.ToString();
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);

        }

        private void btSetAmplitude_Click(object sender, EventArgs e)
        {
            string stat = "FY6900: Vss ";
            if (FY6900.SetVss((double)numericUpDownAmplitude.Value, true))
            {
                double i = FY6900.GetVss();
                if (i > -1)
                    stat += "OK " + i.ToString();
                else stat += "Read back failed";
            }
            else
                stat += "Set failed";
            txtStatus.AddStatusString(stat, Color.Green);
        }

        public CInsightModuleTester_Settings Settings_To_Testboard ()
        {
            CInsightModuleTester_Settings ret = new CInsightModuleTester_Settings();
            if (cbPhi_ICDOn.Checked)
            {
                //ICD on
                ret.ICD_State = CInsightModuleTester_Settings.enICD.ICD_Connected;
            }
            else
            {
                //ICD off
                ret.ICD_State = CInsightModuleTester_Settings.enICD.ICD_DisConnected;
            }

            if (cbOffsetOn.Checked)
            {
                //Offset on
                ret.Uoff = CInsightModuleTester_Settings.enUoff.Uoff_On;
            }
            else
            {
                //Offset off
                ret.Uoff = CInsightModuleTester_Settings.enUoff.Uoff_Off;
            }

            if (cbOffsetPlus.Checked)
            {
                //Offset Plus
                ret.UoffPolarity = CInsightModuleTester_Settings.enUoffPolarity.Polarity_Plus;
            }
            else
            {
                //Offset Minus
                ret.UoffPolarity = CInsightModuleTester_Settings.enUoffPolarity.Polarity_Minus;
            }

            if (cbEEGOn.Checked)
            {
                //EEG on
                ret.EEG = CInsightModuleTester_Settings.enEEG.EEG_On;
            }
            else
            {
                //EEG off
                ret.EEG = CInsightModuleTester_Settings.enEEG.EEG_Off;
            }

            if (rbOffsetLow.Checked)
            {
                ret.UoffLevel = CInsightModuleTester_Settings.enUoffLevel.UoffLevel_Low;
            }

            if (rbOffsetHigh.Checked)
            {
                ret.UoffLevel = CInsightModuleTester_Settings.enUoffLevel.UoffLevel_High;
            }

            InsightModuleTestBoardV1.Init(ret);

            return ret;
        }

        delegate void Update_from_TestboardDelegate(CInsightModuleTester_Settings settings);
        public void Update_from_Testboard(CInsightModuleTester_Settings settings)
        {
            if (cbPhi_ICDOn.InvokeRequired)
            {
                Invoke(
                    new Update_from_TestboardDelegate(Update_from_Testboard),
                    new object[] { settings });
            }
            else
            {
                UpdateBoard = false;
                if (settings.ICD_State == CInsightModuleTester_Settings.enICD.ICD_Connected)
                    cbPhi_ICDOn.Checked = true;
                else
                    cbPhi_ICDOn.Checked = false;

                if (settings.Uoff == CInsightModuleTester_Settings.enUoff.Uoff_On)
                    cbOffsetOn.Checked = true;
                else
                    cbOffsetOn.Checked = false;

                if (settings.UoffPolarity == CInsightModuleTester_Settings.enUoffPolarity.Polarity_Plus)
                    cbOffsetPlus.Checked = true;
                else
                    cbOffsetPlus.Checked = false;

                if (settings.EEG == CInsightModuleTester_Settings.enEEG.EEG_On)
                    cbEEGOn.Checked = true;
                else
                    cbEEGOn.Checked = false;

                if (settings.UoffLevel == CInsightModuleTester_Settings.enUoffLevel.UoffLevel_Low)
                    rbOffsetLow.Checked = true;
                else
                    rbOffsetHigh.Checked = true;

                UpdateBoard = true;
            }
        }

        bool UpdateBoard = true;

        private void cbPhi_ICDON_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void cbOffsetOn_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void cbOffsetPlus_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void cbEEGOn_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void rbOffsetLow_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }

        private void rbOffsetHigh_CheckedChanged(object sender, EventArgs e)
        {
            if (UpdateBoard)
                Settings_To_Testboard();
        }
    }
}
