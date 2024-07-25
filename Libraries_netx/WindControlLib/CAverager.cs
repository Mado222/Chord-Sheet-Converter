namespace WindControlLib
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CAverager
    {
        double val;
        private int _count;
        public int count
        {
            get { return _count; }
        }


        public CAverager()
        {
            Clear();
        }

        public void Clear()
        {
            val=0;
            _count = 0;
        }

        public void Add (double value)
        {
            val += value;
            _count ++;
        }

        public void Subtract (double value)
        {
            val -= value;
            _count --;
        }

        public double GetAverage()
        {
            return val / (double) _count;
        }
    }

    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CIntegerMovingAverager
    {
        private long SumofValues;
        private long SumofValuesTemp;
        private int CountValues;
        private CRingpuffer RP;
        private int buffer_size;

        public CIntegerMovingAverager(int size)
        {
            RP = new CRingpuffer(size);
            buffer_size = size;
            SumofValues = 0;
            CountValues = size;
            for (int i = 0; i < size; i++) RP.Push(0);
            SumofValuesTemp = 0;
            RP.IgnoreOverflowDuringPush = true;
        }

        public int Push(int Value)
        {
            SumofValues += Value;
            SumofValuesTemp += Value;
            SumofValues -= (int)RP.NextLostValue();
            RP.Push(Value);
            CountValues--;
            if (CountValues == 0)
            {
                //SumofValues immer wieder aktualisieren damit Rundungsfehler nicht kumulieren
                SumofValues = SumofValuesTemp;
                SumofValuesTemp = 0;
                CountValues = buffer_size;
            }
            _Average = (int)((double)SumofValues / (double)buffer_size);
            return _Average;
        }

        private int _Average;
        public int Average
        {
            get { return _Average; }
        }
    }

    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CDoubleMovingAverager
    {
        private double SumofValues;
        private double SumofValuesTemp;
        private int CountValues;
        private CRingpuffer RP;
        private int buffer_size;


        public CDoubleMovingAverager(int size)
        {
            RP = new CRingpuffer(size);
            buffer_size = size;
            SumofValues = 0;
            CountValues = size;
            for (int i = 0; i < size; i++) RP.Push(0);
            SumofValuesTemp = 0;
            RP.IgnoreOverflowDuringPush = true;
        }

        public double Push(double Value)
        {
            SumofValues += Value;
            SumofValuesTemp += Value;
            SumofValues -= (double) RP.NextLostValue();
            RP.Push(Value);
            CountValues--;
            if (CountValues == 0)
            {
                //SumofValues immer wieder aktualisieren damit Rundungsfehler nicht kumulieren
                SumofValues = SumofValuesTemp;
                SumofValuesTemp = 0;
                CountValues = buffer_size;
            }
            _Average = SumofValues / (double)buffer_size;
            return _Average;
        }

        private double _Average;
        public double Average
        {
            get { return _Average; }
        }

    }


}