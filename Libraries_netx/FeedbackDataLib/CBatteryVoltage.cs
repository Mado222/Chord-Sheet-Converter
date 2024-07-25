using System.IO;
using System.Xml.Serialization;
using WindControlLib;

namespace FeedbackDataLib
{
    /// <summary>
    /// Holds the characteristics of the Battery in the Neuromaster
    /// </summary>
    public class CBatteryVoltage
    {
        public class BatteryValuePair
        {

            private double _BatteryVoltage_V;
            public double BatteryVoltage_V
            {
                get { return _BatteryVoltage_V; }
                set { _BatteryVoltage_V = value; }
            }


            private double _BatteryPercentage;
            public double BatteryPercentage
            {
                get { return _BatteryPercentage; }
                set { _BatteryPercentage = value; }
            }

            public BatteryValuePair()
            {
                BatteryVoltage_V = 0;
                BatteryPercentage = 0;
            }

            public BatteryValuePair(double BatteryVoltage_V, double BatteryPercentage)
            {
                this.BatteryVoltage_V = BatteryVoltage_V;
                this.BatteryPercentage = BatteryPercentage;
            }
        }

        private BatteryValuePair[] BatteryValuePairs;

        public CBatteryVoltage()
        {
            if (ReadBatteryValues())
            {
            }
            else
            {
                //Load default Values
                BatteryValuePairs = new BatteryValuePair[5];
                BatteryValuePairs[0] = new BatteryValuePair(3, 100);
                BatteryValuePairs[1] = new BatteryValuePair(2.9, 80);
                BatteryValuePairs[2] = new BatteryValuePair(2.8, 60);
                BatteryValuePairs[3] = new BatteryValuePair(2.7, 40);
                BatteryValuePairs[4] = new BatteryValuePair(2.6, 0);
            }
        }

        public double GetPercentage(double BatteryVoltage_V)
        {
            if (BatteryVoltage_V >= BatteryValuePairs[0].BatteryVoltage_V)
            {
                return BatteryValuePairs[0].BatteryPercentage;
            }

            if (BatteryVoltage_V > BatteryValuePairs[BatteryValuePairs.Length - 1].BatteryVoltage_V)
            {
                //Interpolate
                for (int i = 0; i < BatteryValuePairs.Length - 2; i++)
                {
                    if ((BatteryVoltage_V <= BatteryValuePairs[i].BatteryVoltage_V) &&
                        (BatteryVoltage_V > BatteryValuePairs[i + 1].BatteryVoltage_V))
                    {
                        CLinearInterpolation c = new CLinearInterpolation(
                        BatteryValuePairs[i + 1].BatteryVoltage_V, BatteryValuePairs[i + 1].BatteryPercentage,
                        BatteryValuePairs[i].BatteryVoltage_V, BatteryValuePairs[i].BatteryPercentage);

                        return c.GetY(BatteryVoltage_V);
                    }
                }
            }
            return 0;
        }

        public void GetConfigPath(ref string ConfigXMLPath)
        {
            string fullPath = System.IO.Directory.GetCurrentDirectory();
            ConfigXMLPath = fullPath + @"\BatVals.xml";
        }

        public bool ReadBatteryValues()
        {
            bool ret = false;
            TextReader reader = null;

            try
            {
                string ConfigXMLPath = "";
                GetConfigPath(ref ConfigXMLPath);

                FileStream fs = new FileStream(ConfigXMLPath, FileMode.Open);
                reader = new StreamReader(fs);
                XmlSerializer ser = new XmlSerializer(typeof(BatteryValuePair[]));
                BatteryValuePairs = (BatteryValuePair[])ser.Deserialize(reader);
                reader.Close();
                ret = true;
            }
            catch
            {
                ret = false;
            }
            finally
            {
                if (reader != null)
                    reader.Close();
            }
            return ret;
        }

        public bool SaveBatteryValues()
        {
            /* Create a StreamWriter to write with. First create a FileStream
               object, and create the StreamWriter specifying an Encoding to use. */
            try
            {
                string ConfigXMLPath = "";
                GetConfigPath(ref ConfigXMLPath);

                FileStream fs = new FileStream(ConfigXMLPath, FileMode.Create);
                TextWriter writer = new StreamWriter(fs);
                XmlSerializer ser = new XmlSerializer(typeof(BatteryValuePair[]));
                ser.Serialize(writer, BatteryValuePairs);
                writer.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

    }
}
