using System.Text;
using BMTCommunication;

namespace ComponentsLib_GUI
{
    /// <summary>
    /// Klasse für Funktionsgenerator
    /// </summary>
    public class CFY6900
    {
        public const string DriverName = "CH340";
        //Arbitrary1 Command Code "WMW36" --- Achtung - lt Manual 37
        public const int Arbitraty_Chan_No_1 = 36;
        public const int Resolution_bit = 14;
        public const int NumValues_Arbitrary  = 8192;

        private CSerialPortWrapper _Seriell32;
        private readonly byte[] default_return_value = { 0xa };

        private List<byte> LastReturnValue = new List<byte>();
        private bool _isOpen = false;

        public CFY6900()
        {
        }

        public CFY6900(CSerialPortWrapper Seriell32)
        {
            _Seriell32 = Seriell32;
            _isOpen = false;
        }

        public bool Open()
        {
            if (!_isOpen)
            {
                UCComPortSelector ucs = new UCComPortSelector();
                ucs.Init(DriverName);
                if (ucs.Items.Count == 1)
                {
                    if (_Seriell32 == null)
                    {
                        _Seriell32 = new CSerialPortWrapper();
                    }
                    _Seriell32.PortName = (string)ucs.SelectedItem;
                    _Seriell32.BaudRate = 115200;
                    _Seriell32.Open();
                    if (_Seriell32.IsOpen)
                    {
                        //Check for Generator
                        if (SetOutput_Off(true))
                        {
                            _isOpen = true;
                        }
                    }

                }
            }
            return isOpen;
        }

        public bool isOpen { get => _isOpen; }

        public void Close()
        {
            if (_Seriell32 != null)
                _Seriell32.Close();
            _isOpen = false;
        }

        public byte[] GetLastReturnValue()
        {
            return LastReturnValue.ToArray();
        }

        public bool SetOutput_On(bool check_return_value)
        {
            return Send_Command("WMN1", default_return_value, check_return_value);
        }
        public bool SetOutput_Off(bool check_return_value)
        {
            return Send_Command("WMN0", default_return_value, check_return_value);
        }

        public bool SetFrequency(double f, bool check_return_value)
        {
            int ff = (int)(f * 1e6); //[µHz]
            string val = "WMF" + ff.ToString();
            return Send_Command(val, default_return_value, check_return_value);
        }

        public bool SetVss(double Vss, bool check_return_value)
        {
            string val = "WMA" + String.Format("{0:0.00}", Vss);
            val = val.Replace(",", ".");
            return Send_Command(val, default_return_value, check_return_value);
        }

        /// <summary>
        ///   <para>
        ///  Selct Sinus as Output Wave</para>
        /// </summary>
        /// <param name="check_return_value">if set to <c>true</c> [check return value].</param>
        /// <returns></returns>
        public bool SetSinus(bool check_return_value)
        {
            return Send_Command("WMW0", default_return_value, check_return_value);
        }

        /// <summary>  Select Arbitrary number "ChanNo" as Output Wave</summary>
        /// <param name="ChanNo">
        ///   <para>
        ///  Channe number>=1</para>
        /// </param>
        /// <param name="check_return_value">if set to <c>true</c> [check return value].</param>
        /// <returns></returns>
        public bool SetArbitrary(int ChanNo, bool check_return_value)
        {
            if (ChanNo >= 1)
            {
                ChanNo = Arbitraty_Chan_No_1 + ChanNo - 1;
                return Send_Command("WMW" + ChanNo.ToString(), default_return_value, check_return_value);
            }
            return false;
        }

