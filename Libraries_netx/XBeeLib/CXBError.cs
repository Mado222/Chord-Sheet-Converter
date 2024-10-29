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
                ResourceManager rm = new("XBeeLib.Error", Assembly.GetExecutingAssembly());
                string? errorString = null;

                switch (Math.Abs(_Last_ErrorNo))
                {
                    case 0:
                        errorString = rm.GetString("err_NoError");
                        break;
                    case 1:
                        errorString = rm.GetString("err_NoRemoteDevice");
                        break;
                    case 2:
                        errorString = rm.GetString("err_PortCannotBeReopened");
                        break;
                    case 3:
                        errorString = rm.GetString("err_XBeeError");
                        break;
                    case 4:
                        errorString = rm.GetString("err_MoreThan1XBee");
                        break;
                    case 5:
                        errorString = rm.GetString("err_SPReadTimeout");
                        break;
                    case 6:
                        errorString = rm.GetString("err_Checksum");
                        break;
                    case 7:
                        errorString = rm.GetString("err_noOKAT");
                        break;
                    case 8:
                        errorString = rm.GetString("err_NoCorrespondingResponse");
                        break;
                    case 9:
                        errorString = rm.GetString("err_CommandModeFailed");
                        break;
                    case 10:
                        errorString = rm.GetString("err_ATErrorStatus");
                        break;
                    case 11:
                        errorString = rm.GetString("err_ATInvalidCommand");
                        break;
                    case 12:
                        errorString = rm.GetString("err_ATInvalidParam");
                        break;
                    case 13:
                        errorString = rm.GetString("err_LocalDevNotInApi");
                        break;
                    case 14:
                        errorString = rm.GetString("err_ATNoResponse");
                        break;
                    case 15:
                        errorString = rm.GetString("err_NoAssociationResponse");
                        break;
                    case 16:
                        errorString = rm.GetString("err_NodeIdentifierString");
                        break;
                }

                return errorString ?? "No error string found";
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
