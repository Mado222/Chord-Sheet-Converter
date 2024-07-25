
namespace FeedbackDataLib.Modules.CADS1294x
{
    public class CADS1294x_Gain
    {
        private List<Gain> gainOrder =
            [
                Gain.PGA_GAIN_6,
                Gain.PGA_GAIN_1,
                Gain.PGA_GAIN_2,
                Gain.PGA_GAIN_3,
                Gain.PGA_GAIN_4,
                Gain.PGA_GAIN_8,
                Gain.PGA_GAIN_12
            ];

        // Get the bitmask from an amplification value
        public Gain GetBitMaskFromAmplification(int amplification)
        {
            return gainOrder.FirstOrDefault(g => GetAmplification(g) == amplification, Gain.PGA_GAIN_6); // Default to PGA_GAIN_6 if not found
        }

        // Get the amplification value from a bitmask
        public int GetAmplification(Gain gain)
        {
            return gain switch
            {
                Gain.PGA_GAIN_1 => 1,
                Gain.PGA_GAIN_2 => 2,
                Gain.PGA_GAIN_3 => 3,
                Gain.PGA_GAIN_4 => 4,
                Gain.PGA_GAIN_6 => 6,
                Gain.PGA_GAIN_8 => 8,
                Gain.PGA_GAIN_12 => 12,
                _ => throw new ArgumentException("Invalid gain value"),
            };
        }

        // Increase the gain by one step
        public Gain IncreaseGain(Gain currentGain)
        {
            int currentIndex = gainOrder.IndexOf(currentGain);
            if (currentIndex == -1)
                throw new ArgumentException("Current gain is not recognized");

            int nextIndex = currentIndex + 1;
            if (nextIndex >= gainOrder.Count)
                return currentGain; // Return the same gain if already at maximum

            return gainOrder[nextIndex];
        }

        // Decrease the gain by one step
        public Gain DecreaseGain(Gain currentGain)
        {
            int currentIndex = gainOrder.IndexOf(currentGain);
            if (currentIndex == -1)
                throw new ArgumentException("Current gain is not recognized");

            int prevIndex = currentIndex - 1;
            if (prevIndex < 0)
                return currentGain; // Return the same gain if already at minimum

            return gainOrder[prevIndex];
        }


        // Enum to define PGA gain settings
        public enum Gain : byte
        {
            PGA_GAIN_6 = 0,
            PGA_GAIN_1 = 1,
            PGA_GAIN_2 = 2,
            PGA_GAIN_3 = 3,
            PGA_GAIN_4 = 4,
            PGA_GAIN_8 = 5,
            PGA_GAIN_12 = 6
        }
    }
}
