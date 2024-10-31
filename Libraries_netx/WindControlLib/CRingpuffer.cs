/*
;*************** WICHTIGER URHEBERRECHTSHINWEIS ***********************
Diese Software wurde von Manfred BIJAK geschrieben.
Die Software darf ohne Einverständnis des Autors nicht weitergegeben werden
oder Dritten sonstwie zugänglich gemacht werden.
Der Einsatz der Software in modifiziert wie auch in unverändeter Form
fuer Andwendungen ausser der genhemigten ist nur mit schriftlichem 
Einverstaendnis des Autors zulaessig.
;*************** WICHTIGER URHEBERRECHTSHINWEIS ***********************
 * 
 * 27.1.2011
 * Ringpuffer Thread safe gemacht
*/

using System;
using System.Collections.Generic;
using System.Drawing;

namespace WindControlLib
{
    /// <summary>
    /// 
    /// </summary>
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CRingpuffer
    {
        /// <summary>
        /// Puffer
        /// </summary>
		protected object[] buf;
        /// <summary>
        /// Points to next Reading position
        /// </summary>
		protected int ReadPtr = 0;
        /// <summary>
        /// Points to next writing position
        /// </summary>
		protected int WritePtr = 0;
        /// <summary>
        /// Number of vurrently stored objects
        /// </summary>
		public int StoredObjects = 0;
        /// <summary>
        /// Buffer size
        /// </summary>
		protected int intsize;
        /// <summary>
        /// Data not read can be overwritten
        /// </summary>
		public bool IgnoreOverflowDuringPush = false;
        private readonly object CRingpufferLock = new();
        /// <summary>
        /// Buffer was filled one time
        /// Indicates, that the buffer is full with valid values
        /// </summary>
        private bool BufFilled = false;

        public CRingpuffer(int size)
        {
            buf = new object[size];
            intsize = size;
            for (int i = 0; i < size; i++)
            {
                buf[i] = new object();
            }
        }

        public int Length
        {
            get { return buf.Length; }
        }

        public void Clear()
        {
            ReadPtr = 0;
            WritePtr = 0;
            StoredObjects = 0;
            BufFilled = false;
        }

        /// <summary>
        /// Push
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <returns>false: Puffer voll</returns>
        public virtual bool Push(object obj)
        {
            lock (CRingpufferLock)
            {
                bool ret = false;
                if ((StoredObjects < intsize) || IgnoreOverflowDuringPush)
                {
                    buf[WritePtr] = obj;
                    if (StoredObjects < intsize)
                        StoredObjects++;

                    IncrementPointer(ref WritePtr);
                    ret = true;
                }
                else
                {
                    ret = false;
                }
                return ret;
            }
        }

        /// <summary>
        /// Pushes the specified object.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="Pointer">The pointer.</param>
        /// <param name="NumBytetoStore">The number byteto store.</param>
        /// <returns>false: Puffer voll</returns>
        public virtual bool Push(object[] obj, int Pointer, int NumBytetoStore)
        {
            lock (CRingpufferLock)
            {
                bool ret = false;
                if ((intsize - StoredObjects) < NumBytetoStore)
                {
                    for (int i = 0; i < NumBytetoStore; i++)
                    {
                        Push(obj[Pointer + i]);
                    }
                    ret = true;
                }
                return ret;
            }
        }

        /// <summary>
        /// Pop
        /// </summary>
        /// <returns>null wenn Puffer leer</returns>
        public virtual object? Pop()
        {
            lock (CRingpufferLock)
            {
                object? o = null;
                if (StoredObjects > 0)
                {
                    o = buf[ReadPtr];
                    StoredObjects--;
                    IncrementPointer(ref ReadPtr);
                }
                else
                    o = null;
                return o;
            }
        }

        /// <summary>
        /// Liest den ganzen Ringpuffer, beginnend mit dem ältestewn Wert aus 
        /// Funktion ist unabhängig davon, ob und wieviele Werte ausgelesen wurden
        /// Wenn Puffer nicht voll werden nur StoredObjects zurückgegeben
        /// </summary>
        /// <param name="AllData">All data.</param>
		public virtual void PopAll(ref object[] AllData)
        {
            lock (CRingpufferLock)
            {
                List<object> _AllData = [];
                if (BufFilled)
                {
                    int TempReadPtr = WritePtr;
                    for (int i = 0; i < intsize; i++)
                    {
                        _AllData.Add(buf[TempReadPtr]);
                        IncrementPointer(ref TempReadPtr);
                    }
                }
                else
                {
                    for (int i = 0; i < StoredObjects; i++)
                    {
                        _AllData.Add(buf[i]);
                    }
                }
                AllData = [.. _AllData];
            }
        }

        /// <summary>
        /// Liest den ganzen Ringpuffer, beginnend mit dem ältestewn Wert aus 
        /// Funktion ist unabhängig davon, ob und wieviele Werte ausgelesen wurden
        /// Wenn Puffer nicht voll werden nur StoredObjects zurückgegeben
        /// </summary>
        /// <param name="AllData">All data.</param>
        public virtual void PopAll(ref double[] AllData)
        {
            lock (CRingpufferLock)
            {
                List<double> _AllData = [];
                if (BufFilled)
                {
                    int TempReadPtr = WritePtr;
                    for (int i = 0; i < intsize; i++)
                    {
                        _AllData.Add((double)buf[TempReadPtr]);
                        IncrementPointer(ref TempReadPtr);
                    }
                }
                else
                {
                    for (int i = 0; i < StoredObjects; i++)
                    {
                        _AllData.Add((double)buf[i]);
                    }
                }
                AllData = [.. _AllData];
            }
        }

        /// <summary>
        /// Liest den ganzen Ringpuffer, beginnend mit dem ältestewn Wert aus 
        /// Funktion ist unabhängig davon, ob und wieviele Werte ausgelesen wurden
        /// Wenn Puffer nicht voll werden nur StoredObjects zurückgegeben
        /// </summary>
        /// <param name="AllData">All data.</param>
        public virtual void PopAll(ref CDataIn[]? AllData)
        {
            lock (CRingpufferLock)
            {
                List<CDataIn> l = [];
                int TempReadPtr = WritePtr;
                //Wenn Puffer noch nicht voll stehen von 
                // buf[WritePtr] ... puffer Ende falsche Werte
                if (BufFilled)
                {
                    for (int i = 0; i < intsize; i++)
                    {
                        l.Add((CDataIn)buf[TempReadPtr]);
                        IncrementPointer(ref TempReadPtr);
                    }
                }
                else
                {
                    for (int i = 0; i < StoredObjects; i++)
                    {
                        l.Add((CDataIn)buf[i]);
                    }
                }
                AllData = [.. l];
            }
        }

        public virtual CDataIn[]? PopAll()
        {
            CDataIn[]? ret = null;
            PopAll(ref ret);
            return ret;
        }

        /// <summary>
        ///Dieser Wert geht beim nächsten Push verloren - auf diesen wird als nächstes geschrieben
        ///Für Mittelwertsbildung
        /// </summary>
        /// <returns></returns>
        public virtual object NextLostValue()
        {
            return buf[WritePtr];
        }

        /// <summary>
        /// Value that comes with next Pop
        /// </summary>
        /// <returns></returns>
        public virtual object NextPopValue()
        {
            return buf[ReadPtr];
        }


        public void IncrementPointer(ref int ptr)
        {
            ptr++;
            if (ptr == intsize)
            {
                ptr = 0;
                BufFilled = true;
            }
        }

        /*
        private void DecrementPointer(ref int ptr)
        {
            ptr--;
            if (ptr < 0)
            {
                ptr = intsize - 1;
            }
        }*/
    }

