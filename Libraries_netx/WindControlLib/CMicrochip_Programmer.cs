using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindControlLib
{
    public class CMicrochip_Programmer
    {
        public string Name { get; set; }
        public string SerialNo { get; set; }

        public CMicrochip_Programmer(string Name, string SerialNo)
        {
            this.Name = Name;
            this.SerialNo = SerialNo;
        }

        public string FullInfo
        {
            get
            {
                return (Name + " / " + SerialNo);
            }
        }

        public string CMDLine_string
        {
            get
            {
                //(RICEP, ICE4P, ICD4P, ICD3P, PK4P, PK3P, PK5P, ICD5P, PM3)
                string cmparam = "-TP";
                if (Name == "ICD 3")
                    cmparam += "ICD3";
                else if (Name == "ICD 4")
                    cmparam += "ICD4";
                else if (Name == "PICkit 4")
                    //MPLAB PICkit 4   –  PK4
                    cmparam += "PK4";
                else
                    cmparam += "-NotSet";
                return cmparam;
            }
        }
    }


}
