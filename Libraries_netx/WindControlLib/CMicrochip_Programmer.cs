namespace WindControlLib
{
    public class CMicrochip_Programmer(string Name, string SerialNo)
    {
        public string Name { get; set; } = Name;
        public string SerialNo { get; set; } = SerialNo;

        public string FullInfo
        {
            get
            {
                return Name + " / " + SerialNo;
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