    /// <summary>
    /// Ringpuffer for bytes
    /// </summary>
    /// <remarks>class should be thread safe; Buffer intehrity is checked and in case not buffer is reset</remarks>
	public class CByteRingpuffer
    {
        public byte[] buf;
        private int WritePtr;
        public int StoredBytes;
        protected int intsize;
        public bool IgnoreOverflowDuringPush = false;
        private readonly object CByteRingpufferLock = new();


        public int EmptySpace
        {
            get { return buf.Length - StoredBytes; }
        }

        private int _ReadPtr;
        public int ReadPtr
        {
            get { return _ReadPtr; }
        }

        public CByteRingpuffer(int size)
        {
            buf = new byte[size];
            intsize = size;
            for (int i = 0; i < size; i++)
            {
                buf[i] = new byte();
            }
            Clear();
        }

        public void Clear()
        {
            _ReadPtr = 0;
            WritePtr = 0;
            StoredBytes = 0;
        }

        private void CheckBufferIntegrity()
        {
            if (StoredBytes == 0)
                if (WritePtr != ReadPtr)
                {
                    Clear();
                }
        }


        //Gibt den ReadPtr zum lesen in buf zurück, berechnet aus dem aktuellen ReadPtr + Offset
        /*
        public int CalcReadPtr (int Offset)
		{
			int rpt= ReadPtr+Offset;
			if (rpt>=intsize) rpt=rpt-intsize;
			return rpt;
		}*/

