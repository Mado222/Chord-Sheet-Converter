namespace WindControlLib
{
    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CAverager
    {
        double val;
        private int _count;
        public int Count
        {
            get { return _count; }
        }


        public CAverager()
        {
            Clear();
        }

        public void Clear()
        {
            val = 0;
            _count = 0;
        }

        public void Add(double value)
        {
            val += value;
            _count++;
        }

        public void Subtract(double value)
        {
            val -= value;
            _count--;
        }

        public double GetAverage()
        {
            return val / _count;
        }
    }

    [Serializable()]    //Set this attribute to all the classes that want to serialize
    public class CIntegerMovingAverager
    {
        private long sumofValues;
        private long sumofValuesTemp;
        private int countValues;
        private readonly CRingpuffer rp;
        private readonly int buffer_size;

        public CIntegerMovingAverager(int size)
        {
            rp = new CRingpuffer(size);
            buffer_size = size;
            sumofValues = 0;
            countValues = size;
            for (int i = 0; i < size; i++) rp.Push(0);
            sumofValuesTemp = 0;
            rp.IgnoreOverflowDuringPush = true;
        }

        public int Push(int Value)
        {
            sumofValues += Value;
            sumofValuesTemp += Value;
            sumofValues -= (int)rp.NextLostValue();
            rp.Push(Value);
            countValues--;
            if (countValues == 0)
            {
                //SumofValues immer wieder aktualisieren damit Rundungsfehler nicht kumulieren
                sumofValues = sumofValuesTemp;
                sumofValuesTemp = 0;
                countValues = buffer_size;
            }
            _Average = (int)(sumofValues / (double)buffer_size);
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
        private readonly CRingpuffer RP;
        private readonly int buffer_size;


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
            SumofValues -= (double)RP.NextLostValue();
            RP.Push(Value);
            CountValues--;
            if (CountValues == 0)
            {
                //SumofValues immer wieder aktualisieren damit Rundungsfehler nicht kumulieren
                SumofValues = SumofValuesTemp;
                SumofValuesTemp = 0;
                CountValues = buffer_size;
            }
            _Average = SumofValues / buffer_size;
            return _Average;
        }

        private double _Average;
        public double Average
        {
            get { return _Average; }
        }

    }


}