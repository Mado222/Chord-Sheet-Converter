using FeedbackDataLib.Modules.CADS1292x;

using FeedbackDataLib;


namespace FeedbackDataLib_GUI
{
    public partial class ucModuleSpecificSetup_ExGADS : UserControl
    {

        bool NO_ComboBox_SelectedIndexChanged = false;

        public CModuleExGADS1292 cModuleExGADS1292 { get; set; }

        public ucModuleSpecificSetup_ExGADS()
        {
            InitializeComponent();
            cModuleExGADS1292 = new CModuleExGADS1292
            {
                name = "ucModule"
            };
        }

        public byte[] GetModuleSpecificInfo()
        {
            GetComboBoxes();
            return cModuleExGADS1292.GetModuleSpecific();
        }

        public void SetModuleSpecificInfo(CModuleExGADS1292 ModuleInfo)
        {
            cModuleExGADS1292.SetModuleSpecific(ModuleInfo.GetModuleSpecific());
            cModuleExGADS1292.ElectrodeImpedance = ModuleInfo.ElectrodeImpedance;
            SetComboBoxes();
            SetImpedanceBoxes();
        }

        private void ucModuleSpecificSetup_ExGADS_Load(object sender, EventArgs e)
        {
            Update_dgvs();
        }

        private void Update_dgvs ()
        {
            NO_ComboBox_SelectedIndexChanged = true;
            //CONFIG1
            sample_ModeComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG1.enConfig1_bit7));
            sample_RateComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG1.enConfig1_bit2_0));

            //CONFIG2
            lead_offComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG2.enConfig2_bit6_Lead_off));
            refBufComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG2.enConfig2_bit5_REFBUF));
            vrefComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG2.enConfig2_bit4_VREF));
            oscoutComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG2.enConfig2_bit3_OscOut));
            testsignalComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG2.enConfig2_bit1_Testsignal));
            testfrequencyComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_CONFIG2.enConfig2_bit0_TestFrequency));

            //CHANNEL_CONFIG
            powerComboBox1.DataSource = Enum.GetValues(typeof(CADS1292x_CHANxSET.enChanxSet_bit7_Power));
            powerComboBox2.DataSource = Enum.GetValues(typeof(CADS1292x_CHANxSET.enChanxSet_bit7_Power));
            gainComboBox1.DataSource = Enum.GetValues(typeof(CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain));
            gainComboBox2.DataSource = Enum.GetValues(typeof(CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain));
            mUXComboBox1.DataSource = Enum.GetValues(typeof(CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX));
            mUXComboBox2.DataSource = Enum.GetValues(typeof(CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX));

            //LEAD_OFF
            compThresholdComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF.enLOFF_bit7_5_Comp_Threshold));
            leadOff_currentComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF.enLOFF_bit3_2_LeadOff_Current));
            leadOff_FrequencaComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF.enLOFF_bit0_LeadOff_Frequency));

            //LEAD_OFF_SENSE
            lOFF1PComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF_SENSE.enLOFFSENSE_bit0_LOFF1P));
            lOFF1NComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF_SENSE.enLOFFSENSE_bit1_LOFF1N));
            lOFF2PComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF_SENSE.enLOFFSENSE_bit2_LOFF2P));
            lOFF2NComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF_SENSE.enLOFFSENSE_bit3_LOFF2N));
            fLIP1ComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF_SENSE.enLOFFSENSE_bit4_FLIP1));
            fLIP2ComboBox.DataSource = Enum.GetValues(typeof(CADS1292x_LOFF_SENSE.enLOFFSENSE_bit5_FLIP2));
            NO_ComboBox_SelectedIndexChanged = false;

        }

        private void SetComboBoxes()
        {
            CModuleExGADS1292 mi = cModuleExGADS1292;
            NO_ComboBox_SelectedIndexChanged = true;
            //CONFIG1
            sample_ModeComboBox.SelectedItem = mi.CONFIG1[0].Sample_Mode;
            sample_RateComboBox.SelectedItem = mi.CONFIG1[0].Sample_Rate;

            //CONFIG2
            lead_offComboBox.SelectedItem = mi.CONFIG2[0].Lead_off;
            refBufComboBox.SelectedItem = mi.CONFIG2[0].RefBuf;
            vrefComboBox.SelectedItem = mi.CONFIG2[0].enVref;
            oscoutComboBox.SelectedItem = mi.CONFIG2[0].Oscout;
            testsignalComboBox.SelectedItem = mi.CONFIG2[0].Testsignal;
            testfrequencyComboBox.SelectedItem = mi.CONFIG2[0].Testfrequency;

            //CHANNEL_CONFIG
            powerComboBox1.SelectedItem = mi.CHANxSET[0].Power;
            powerComboBox2.SelectedItem = mi.CHANxSET[1].Power;
            gainComboBox1.SelectedItem = mi.CHANxSET[0].Gain;
            gainComboBox2.SelectedItem = mi.CHANxSET[1].Gain;
            mUXComboBox1.SelectedItem = mi.CHANxSET[0].MUX;
            mUXComboBox2.SelectedItem = mi.CHANxSET[1].MUX;

            //LEAD_OFF
            compThresholdComboBox.SelectedItem = mi.LEADOFF[0].CompThreshold;
            leadOff_currentComboBox.SelectedItem = mi.LEADOFF[0].LeadOff_current;
            leadOff_FrequencaComboBox.SelectedItem = mi.LEADOFF[0].LeadOff_AC_DC;

            //LEAD_OFF_SENSE
            lOFF1PComboBox.SelectedItem = mi.LEADOFF_SENSE[0].LOFF1P;
            lOFF1NComboBox.SelectedItem = mi.LEADOFF_SENSE[0].LOFF1N;
            lOFF2PComboBox.SelectedItem = mi.LEADOFF_SENSE[0].LOFF2P;
            lOFF2NComboBox.SelectedItem = mi.LEADOFF_SENSE[0].LOFF2N;
            fLIP1ComboBox.SelectedItem = mi.LEADOFF_SENSE[0].FLIP1;
            fLIP2ComboBox.SelectedItem = mi.LEADOFF_SENSE[0].FLIP2;
        }

        private void SetImpedanceBoxes()
        {
            //Electrode Information
            CADS1292x_ElectrodeImp mi = cModuleExGADS1292.ElectrodeImpedance;

            int cn = 1;
            electrode_imp_xnTextBox1.Text = (mi.Get_ElectrodeInfo_n(0).Impedance_Ohm / 1000).ToString("F0");
            uElektrode_xn_mVTextBox1.Text = (mi.Get_ElectrodeInfo_n(0).UElektrode_V * 1e6).ToString("F0");

            electrode_imp_xpTextBox1.Text = (mi.Get_ElectrodeInfo_p(0).Impedance_Ohm / 1000).ToString("F0");
            uElektrode_xp_mVTextBox1.Text = (mi.Get_ElectrodeInfo_p(0).UElektrode_V * 1e6).ToString("F0");

            electrode_imp_xnTextBox2.Text = (mi.Get_ElectrodeInfo_n(1).Impedance_Ohm / 1000).ToString("F0");
            uElektrode_xn_mVTextBox2.Text = (mi.Get_ElectrodeInfo_n(1).UElektrode_V * 1e6).ToString("F0");

            electrode_imp_xpTextBox2.Text = (mi.Get_ElectrodeInfo_p(1).Impedance_Ohm / 1000).ToString("F0");
            uElektrode_xp_mVTextBox2.Text = (mi.Get_ElectrodeInfo_p(1).UElektrode_V * 1e6).ToString("F0");


            NO_ComboBox_SelectedIndexChanged = false;
        }

        private void GetComboBoxes()
        {
            CModuleExGADS1292 mi = cModuleExGADS1292;
            mi.CONFIG1[0].Sample_Mode = (CADS1292x_CONFIG1.enConfig1_bit7)sample_ModeComboBox.SelectedItem;
            mi.CONFIG1[0].Sample_Rate = (CADS1292x_CONFIG1.enConfig1_bit2_0)sample_RateComboBox.SelectedItem;

            //CONFIG2
            mi.CONFIG2[0].Lead_off = (CADS1292x_CONFIG2.enConfig2_bit6_Lead_off) lead_offComboBox.SelectedItem;
            mi.CONFIG2[0].RefBuf = (CADS1292x_CONFIG2.enConfig2_bit5_REFBUF) refBufComboBox.SelectedItem;
            mi.CONFIG2[0].enVref = (CADS1292x_CONFIG2.enConfig2_bit4_VREF) vrefComboBox.SelectedItem;
            mi.CONFIG2[0].Oscout = (CADS1292x_CONFIG2.enConfig2_bit3_OscOut) oscoutComboBox.SelectedItem;
            mi.CONFIG2[0].Testsignal = (CADS1292x_CONFIG2.enConfig2_bit1_Testsignal) testsignalComboBox.SelectedItem;
            mi.CONFIG2[0].Testfrequency = (CADS1292x_CONFIG2.enConfig2_bit0_TestFrequency) testfrequencyComboBox.SelectedItem;

            //CHANNEL_CONFIG
            mi.CHANxSET[0].Power = (CADS1292x_CHANxSET.enChanxSet_bit7_Power) powerComboBox1.SelectedItem;
            mi.CHANxSET[1].Power = (CADS1292x_CHANxSET.enChanxSet_bit7_Power) powerComboBox2.SelectedItem;
            mi.CHANxSET[0].Gain = (CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain) gainComboBox1.SelectedItem;
            mi.CHANxSET[1].Gain = (CADS1292x_CHANxSET.enChanxSet_bit6_4_Gain) gainComboBox2.SelectedItem;
            mi.CHANxSET[0].MUX = (CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX) mUXComboBox1.SelectedItem;
            mi.CHANxSET[1].MUX = (CADS1292x_CHANxSET.enChanxSet_bit3_0_MUX) mUXComboBox2.SelectedItem;

            //LEAD_OFF
            mi.LEADOFF[0].CompThreshold = (CADS1292x_LOFF.enLOFF_bit7_5_Comp_Threshold) compThresholdComboBox.SelectedItem;
            mi.LEADOFF[0].LeadOff_current = (CADS1292x_LOFF.enLOFF_bit3_2_LeadOff_Current) leadOff_currentComboBox.SelectedItem;
            mi.LEADOFF[0].LeadOff_AC_DC = (CADS1292x_LOFF.enLOFF_bit0_LeadOff_Frequency) leadOff_FrequencaComboBox.SelectedItem;

            //LEAD_OFF_SENSE
            mi.LEADOFF_SENSE[0].LOFF1P = (CADS1292x_LOFF_SENSE.enLOFFSENSE_bit0_LOFF1P) lOFF1PComboBox.SelectedItem;
            mi.LEADOFF_SENSE[0].LOFF1N = (CADS1292x_LOFF_SENSE.enLOFFSENSE_bit1_LOFF1N)lOFF1NComboBox.SelectedItem;
            mi.LEADOFF_SENSE[0].LOFF2P = (CADS1292x_LOFF_SENSE.enLOFFSENSE_bit2_LOFF2P) lOFF2PComboBox.SelectedItem;
            mi.LEADOFF_SENSE[0].LOFF2N = (CADS1292x_LOFF_SENSE.enLOFFSENSE_bit3_LOFF2N)lOFF2NComboBox.SelectedItem;
            mi.LEADOFF_SENSE[0].FLIP1 = (CADS1292x_LOFF_SENSE.enLOFFSENSE_bit4_FLIP1)fLIP1ComboBox.SelectedItem;
            mi.LEADOFF_SENSE[0].FLIP2 = (CADS1292x_LOFF_SENSE.enLOFFSENSE_bit5_FLIP2)fLIP2ComboBox.SelectedItem;
        }



        private void btSetDefault_Click(object sender, EventArgs e)
        {
            //Siehe ExG mit ADS1292.docx
            byte[] buf = { 16, 177, 1, 161, 184, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            cModuleExGADS1292.SetModuleSpecific(buf);
            Update_dgvs();
        }

        private void ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!NO_ComboBox_SelectedIndexChanged)
                GetComboBoxes();
        }
    }
}

