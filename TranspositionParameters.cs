using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ChordSheetConverter.CScales;

namespace ChordSheetConverter
{
    public class TranspositionParameters : INotifyPropertyChanged
    {
        private string _sourceKey = "G";
        private ScaleType _sourceScaleType = ScaleType.Major;
        private ScaleMode _sourceMode = ScaleMode.Ionian;
        private string _targetKey = "G";
        private ScaleType _targetScaleType = ScaleType.Major;
        private ScaleMode _targetMode = ScaleMode.Ionian;

        public string SourceKey
        {
            get => _sourceKey;
            set
            {
                if (_sourceKey != value)
                {
                    _sourceKey = value;
                    OnPropertyChanged(nameof(SourceKey));
                }
            }
        }

        public ScaleType SourceScaleType
        {
            get => _sourceScaleType;
            set
            {
                if (_sourceScaleType != value)
                {
                    _sourceScaleType = value;
                    OnPropertyChanged(nameof(SourceScaleType));
                }
            }
        }

        public ScaleMode SourceMode
        {
            get => _sourceMode;
            set
            {
                if (_sourceMode != value)
                {
                    _sourceMode = value;
                    OnPropertyChanged(nameof(SourceMode));
                }
            }
        }

        public string TargetKey
        {
            get => _targetKey;
            set
            {
                if (_targetKey != value)
                {
                    _targetKey = value;
                    OnPropertyChanged(nameof(TargetKey));
                }
            }
        }

        public ScaleType TargetScaleType
        {
            get => _targetScaleType;
            set
            {
                if (_targetScaleType != value)
                {
                    _targetScaleType = value;
                    OnPropertyChanged(nameof(TargetScaleType));
                }
            }
        }

        public ScaleMode TargetMode
        {
            get => _targetMode;
            set
            {
                if (_targetMode != value)
                {
                    _targetMode = value;
                    OnPropertyChanged(nameof(TargetMode));
                }
            }
        }

        public TranspositionParameters() { }

        public TranspositionParameters(string sourceKey, ScaleType sourceScaleType, ScaleMode sourceMode, string targetKey, ScaleType targetScaleType, ScaleMode targetMode = ScaleMode.Ionian)
        {
            SourceKey = sourceKey;
            SourceScaleType = sourceScaleType;
            SourceMode = sourceMode;
            TargetKey = targetKey;
            TargetScaleType = targetScaleType;
            TargetMode = targetMode;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
