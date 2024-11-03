/*
;*************** WICHTIGER URHEBERRECHTSHINWEIS ***********************
Diese Software wurde von Manfred BIJAK geschrieben.
Die Software darf ohne Einverständnis des Autors nicht weitergegeben werden
oder Dritten sonstwie zugänglich gemacht werden.
Der Einsatz der Software in modifiziert wie auch in unverändeter Form
fuer Andwendungen ausser der genhemigten ist nur mit schriftlichem 
Einverstaendnis des Autors zulaessig.
;*************** WICHTIGER URHEBERRECHTSHINWEIS ***********************
*/

namespace WindControlLib
{

    /// <summary>
    /// Konfigurationsklasse für CMinMaxSuche
    /// </summary>
    /// <remarks>
    /// </remarks>
    public class CConfigMinMaxSuche
    {
        private int _MinAR = 4;					//Minimum Atem Rate [bpm]
        /// <summary>
        /// Minimum (Pulse, Atem, )  Rate [bpm]
        /// </summary>
        public int MinAR
        {
            get { return _MinAR; }
            set { _MinAR = value; UpdateVals(); }
        }
        private int _MaxAR = 40;					//Minimum Atem Rate [bpm]
        /// <summary>
        /// Maximum (Pulse, Atem, )  Rate [bpm]
        /// </summary>
        public int MaxAR
        {
            get { return _MaxAR; }
            set { _MaxAR = value; UpdateVals(); }
        }
        private TimeSpan _MaxNeg1;
        /// <summary>
        /// [ms] max Dauer Phase1
        /// </summary>
        public TimeSpan MaxNeg1
        {
            get { return _MaxNeg1; }
            set { _MaxNeg1 = value; }
        }
        private TimeSpan _MaxNeg2;
        /// <summary>
        /// [ms] max Dauer Phase2
        /// </summary>
        public TimeSpan MaxNeg2
        {
            get { return _MaxNeg2; }
            set { _MaxNeg2 = value; }
        }
        private TimeSpan _MaxPos1;
        /// <summary>
        /// [ms] max Dauer Phase3
        /// </summary>
        public TimeSpan MaxPos1
        {
            get { return _MaxPos1; }
            set { _MaxPos1 = value; }
        }

        /// <summary>
        /// Nach so vielen Samples werden die Schwellwerte um ThrReductionPercentage reduziert
        /// </summary>
        public int ThrNextReductionSamples = 0;
        public double ThrReductionPercentage = 0.7;

        private static void UpdateVals()
        {
            //MaxNeg1 = new TimeSpan(0, 0, 0, 0, (int)60000 / MinAR);	 //[ms] max Dauer Phase1
            //MaxNeg2 = new TimeSpan(0, 0, 0, 0, (int)60000 / MinAR);	 //[ms] max Dauer Phase2
            //MaxPos1 = new TimeSpan(0, 0, 0, 0, (int)60000 / MinAR);	 //[ms] max Dauer Phase3
        }
    }

    /// <summary>
    /// Findet Min und Max von Kurven mit biphasischem Aussehen (Puls, Atem etc)
    /// </summary>
    /// <remarks>
    /// Einstellung der Kurvenparameter über CConfigMinMaxSuche
    /// Kurvenwerte liegen im Bereich +-1
    /// </remarks>
    public class CMinMaxSuche
    {
        //Phasen
        const int noPhase = 0;
        const int neg1 = 1;		//Spannung hat RThrNeg1 unterschritten
        const int neg2 = 2;		//Spannung hat RThrNeg2 überschritten
        const int pos1 = 3;	    //Spannung hat RThrPos1 überschritten

        private readonly CDataIn AMax = new();


        private CDataIn AMaxLast = new();
        public CDataIn MaxLast
        {
            get { return AMaxLast; }
            set { AMaxLast = value; }
        }

        private CDataIn AMinLast = new();
        public CDataIn MinLast
        {
            get { return AMinLast; }
            set { AMinLast = value; }
        }

        private readonly CDataIn AMin = new();

        private readonly CDataIn Begin_neg1 = new();	//Beginn der 1. Phase
        private readonly CDataIn Begin_neg2 = new();	//Beginn der 2. Phase
        private readonly CDataIn Begin_pos1 = new();

        private byte RPhase;

        //Thresholds mit Hysterese
        private int AThrNeg1;
        private int AThrNeg2;
        private int AThrPos1;
        private int AThrPos2;

        private double _VSS = 0;
        /// <summary>
        /// Peak-Peak Value
        /// </summary>
        public double VSS
        {
            get { return _VSS; }
        }

        private double _Rate = 0;
        /// <summary>
        /// Events per Minute
        /// </summary>
        public double Rate
        {
            get { return _Rate; }
        }

        //Autom. Verringerung der Schwelle
        //Wenn ThrNextReductionSamples == CountThrReductionSamples wird AThrNeg1 = AThrNeg1* ThrReductionPercentage berechnet
        private int CountThrReductionSamples = 0;

        private CConfigMinMaxSuche _ConfigMinMaxSuche = new();
        public CConfigMinMaxSuche ConfigMinMaxSuche
        {
            get { return _ConfigMinMaxSuche; }
            set { _ConfigMinMaxSuche = value; }
        }

