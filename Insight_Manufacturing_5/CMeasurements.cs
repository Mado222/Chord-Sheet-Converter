using System.Collections.Generic;
using System.Reflection;
using Insight_Manufacturing5_net8.tests_measurements;


namespace Insight_Manufacturing5_net8
{
    public class CMeasurements
    {
        public List<CMeasurementItem> Measurements_Items;

        public CMeasurements()
        {
            //Hier stehen alle verfügbaren Module zum flashen, messen usw.
            Measurements_Items = new List<CMeasurementItem>();
        }

        public bool is_Save_to_DB_overwritten(System.Type class_to_check)
        {
            var i = class_to_check.GetMember("Save_to_DB",
                   BindingFlags.NonPublic
                 | BindingFlags.Instance
                 | BindingFlags.DeclaredOnly).Length == 0;

            return i;
        }
    }

    public class CMeasurementItem
    {
        public CBase_tests_measurements Measurement_Object { get; set; } = null;
        public CBase_tests_measurements.enModuleTestResult ModuleTestResult => Measurement_Object.ModuleTestResult;
        public bool Measurement_Active { get; set; } = false;
        public bool Measurement_SavetoDB { get; set; } = true;
        public int Measurement_idx { get; set; }
        public bool Measurement_done { get; set; } = false;
        public string Measurement_Duration { get; set; }
        //public string Measurement_Name { get; set; }

        
        private string _Measurement_Name = "";
        public string Measurement_Name
        {
            get
            {
                string ret = _Measurement_Name;
                if (!ret.Contains("hex"))
                {
                    if (Measurement_Object.Job_Message != "")
                        ret += ": " + Measurement_Object.Job_Message;
                }
                return ret;
            }
            set { _Measurement_Name = value; }
        }


        public CMeasurementItem()
        {

        }

        public void Reset()
        {
            if (Measurement_Object != null)
                Measurement_Object.Reset();
            Measurement_done = false;
        }

        public CMeasurementItem(int Measurement_idx,
            string Measurement_Name,
            bool Measurement_Active,
            bool Measurement_SavetoDB,
            CBase_tests_measurements Measurement_Object = null)
        {
            if (Measurement_Object.Default_Active == null)
                this.Measurement_Active = Measurement_Active;
            else
                this.Measurement_Active = (bool) Measurement_Object.Default_Active;


            this.Measurement_idx = Measurement_idx;
            this.Measurement_Name = Measurement_Name;
            this.Measurement_SavetoDB = Measurement_SavetoDB;
            this.Measurement_Object = Measurement_Object;
        }
    }
}
