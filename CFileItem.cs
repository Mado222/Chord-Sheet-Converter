namespace ChordSheetConverter
{
    public class CFileItem
    {
        private string _FileNamePath = "";
        public string fileNamePath
        {
            get { return _FileNamePath; }
            set { _FileNamePath = value; }
        }

        private string _ProcessStatus= "";
        public string processStatus
        {
            get { return _ProcessStatus; }
            set { _ProcessStatus = value; }
        }

        public CFileItem()
        { }

        public CFileItem(string fileNamePath, string processStatus)
        {
            this.fileNamePath = fileNamePath;
            this.processStatus = processStatus;
        }
    }
}
