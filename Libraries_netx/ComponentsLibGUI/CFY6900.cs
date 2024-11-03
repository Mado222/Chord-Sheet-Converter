using BMTCommunicationLib;
using System.Runtime.Versioning;
using System.Text;

namespace ComponentsLib_GUI
{
    [SupportedOSPlatform("windows")]
    /// <summary>
    /// Klasse für Funktionsgenerator
    /// </summary>
    public class CFY6900
    {
        public const string DriverName = "CH340";
        //Arbitrary1 Command Code "WMW36" --- Achtung - lt Manual 37
        public const int ArbitratyChanNo1 = 36;
        public const int ResolutionBit = 14;
        public const int NumValuesArbitrary = 8192;

        private CSerialPortWrapper _Seriell32 = new();
        private readonly byte[] default_return_value = [0xa];
        private List<byte> LastReturnValue = [];

        public bool IsOpen { get => _Seriell32.IsOpen; }

        public string ComPort
        {
            get
            {
                if (_Seriell32?.PortName is not null)
                    return _Seriell32.PortName;
                return "";
            }
        }

        public string FwVersion { get; private set; } = string.Empty;

        public UCComPortSelector? ucs = new();

        public List<string>? GetRelatedComPorts()
        {
            List<string>? ret = null;
            if (ucs is not null && ucs.Items is not null)
                ret = ucs.Items.Cast<string>().ToList();
            return ret;
        }


        public CFY6900()
        {
            _Seriell32 = new CSerialPortWrapper();
            ucs.Init(DriverName);
        }

        public bool Open(string ComPort = "")
        {
            Close();
            if (ComPort == "")
            {
                if (ucs?.SelectedItem is not null && ucs?.Items.Count == 1)
                {
                    ComPort = (string)ucs.SelectedItem;
                }
            }

            if (ComPort != "")
            {
                _Seriell32 ??= new CSerialPortWrapper();
                _Seriell32.PortName = ComPort;
                _Seriell32.BaudRate = 115200;
                _Seriell32.GetOpen();
                if (_Seriell32.IsOpen)
                {
                    //Check for Generator
                    if (SetOutputOff(true))
                    {
                        FwVersion = GetFWVersion();
                    }
                }
            }
            return IsOpen;
        }

        public void Close()
        {
            _Seriell32?.Close();
        }

        public byte[] GetLastReturnValue()
        {
            return [.. LastReturnValue];
        }

        public bool SetOutputOn(bool checkReturnValue)
        {
            return Send_Command("WMN1", default_return_value, checkReturnValue);
        }
        public bool SetOutputOff(bool checkReturnValue)
        {
            return Send_Command("WMN0", default_return_value, checkReturnValue);
        }

        public bool SetFrequency(double f, bool checkReturnValue)
        {
            if (FwVersion != string.Empty)
            {
                string val = "";
                //FY6900 FW 1.3
                if (FwVersion == "V1.3")
                {
                    int ff = (int)(f * 1e6); //[µHz]
                    val = "WMF" + ff.ToString("00000000000000");
                }
                else if (FwVersion == "V1.5")
                {
                    //FY6900 FW 1.5
                    val = "WMF" + f.ToString("00000000.000000").Replace(".", "");//
                }

                return Send_Command(val, default_return_value, checkReturnValue);

            }
            return false;
        }

        public bool SetVss(double Vss, bool checkReturnValue)
        {
            string val = "WMA" + string.Format("{0:0.00}", Vss);
            val = val.Replace(",", ".");
            return Send_Command(val, default_return_value, checkReturnValue);
        }

        /// <summary>
        ///   <para>
        ///  Selct Sinus as Output Wave</para>
        /// </summary>
        /// <param name="checkReturnValue">if set to <c>true</c> [check return value].</param>
        /// <returns></returns>
        public bool SetSinus(bool checkReturnValue)
        {
            return Send_Command("WMW0", default_return_value, checkReturnValue);
        }

        /// <summary>  Select Arbitrary number "ChanNo" as Output Wave</summary>
        /// <param name="ChanNo">
        ///   <para>
        ///  Channe number>=1</para>
        /// </param>
        /// <param name="checkReturnValue">if set to <c>true</c> [check return value].</param>
        /// <returns></returns>
        public bool SetArbitrary(int ChanNo, bool checkReturnValue)
        {
            if (ChanNo >= 1)
            {
                ChanNo = ArbitratyChanNo1 + ChanNo - 1;
                return Send_Command("WMW" + ChanNo.ToString(), default_return_value, checkReturnValue);
            }
            return false;
        }


        public string GetFWVersion()
        {
            //V1.5
            //V1.3
            _Seriell32?.DiscardInBuffer();
            if (Send_Command("UVE", default_return_value, false))
            {
                byte[]? return_value = GetResponse();
                if (return_value != null)
                {
                    return Encoding.ASCII.GetString(return_value);
                }
            }
            return string.Empty;

        }


        public int GetWaveform()
        {
            //0 = Sinus
            _Seriell32?.DiscardInBuffer();
            if (Send_Command("RMW", default_return_value, false))
            {
                byte[]? return_value = GetResponse();
                if (return_value != null)
                {
                    string res = Encoding.ASCII.GetString(return_value);
                    return Convert.ToInt32(res);
                }
            }
            return -1;
        }

        public double GetFrequency()
        {
            //00000120.000000 -> 120Hz

            _Seriell32?.DiscardInBuffer();
            if (Send_Command("RMF", default_return_value, false))
            {
                byte[]? return_value = GetResponse();
                if (return_value != null)
                {
                    string res = Encoding.ASCII.GetString(return_value);
                    res = res.Replace(".", ",");
                    return Convert.ToDouble(res);
                }
            }
            return -1;
        }

        public double GetVss()
        {
            //60000 -> 6V

            _Seriell32?.DiscardInBuffer();
            if (Send_Command("RMA", default_return_value, false))
            {
                byte[]? return_value = GetResponse();
                if (return_value != null)
                {
                    string res = Encoding.ASCII.GetString(return_value);
                    res = res.Replace(".", ",");
                    return Convert.ToDouble(res) / 10000;
                }
            }
            return -1;
        }


        private byte[]? GetResponse()
        {
            List<byte> ret = [];
            DateTime dt = DateTime.Now + new TimeSpan(0, 0, 0, 0, 20);

            while (DateTime.Now < dt)
            {
                if (_Seriell32 is not null && _Seriell32.BytesToRead > 0)
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
        private bool Send_Command(string s, byte[]? return_value = null)
        {
            List<byte> outbuf = new(Encoding.ASCII.GetBytes(s)) { 0x0a };
            return Send_Command([.. outbuf], return_value);
        }
        private bool Send_Command(byte[] outbuf, byte[]? return_value = null)
        {
            bool bret = true;
            if (_Seriell32 is not null && _Seriell32.IsOpen)
            {
                _Seriell32.DiscardInBuffer();
                _Seriell32.DiscardOutBuffer();
                _Seriell32.Write([.. outbuf], 0, outbuf.Length);

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
