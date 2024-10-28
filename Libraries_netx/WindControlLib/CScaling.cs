using System;
using System.Collections.Generic;

namespace WindControlLib
{
    /// <summary>
    /// Used to get proper values for axes scaling
    /// and implements linear interpolation of values to a specified range
    /// </summary>
	public class CScaling
	{

        /// <summary>
        /// Used Intervalls on Scale
        /// </summary>
        private readonly double[] pref_deltaTiks = { 0.1, 0.2, 0.25, 0.3, 0.5, 1, 2, 2.5, 3, 5 };
        private readonly int cminTiks = 3;

        private CLinearInterpolation LinInterpol;
        
        private double  _ValMax;
        /// <summary>
        /// Max Value of the scale
        /// </summary>
        /// <remarks>
        /// Will be set to a rounded value in SetParams
        /// </remarks>
        public double  ValMax
        {
            get { return _ValMax; }
        }

        private double _ValMin;
        /// <summary>
        /// Min Value of the scale
        /// </summary>
        /// <remarks>
        /// Will be set to a rounded value in SetParams
        /// </remarks>
        public double ValMin
        {
            get { return _ValMin; }
        }

        private double _deltaTik;
        /// <summary>
        /// Value between Tiks, calculated in SetParams
        /// </summary>
        public double deltaTik
        {
            get { return _deltaTik; }
        }

        /// <summary>
        /// Absolute value between ValMax and ValMin
        /// </summary>
        private double _ValMax_Abs;


        private int _Tiks;
        /// <summary>
        /// Used number of Tiks bigger =  cminTiks, smaller =  maxTiks in SetParams())
        /// </summary>
        public int Tiks
        {
            get { return _Tiks; }
        }


        private int _Skal1;
        /// <summary>
        /// Scaling Value related to Valmax
        /// </summary>
        public int SkalMax
        {
            get { return _Skal1; }
        }


        private int _Skal2;
        /// <summary>
        /// Scaling Value related to Valmin
        /// </summary>
        public int SkalMin
        {
            get { return _Skal2; }
        }


        int exp = 0;                  //multiplikator = 10^exp
        double multiplikator = 1;  //Value x multiplikator ... puts it in the range that meets prefix
        string prefix="";   //m, µ ... 
        