        public CMinMaxSuche(CConfigMinMaxSuche ConfigMinMaxSuche)
        {
            this.ConfigMinMaxSuche = ConfigMinMaxSuche;
        }

        public bool ProcessData(CDataIn DataIn)
        {
            bool ret = false;
            /*
            if (DataIn.HWChannelNumber != 10)
            {
                RPhase = noPhase;
            }*/
            switch (RPhase)
            {
                case noPhase:
                    {
                        CountThrReductionSamples++;
                        if (DataIn.Value < AThrNeg1)
                        {
                            //Minimum Suche beginnt
                            AMin.Copy(DataIn);
                            RPhase = neg1;
                            Begin_neg1.Copy(DataIn);
                        }
                        if (CountThrReductionSamples >= _ConfigMinMaxSuche.ThrNextReductionSamples)
                        {
                            CountThrReductionSamples = 0;
                            AThrNeg1 = (int)(AThrNeg1 * _ConfigMinMaxSuche.ThrReductionPercentage);
                            AThrNeg2 = (int)(AThrNeg2 * _ConfigMinMaxSuche.ThrReductionPercentage);
                            AThrPos1 = (int)(AThrPos1 * _ConfigMinMaxSuche.ThrReductionPercentage);
                            AThrPos2 = (int)(AThrPos2 * _ConfigMinMaxSuche.ThrReductionPercentage);
                        }
                        break;
                    }
                case neg1:
                    {
                        //Minimum suchen
                        if (DataIn.Value < AMin.Value)
                        {
                            //Neuer neg Spitzenwert
                            AMin.Copy(DataIn);
                        }
                        if (DataIn.Value > AThrNeg2)
                        {
                            RPhase = neg2;
                            Begin_neg2.Copy(DataIn);
                        }
                        //Abbruchkriterium
                        if ((DataIn.TS_Since_LastSync - Begin_neg1.TS_Since_LastSync) > _ConfigMinMaxSuche.MaxNeg1)
                        {
                            StopRZackensuche();
                        }
                        break;
                    }
                case neg2:
                    {
                        //Warten bis DataIn.Value RThrPos1 überschreitet
                        if (DataIn.Value > AThrPos1)
                        {
                            //Maximum Suche beginnt
                            RPhase = pos1;
                            AMax.Copy(DataIn);
                            Begin_pos1.Copy(DataIn);
                        }
                        //Abbruchkriterium
                        if ((DataIn.TS_Since_LastSync - Begin_neg2.TS_Since_LastSync) > _ConfigMinMaxSuche.MaxNeg2)
                        {
                            StopRZackensuche();
                        }
                        break;
                    }
                case pos1:
                    {
                        //Maximum suchen
                        if (DataIn.Value > AMax.Value)
                        {
                            //Neuer pos Spitzenwert
                            AMax.Copy(DataIn);
                        }
                        if (DataIn.Value < AThrPos2)
                        {
                            //Rzackensuche fertig, Herzfrequenz berechnen
                            if (AMaxLast.Value != 0)	//Damit nicht durchläuft wenn vorher ein ungültiger Puls war
                            {
                                //Neue Schwellwerte berechnen
                                AThrPos1 = AMaxLast.Value / 2;
                                AThrPos2 = AMaxLast.Value / 4;
                                AThrNeg1 = AMinLast.Value / 2;
                                AThrNeg2 = AMinLast.Value / 4;

                                //Ausgangswerte berechnen
                                double dicmax = (AMax.TS_Since_LastSync - AMaxLast.TS_Since_LastSync).TotalMilliseconds;
                                double dicmin = (AMin.TS_Since_LastSync - AMinLast.TS_Since_LastSync).TotalMilliseconds;
                                double dic = (dicmax + dicmin) / 2; //MW aus Abstand der Max und der Min
                                if (dic != 0) _Rate = dic / 60000;      //gleich auch 1/x
                                _VSS = AMax.Value - AMin.Value;
                                ret = true;
                            }
                            AMaxLast.Copy(AMax);
                            AMinLast.Copy(AMin);
                            AMax.Value = 0;
                            AMin.Value = 0;
                            RPhase = noPhase;
                            CountThrReductionSamples = 0;
                        }
                        //Abbruchkriterium
                        if ((DataIn.TS_Since_LastSync - Begin_pos1.TS_Since_LastSync) > _ConfigMinMaxSuche.MaxPos1)
                        {
                            StopRZackensuche();
                        }
                        break;
                    }
            }//switch
            return ret;
        }
        private void StopRZackensuche()
        {
            RPhase = noPhase;
            AMaxLast.Value = 0;
            CountThrReductionSamples = 0;
        }
    }

    /// <summary>
    /// Lineare Interpolation entsprechen 2 x/y Wertepaaren
    /// </summary>   
    public class CLinearInterpolation
    {
        private double _k = 0;
        public double k
        {
            get { return _k; }
        }

        private double _d = 0;
        public double d
        {
            get { return _d; }
        }

        public CLinearInterpolation(double x1, double y1, double x2, double y2)
        {
            SetParameters(x1, y1, x2, y2);
        }

        public void SetParameters(double x1, double y1, double x2, double y2)
        {
            _k = (y1 - y2) / (x1 - x2);
            _d = y1 - (_k * x1);
        }

        public double GetX(double y)
        {
            return (y - d) / k;
        }

        public double GetY(double x)
        {
            return k * x + d;
        }

    }

}