using WindControlLib;
using static BMTCommunicationLib.CInsightDataEnDecoder;

namespace BMTCommunicationLib
{
    public class CNeuromaster
    {
        public static bool Check4Neuromaster(ISerialPort Seriell32, byte[] SequToSend, byte[] SequToReturn, byte CommandChannelNo)
        {
            bool ret = false;
            bool Failed = false;
            if (Seriell32.IsOpen)
            {
                //Hier abfragen ob ein Device da ist
                try
                {
                    Seriell32.DiscardInBuffer();
                    Seriell32.DiscardOutBuffer();
                    Seriell32.Write(SequToSend, 0, SequToSend.Length);
                    //Thread.Sleep(300);
                }
                catch (Exception)
                {
                    //OnLogError(ee.Message);
                    //log.Error(ee.Message, ee);
                    Failed = true;
                }
                if (!Failed)
                {
                    //int DataToReceive = 4 + AliveSequToReturn.Length;
                    int ptr = 0;
                    int DataToReceive = 4;
                    byte[] buffer = new byte[DataToReceive];
                    DateTime Timeout = DateTime.Now + new TimeSpan(0, 0, 1);    //1s Timeout

                    while (DateTime.Now < Timeout)
                    //while (true)
                    {
                        int ReadRes = Seriell32.Read(ref buffer, ptr, DataToReceive, 100);
                        if (ReadRes == DataToReceive)
                        {
                            CDataIn DI = new();
                            if (Parse4Byte(buffer, ref DI))
                            {
                                if (DI.HW_cn == CommandChannelNo)
                                {
                                    //DI.
                                    if (DI.Value == SequToReturn.Length)
                                    {
                                        //Über Kommandokanalkanal kommt die passende Anzahl von bytes
                                        //Bytes hereinholen
                                        byte[] buffer2 = new byte[SequToReturn.Length];
                                        ReadRes = Seriell32.Read(ref buffer2, 0, SequToReturn.Length, 100);

                                        if (ReadRes == SequToReturn.Length)
                                        {
                                            //Bytes kontrollieren
                                            bool ValCorrect = true;
                                            for (int i = 0; i < SequToReturn.Length; i++)
                                            {
                                                if (buffer2[i] != SequToReturn[i]) ValCorrect = false;
                                            }
                                            if (ValCorrect)
                                            {
                                                //Gerät da
                                                ret = true;
                                                break;
                                            }
                                            //Ir
                                        } //if (ReadRes == AliveSequToReturn.Length)
                                    }//if (DI.Value == AliveSequToReturn.Length)
                                } //if (DI.Value == AliveSequToReturn.Length)
                            } //if (CDecodeBytes.Decode4Byte(bh2, bh1, bh0, bl, ref DI))
                        } //if (ReadRes == DataToReceive)
                        if (!ret)
                        {
                            if (ReadRes > 0)
                            {
                                //Nichts verwertbares hereingekommen, weiterschieben 
                                ptr = 3;
                                DataToReceive = 1;
                                for (int i = 0; i < buffer.Length - 1; i++)
                                    buffer[i] = buffer[i + 1];
                            } //if (ReadRes > 0)
                        } //if (!ret)
                    }   //while (DateTime.Now < Timeout)
                } //if (!Failed)
            }   //if (Seriell32.IsOpen)
            return ret;
        }
    }
}
