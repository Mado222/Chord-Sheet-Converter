using static FeedbackDataLib.CEEGSpectrum;

namespace FeedbackDataLib
{
    public class CEEGCalcChannels
    {
        /// <summary>
        /// All info about EEG SW Channels
        /// </summary>
        public List<(int index, string shortName, string longName)> EEG_SWChannels;

        /// <summary>
        /// Dictionary for efficient lookup
        /// </summary>
        public Dictionary<string, int> shortNameToindex = [];
        public Dictionary<string, int> longNameToindex = [];
        public Dictionary<string, int> SWChannelNameToindex = [];

        /// <summary>
        /// The eeg sw channel names
        /// </summary>
        public readonly CEEG_FrequencyRanges[] EEG_FrequencyRanges =
        [
            new("DC",0, 0.9, 1),
            new ("Delta",1,3,1),
            new ("Theta",4,8,1),
            new ("Alpha",8,12,1),
            new ("SMR",12,15,1),
            new ("LowBeta",15,18,1),
            new ("Beta",18,23,1),
            new ("HighBeta",23,30,1),
            new ("Artefacts",51,58,1),
        ];
        private readonly string[] EEGCalcParams =
        [
            "Ratio_Theta_LowBeta",
            "Ratio_Theta_Beta",
            "Ratio_Theta_LowBeta_Beta"
        ];

        private string[] Get_Channel_Shortnames()
        {
            List<string> ret = [];
            for (int i = 0; i < EEG_FrequencyRanges.Length; i++)
            {
                ret.Add(EEG_FrequencyRanges[i].Name);
            }
            for (int i = 0; i < EEGCalcParams.Length; i++)
            {
                ret.Add(EEGCalcParams[i]);
            }
            return [.. ret];
        }

        public CEEGCalcChannels()
        {
            EEG_SWChannels = [];
            Update_idx(0);
        }

        public void Update_idx(int offset)
        {
            EEG_SWChannels.Clear();
            string[] sn = Get_Channel_Shortnames();
            for (int i = 0; i < sn.Length; i++)
            {
                EEG_SWChannels.Add((i + offset, sn[i], sn[i] + " [Veff]"));
            }

            // Populate the dictionary with short names and their corresponding indices
            foreach (var (index, shortName, longName) in EEG_SWChannels)
            {
                shortNameToindex[shortName] = index;
                longNameToindex[longName] = index;
            }
        }

        public int Get_idx_from_shortName(string shortName)
        {
            shortNameToindex.TryGetValue(shortName, out int ret);
            return ret;
        }

        public int Get_idx_from_longName(string longName)
        {
            longNameToindex.TryGetValue(longName, out int ret);
            return ret;
        }
        public int Get_idx_from_SWChannelName(string SWChannelName)
        {
            int ret = -1;
            if (SWChannelNameToindex.TryGetValue(SWChannelName, out int _ret))
            { ret = _ret; }
            else
            {
                //Add value
                foreach (var sn in shortNameToindex)
                {
                    if (SWChannelName.Contains(sn.Key))
                    {
                        SWChannelNameToindex[SWChannelName] = sn.Value;
                        ret = sn.Value;
                        break;
                    }

                }
            }
            return ret;
        }

        public string[] Get_longNamesArray()
        {
            return EEG_SWChannels.Select(item => item.longName).ToArray();
        }
    }
}
