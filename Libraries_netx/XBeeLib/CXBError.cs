using System;
using System.Reflection;
using System.Resources;

namespace XBeeLib
{
    /// <summary>
    /// Error class
    /// </summary>
    /// <remarks>Error class for errors occured in the XBeeSeries1 class</remarks>
    public class CXBError
    {
        /*
        private static string[] ErrorStrings = {"Serial Port Read Timeout", 
                                    "Checksum not correct",
                                    "No OK (AT-Mode) received",
                                    "No corresponding response received",
                                    "Can't enter command mode",
                                    "AT Command Response: Error-Status received",
                                    "AT Command Response: Invalid Command",
                                    "AT Command Response: Invalid Parameter",
                                    "Local Device is not in API-Mode",
                                    "AT Command Response: No Response",
                                    "Association Process was not successfully",
                                    "New Node-Identifier-String is not adequate"};
         */

                                //XBSetLastError(4);
                        //_Error.SetError_CommandModeFailed();


        /// <summary>
        /// returns the error message corresponding to the error number
        /// </summary>
        public string LastError_String
        {
            get
            {
                ResourceManager rm = new ResourceManager("XBeeLib.Error", Assembly.GetExecutingAssembly());
                switch (Math.Abs(_Last_ErrorNo))
                {
                    case 0:
                        return rm.GetString("err_NoError");
                    case 1:
                        return rm.GetString("err_NoRemoteDevice");
                    case 2:
                        return rm.GetString("err_PortCannotBeReopened");
                    case 3:
                        return rm.GetString("err_XBeeError");
                    case 4:
                        return rm.GetString("err_MoreThan1XBee");
                    case 5:
                        return rm.GetString("err_SPReadTimeout");
                    case 6:
                        return rm.GetString("err_Checksum");
                    case 7:
                        return rm.GetString("err_noOKAT");
                    case 8:
                        return rm.GetString("err_NoCorrespondingResponse");
                    case 9:
                        return rm.GetString("err_CommandModeFailed");
                    case 10:
                        return rm.GetString("err_ATErrorStatus");
                    case 11:
                        return rm.GetString("err_ATInvalidCommand");
                    case 12:
                        return rm.GetString("err_ATInvalidParam");
                    case 13:
                        return rm.GetString("err_LocalDevNotInApi");
                    case 14:
                        return rm.GetString("err_ATNoResponse");
                    case 15:
                        return rm.GetString("err_NoAssociationResponse");
                    case 16:
                        return rm.GetString("err_NodeIdentifierString");
                }
                return "No Error string available";
            }
        }

        public void SetError_NoError()
        {
            Last_ErrorNo = 0;
        }
        public void SetError_NoRemoteDevice()
        {
            Last_ErrorNo = -1;
        }
        public void SetError_PortCannotBeReopened()
        {
            Last_ErrorNo = -2;
        }
        public void SetError_XBeeError()
        {
            Last_ErrorNo = -3;
        }
        public void SetError_MoreThan1XBee()
        {
            Last_ErrorNo = -4;
        }

        public void SetError_SPReadTimeout()
        {
            Last_ErrorNo = -5;
        }
        public void SetError_Checksum()
        {
            Last_ErrorNo = -6;
        }
        public void SetError_noOKAT()
        {
            Last_ErrorNo = -7;
        }
        public void SetError_NoCorrespondingResponse()
        {
            Last_ErrorNo = -8;
        }
        public void SetError_CommandModeFailed()
        {
            Last_ErrorNo = -9;
        }
        public void SetError_ATErrorStatus()
        {
            Last_ErrorNo = -10;
        }
        public void SetError_ATInvalidCommand()
        {
            Last_ErrorNo = -11;
        }
        public void SetError_ATInvalidParam()
        {
            Last_ErrorNo = -12;
        }
        public void SetError_LocalDevNotInApi()
        {
            Last_ErrorNo = -13;
        }
        public void SetError_ATNoResponse()
        {
            Last_ErrorNo = -14;
        }
        public void SetError_NoAssociationResponse()
        {
            Last_ErrorNo = -15;
        }
        public void SetError_NodeIdentifierString()
        {
            Last_ErrorNo = -16;
        }

        /// <summary>
        /// error number
        /// </summary>
        private int _Last_ErrorNo = 0;
        public int Last_ErrorNo
        {
            get { return _Last_ErrorNo; }
            set
            {
                _Last_ErrorNo = value;
                //logger.Error("Error-No.: " + Last_ErrorNo.ToString() + "/t" + LastError_String);
            }
        }
    }
}