        /*
        /// <summary>Uploads the arbitratry waveform.</summary>
        /// <param name="values">  8192 values between +1 and -1</param>
        /// <param name="ChanNo">  Arbitrary Channel No - 1, 2, ...</param>
        /// <returns></returns>
        public bool UploadArbitratryWaveform (double [] values, int ChanNo)
        {
            //First send
            //"DDS_WAVE03"+0xA
            string s = "DDS_WAVE" + ChanNo.ToString("D2");
            List<byte> outbuf = new List<byte>(ASCIIEncoding.ASCII.GetBytes(s));
            outbuf.Add(0x0A);
            Send_Command(outbuf.ToArray());
            //outbuf.AddRange (outbuf);
            if (Send_Command(outbuf.ToArray(), new byte[] { 0x57, 0x0A }))
            {
                //Make value array
                outbuf.Clear();
                double LSB = 2.0 / (1 << Resolution_bit);
                UInt16 val16 = 0;
                UInt16 [] val16all = new UInt16[values.Length];

                values[0] = 1;

                for (int i = 0; i < values.Length; i++)
                {
                    if (i>0) values[i] = 0;
                    if (values[i] < -1) values[i] = -1;
                    if (values[i] > 1) values[i] = 1;

                    val16 = (UInt16)((values[i] + 1) / LSB);
                    if (val16 != 0) val16 -= 1;
                    outbuf.AddRange(BitConverter.GetBytes(val16));
                    val16all[i] = val16;
                }
                outbuf.AddRange(new byte [] {0,0,0,0,0,0,0 });
                WindControlLib.CDelay.Delay_ms(500);

                if (Send_Command(outbuf.ToArray(), new byte[] { 0x48, 0x0A }))
                {
                    return true;
                }
            }
            return false;
        }

        public void Init_China()
        {
            string[] cmds = new string[]
            {
                "UMO","UVE","RSA0","RSA1","RSA2",
                "RSA3","RSA4","RBZ","RMS","RUL","RMW",
                "RMF","RMA","RMO","RMD","RMP","RMT","RMN",
                "RPF","RPM","RFK","RPN","RPR","RFM","RPP","RFW","RFF","RFA","RFO","RFD","RFP","RFT","RFN"};

            foreach (string s in cmds)
            {
                Send_Command(s);
                WindControlLib.CDelay.Delay_ms(50);
            }
        }*/

        public int GetWaveform()
        {
            //0 = Sinus
            _Seriell32.DiscardInBuffer();
            if (Send_Command("RMW", default_return_value, false))
            {
                byte[] return_value = GetResponse();
                if (return_value != null)
                {
                    string res = ASCIIEncoding.ASCII.GetString(return_value);
                    return Convert.ToInt32(res);
                }
            }
            return -1;
        }

        public double GetFrequency()
        {
            //00000120.000000 -> 120Hz

            _Seriell32.DiscardInBuffer();
            if (Send_Command("RMF", default_return_value, false))
            {
                byte[] return_value = GetResponse();
                if (return_value != null)
                {
                    string res = ASCIIEncoding.ASCII.GetString(return_value);
                    res = res.Replace(".", ",");
                    return Convert.ToDouble(res);
                }
            }
            return -1;
        }

        public double GetVss()
        {
            //60000 -> 6V

            _Seriell32.DiscardInBuffer();
            if (Send_Command("RMA", default_return_value, false))
            {
                byte[] return_value = GetResponse();
                if (return_value != null)
                {
                    string res = ASCIIEncoding.ASCII.GetString(return_value);
                    res = res.Replace(".", ",");
                    return Convert.ToDouble(res) / 10000;
                }
            }
            return -1;
        }


        private byte[] GetResponse()
        {
            List<byte> ret = new List<byte>();
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, 20);

            while (DateTime.Now < dt)
            {
                if (_Seriell32.BytesToRead > 0)
                {
                    byte[] btret = new byte[_Seriell32.BytesToRead];
                    _Seriell32.Read(ref btret, 0, _Seriell32.BytesToRead);
                    ret.AddRange(btret);
                    if (btret.Last() == 0xa)
                    {
                        btret = ret.Where(val => val != 0xa).ToArray(); //remove all 0xa
                        return btret;
                    }
                }
            }
            return null;
        }

        private bool Send_Command(string s, byte[] return_value, bool check_return_value)
        {
            if (check_return_value)
            {
                return Send_Command(s, return_value);
            }
            return Send_Command(s); ;
        }



        /// <summary>  Converts s to ASCII and adds 0x0A</summary>
        /// <param name="s">  String Command to send</param>
        /// <param name="return_value">Return value from FY6900 - will be checked</param>
        /// <returns></returns>
        private bool Send_Command(string s, byte[] return_value = null)
        {
            List<byte> outbuf = new List<byte>(ASCIIEncoding.ASCII.GetBytes(s)) {0x0a};
            return Send_Command(outbuf.ToArray(), return_value);
        }
        private bool Send_Command(byte[] outbuf, byte[] return_value = null)
    {
            bool bret = true;
            if (_Seriell32.IsOpen)
            {
                _Seriell32.DiscardInBuffer();
                _Seriell32.DiscardOutBuffer();
                _Seriell32.Write(outbuf.ToArray(), 0, outbuf.Length);

                if (return_value != null)
                {
                    //Check return value
                    bret = false;
                    byte[] ret = new byte[return_value.Length];
                    _Seriell32.ReadTimeout = 500;
                    if (_Seriell32.Read(ref ret, 0, return_value.Length) == return_value.Length)
                    {
                        if (ret != null)
                        {
                            LastReturnValue = new List<byte>(ret);
                            bret = ret.SequenceEqual(return_value);
                        }
                    }
                }
                return bret;
            }
            return false;
        }
    }
}
