using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static FeedbackDataLib.CEEG_Spectrum;

namespace FeedbackDataLib
{
    public class CEEGSWChannels
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
        public readonly CFrequencyRange_EEG[] _EEG_FFT_Channels =
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
        private readonly string[] _EEG_Calc_Channels =
        [
            "Ratio_Theta_LowBeta",
            "Ratio_Theta_Beta",
            "Ratio_Theta_LowBeta_Beta"
        ];

        private string[] get_Channel_Shortnames ()
        {
            List<string> ret = [];
            for (int i=0; i<_EEG_FFT_Channels.Length; i++)
            {
                ret.Add(_EEG_FFT_Channels[i].Name);
            }
            for (int i = 0; i < _EEG_Calc_Channels.Length; i++)
            {
                ret.Add(_EEG_Calc_Channels[i]);
            }
            return ret.ToArray();
        }

        public CEEGSWChannels()
        {
            EEG_SWChannels = [];
            update_idx(0);
        }

        public void update_idx(int offset)
        {
            EEG_SWChannels.Clear();
            string[] sn = get_Channel_Shortnames();
            for (int i = 0; i < sn.Length; i++)
            {
                EEG_SWChannels.Add((i + offset, sn[i], sn[i] + " [Veff]"));
            }

            // Populate the dictionary with short names and their corresponding indices
            foreach (var data in EEG_SWChannels)
            {
                shortNameToindex[data.shortName] = data.index;
                longNameToindex[data.longName] = data.index;
            }
        }

        public int get_idx_from_shortName(string shortName)
        {
            shortNameToindex.TryGetValue(shortName, out int ret);
            return ret;
        }

        public int get_idx_from_longName(string longName)
        {
            longNameToindex.TryGetValue(longName, out int ret);
            return ret;
        }
        public int get_idx_from_SWChannelName(string SWChannelName)
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


        public string[] get_longNamesArray()
        {
            return EEG_SWChannels.Select(item => item.longName).ToArray();
        }
    }
}