        //Return Value = false: Puffer voll
        public virtual bool Push(byte obj)
        {
            lock (CByteRingpufferLock)
            {
                bool ret = false;
                if ((StoredBytes < intsize) || IgnoreOverflowDuringPush)
                {
                    buf[WritePtr] = obj;
                    WritePtr++; StoredBytes++;
                    if (WritePtr == intsize) { WritePtr = 0; }
                    ret = true;
                }
                return ret;
            }
        }

        //Return Value = false: Puffer voll
        public virtual bool Push(byte[] obj, int Pointer, int NumBytetoStore)
        {
            lock (CByteRingpufferLock)
            {
                bool ret = false;
                if ((intsize - StoredBytes) >= NumBytetoStore)
                {
                    for (int i = 0; i < NumBytetoStore; i++)
                    {
                        Push(obj[Pointer + i]);
                    }
                    ret = true;
                }
                else
                {
                    ret = false;
                }
                CheckBufferIntegrity(); //debug
                return ret;
            }
        }

        //Ergebnis =null wenn Puffer leer
        public virtual byte? Pop()
        {
            lock (CByteRingpufferLock)
            {

                byte? o = null;
                if (StoredBytes > 0)
                {
                    o = buf[ReadPtr];
                    _ReadPtr++; StoredBytes--;
                    if (ReadPtr == intsize)
                    { _ReadPtr = 0; }
                }
                else
                    o = null;
                //CheckBufferIntegrity(); //debug
                return o;
            }
        }

        public virtual byte Pop(ref byte[] obj, int offset, int NumBytetoRead)
        {
            byte o = 0;
            if (NumBytetoRead <= StoredBytes)
            {
                for (int i = 0; i < NumBytetoRead; i++)
                {
                    byte? b = Pop();
                    if (b != null)
                    {
                        obj[offset] = (byte)b;
                        o++;
                    }
                }
            }
            return o;
        }

        //liest den ganzen Ringpuffer aus 
        // AllData[0]=Aeltester Wert, AllData[intsize-1]=Juengster Wert
        // Funktion ist unabhängig davon, ob und wieviele Werte ausgelesen worden
        public virtual void PopAll(ref byte[] AllData)
        {
            lock (CByteRingpufferLock)
            {
                AllData = new byte[intsize];
                int TempReadPtr = WritePtr;
                for (int i = 0; i < intsize; i++)
                {
                    AllData[i] = buf[TempReadPtr];
                    TempReadPtr++;
                    if (TempReadPtr == intsize) { TempReadPtr = 0; }
                }
            }
        }
    }


    /// <summary>
    /// Daten Klasse zur Darstellung y1...yx vs. time
    /// und Farbe des Punktes
    /// </summary>
    public class CYvsTimeDataColor(int ysize) : CYvsTimeData(ysize)
    {
        public Color DotColor;