        /// <summary>
        /// Calculate rounded Values
        /// </summary>
        private void CalcVals()
        {
            //Max Achsenabschnitt suchen
            double max_range = Math.Abs(_ValMin);
            if (_ValMax > max_range)
                max_range = _ValMax;

            if (max_range != 0)
            {
                if (max_range >= 1e9)
                {
                    //Giga
                    exp = -9;
                    prefix = "G";
                }
                else if (max_range >= 1e6)
                {
                    //Mega
                    exp = -6;
                    prefix = "M";
                }
                else if (max_range >= 1e3)
                {
                    //Kilo
                    exp = -3;
                    prefix = "k";
                }
                else if (max_range <= 1e-8)
                {
                    //nano
                    exp = 9;
                    prefix = "n";
                }
                else if (max_range <= 1e-5)
                {
                    //micro
                    exp = 6;
                    prefix = "µ";
                }
                else if (max_range <= 1e-2)
                {
                    //milli
                    exp = 3;
                    prefix = "m";
                }

                multiplikator = Math.Pow(10, exp);
                _ValMax_Abs = max_range;

                double norm_ValMax = _ValMax;
                double norm_ValMin = _ValMin;

                //Wert auf Bereich 1..10 reduzieren
                int exp2 = 0;
                if (double.IsNegativeInfinity(max_range)) max_range = -1;
                if (double.IsPositiveInfinity(max_range)) max_range = 1;
                if (max_range > 1)
                {
                    while (max_range > 10)
                    {
                        max_range /= 10;
                        norm_ValMax /= 10;
                        norm_ValMin /= 10;
                        exp2++;
                    }
                }
                else
                {
                    while (max_range <= 1)
                    {
                        max_range *= 10;
                        norm_ValMax *= 10;
                        norm_ValMin *= 10;
                        exp2--;
                    }
                }

                //Jetzt schauen, welche Unterteilung am besten passt
                double norm_range = norm_ValMax - norm_ValMin;


                if (!double.IsNaN(norm_range))
                {
                    double[] _pref_deltaTiks = new double[pref_deltaTiks.Length];
                    Array.Copy(pref_deltaTiks, _pref_deltaTiks, pref_deltaTiks.Length);

                    List<double> dif = new List<double>();
                    List<int> list_idx_pref_deltaTiks = new List<int>();
                    List<int> list_no_Tiks = new List<int>();

                    while (dif.Count < 1)
                    {
                        for (int i = 0; i < _pref_deltaTiks.Length; i++)
                        {
                            int j;
                            for (j = cminTiks; j <= _Tiks; j++)
                            {
                                double d = (_pref_deltaTiks[i] * j);
                                if (d >= norm_range)
                                {
                                    dif.Add(d - norm_ValMax);
                                    list_idx_pref_deltaTiks.Add(i);
                                    list_no_Tiks.Add(j);
                                    break;
                                }
                            }
                        }

                        if (dif.Count < 1)
                        {
                            for (int i = 0; i < _pref_deltaTiks.Length; i++)
                            {
                                _pref_deltaTiks[i] *= 10;
                            }
                        }
                    }

                    //Aussuchen, welche Komnbination wir nehemen
                    int idx_min = 0;

                    if (dif.Count > 0)
                    {
                        double min_in_arr = dif[0];

                        for (int i = 1; i < dif.Count; i++)
                        {
                            if (dif[i] < min_in_arr)
                            {
                                idx_min = i;
                                min_in_arr = dif[i];
                            }
                        }
                    }
                    else
                    {
                        //???????
                        //ToDo: Was machma jetzt
                        //Darf eiegntlich nicht passieren
                    }

                    _deltaTik = _pref_deltaTiks[list_idx_pref_deltaTiks[idx_min]] * Math.Pow(10, exp2);
                    _Tiks = list_no_Tiks[idx_min];

                    double dd = _ValMax * 0.999;  //Rundungsfehlerkompemsation
                    _ValMax = 0;
                    if (dd > 0)
                    {
                        while (_ValMax < dd)
                        {
                            _ValMax += _deltaTik;
                        }
                    }
                    else
                    {
                        while (_ValMax > dd)
                        {
                            _ValMax -= _deltaTik;
                        }
                    }

                    dd = _ValMin * 0.999;
                    _ValMin = 0;
                    if (dd > 0)
                    {
                        while (_ValMin < dd)
                        {
                            _ValMin += _deltaTik;
                        }
                    }
                    else
                    {
                        while (_ValMin > dd)
                        {
                            _ValMin -= _deltaTik;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set Parameters
        /// </summary>
        /// <param name="ValMax">The val max.</param>
        /// <param name="ValMin">The val min.</param>
        /// <param name="TiksMax">The max tiks.</param>
        public void Set_Params (double ValMax, double ValMin, int TiksMax)
        {
            if (TiksMax < cminTiks)
            {
                TiksMax = cminTiks;
            }
            _Tiks = TiksMax;
            _ValMax = ValMax;
            _ValMin = ValMin;
            CalcVals();
        }

        /// <summary>
        /// Gets a formatted string with Prefix
        /// </summary>
        /// <remarks>
        /// Used to put proper string to Tiks
        /// </remarks>
        /// <param name="Value">Value</param>
        /// <returns></returns>
        public string GetString_for_Value(double Value)
        {
            //Rundungsfehler bei 0 ausgleichen
            if (Math.Abs(Value) < _ValMax_Abs / 1000) return "0";
            Value *= multiplikator;
            double _ValMax_Abs_norm = multiplikator * _ValMax_Abs;

            string ret;
            if (_ValMax_Abs_norm < 1)
            {
                //2 Nachkommastellen
                ret = string.Format("{0:0.00}", Value);
            }
            else if (_ValMax_Abs_norm < 10)
            {
                //1 Nachkommastelle
                ret = string.Format("{0:0.0}", Value);
            }
            else
            {
                //Keine Nachkommastelle
                ret = string.Format("{0:#}", Value);
            }

            ret += prefix;
            return ret;
        }

        /// <summary>
        /// Sets scaling Prams
        /// </summary>
        /// <param name="Skal1">Scaling Value related to Valmax</param>
        /// <param name="Skal2">Scaling Value related to Valmin</param>
        public void Set_Skal_Params(int Skal1, int Skal2)
        {
            _Skal2 = Skal2;
            _Skal1 = Skal1;
            if (Skal2!=Skal1)
            {
            LinInterpol = new CLinearInterpolation(
                Skal2,    //x1
                ValMin,   //y1 
                Skal1,    //x2
                ValMax);  //y2
            }
        }

        /// <summary>
        /// Gets the scaled value (pixels)
        /// </summary>
        /// <param name="Value">value</param>
        /// <returns></returns>
        public int Get_Skal_Value(double Value)
        {
            int ret = 0;
            if (LinInterpol != null)
            {
                ret = (int)LinInterpol.GetX(Value);
            }
            return ret;
        }
    }
 }
