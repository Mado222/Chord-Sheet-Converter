namespace WindControlLib
{
    /// <summary>
    /// Zur Verwaltung von PID und VID
    /// </summary>
    public class CVidPid
    {
        public CVidPid() { }

        public CVidPid(string vID, string pID)
        {
            VID = vID;
            PID = pID;
        }

        public string VID { get; set; } = string.Empty;
        public string PID { get; set; } = string.Empty;

        private static readonly string[] separator = { "&", "_" };

        // "VID_0403&PID_6010"
        public virtual string VID_PID
        {
            get => $"VID_{VID}&PID_{PID}";
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    VID = string.Empty;
                    PID = string.Empty;
                    return;
                }

                string[] parts = value.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 4)
                {
                    if (parts[0].Equals("VID", StringComparison.OrdinalIgnoreCase))
                        VID = parts[1];
                    if (parts[2].Equals("PID", StringComparison.OrdinalIgnoreCase))
                        PID = parts[3];
                }
                else
                {
                    VID = string.Empty;
                    PID = string.Empty;
                }
            }
        }
    }
}