        public virtual void Copy(CYvsTimeDataColor c)
        {
            base.Copy(c);
            DotColor = c.DotColor;
        }
    }

    /// <summary>
    /// Daten Klasse zur Darstellung y1...yx vs. time
    /// </summary>
    public class CYvsTimeData
    {
        public double[] yData;
        public DateTime xData;

        public CYvsTimeData(int ysize)
        {
            yData = new double[ysize];
        }

        public CYvsTimeData(DateTime xData, double yData)
        {
            this.yData = [yData];
            this.xData = xData;
        }

        public virtual void Copy(CYvsTimeData c)
        {
            for (int i = 0; i < yData.Length; i++)
                yData[i] = c.yData[i];
            xData = c.xData;
        }
    }

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////
    /// 
    /// CYvsTimeRingPuffer:
    /// 
    /// Klasse zur Interpolation von y-Zeit Daten
    /// numyData stellt die Anzahl der y-Werte, die zu einem Zeitwert gehören ein
    /// 
    /// ///////////////////////////////////////////////////////////////////////////
    /// </summary>

    public class CYvsTimeRingPuffer : CRingpuffer
    {
        public int numyData = 1;
        public CYvsTimeData? Prev_pop_data; //Vorletzter aus dem Ringpuffer gelesener Datensatz
        public CYvsTimeData? Pop_data;      //Zuletzt aus dem Ringpuffer gelesener Datensatz
        public TimeSpan deltaTimePush;
        public bool Interpol_out_of_range = false;  //true wenn der x-Wert (Zeit) nicht zwischen den beiden Interpolationsstützpunkten liegt


        public CYvsTimeRingPuffer(int RingPufferSize) : base(RingPufferSize)
        {
            InitCYvsTimeRingPuffer();
        }

        protected void InitCYvsTimeRingPuffer()
        {
            Prev_pop_data = new CYvsTimeData(numyData);
            Pop_data = new CYvsTimeData(numyData);
        }

        public override object? Pop()
        {
            //CYvsTimeData pd=new CYvsTimeData(numyData);
            CYvsTimeData? pd = (CYvsTimeData?)base.Pop();
            if (pd is not null && Prev_pop_data is not null && Pop_data is not null)
            {
                Prev_pop_data.Copy(Pop_data);   //Datensätze aktualisiern
                Pop_data.Copy(pd);
            }
            return pd;
        }

        public void Push(double[] yvals, DateTime xval)
        {
            CYvsTimeData xydata = new(numyData);

            for (int i = 0; i < numyData; i++)
            {
                xydata.yData[i] = yvals[i];
            }
            xydata.xData = xval;
            base.Push(xydata);
        }

        public void Push(CYvsTimeData data)
        { base.Push(data); }

        public CYvsTimeData? GetInterpolatedData(DateTime dt)
        {
            CYvsTimeData? ret = null;
            //Daten holen bis Zeitfenster passt
            if (Pop_data is not null && Prev_pop_data is not null)
            {
                while ((dt > Pop_data.xData) && (base.StoredObjects > 0))
                {
                    Pop();
                }

                if (Prev_pop_data.xData.Ticks != Pop_data.xData.Ticks)  //Division durch 0 abfangen
                {
                    double k;
                    double d;
                    ret = new CYvsTimeData(numyData);
                    for (int i = 0; i < numyData; i++)
                    {
                        k = (Prev_pop_data.yData[i] - Pop_data.yData[i]) / (Prev_pop_data.xData.Ticks - Pop_data.xData.Ticks);
                        d = Prev_pop_data.yData[i] - k * Prev_pop_data.xData.Ticks;
                        ret.xData = dt;
                        ret.yData[i] = k * dt.Ticks + d;
                    }
                    //Check range
                    if (dt > Pop_data.xData)
                        Interpol_out_of_range = true;
                    else
                        Interpol_out_of_range = false;
                }
            }
            return ret;
        }
    }
}
