namespace ComponentsLib_GUI
{
    public class CStatusText : RichTextBox
    {
        public CStatusText()
        {
            this.ScrollBars = RichTextBoxScrollBars.Vertical;
        }

        /// <summary>
        /// Add Date to the Status String
        /// </summary>
        public bool AddDate { get; set; }

        /// <summary>
        /// Add time to the Status string
        /// </summary>
        public bool AddTime { get; set; }

        /// <summary>
        /// Clear Lines after xx lines
        /// </summary>
        /// <value>
        /// 0: no clear
        /// </value>
        public int ClearLines { get; set; } = 0;

        /// <summary>
        /// Limits no of Lines
        /// </summary>
        /// <value>
        /// 0: no limit
        /// </value>
        public int MaxLines { get; set; } = 0;

        public bool RedTextAddedd { get; set; } = false;


        #region AddStatusString

        /// <summary>
        /// Adds the status string without DateTime, ignoring AddDate and AddTime
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="LineBreak_Pre">if set to <c>true</c> [line break pre].</param>
        /// <param name="Col">The col.</param>
        /// <param name="LineBreak_Post">if set to <c>true</c> [line break post].</param>
        public void AddStatusStringNoDateTime(string text, bool LineBreak_Pre, Color Col, bool LineBreak_Post = false)
        {
            bool buAddTime = this.AddTime;
            bool buAddDate = this.AddDate;
            this.AddTime = false;
            this.AddDate = false;
            AddStatusString(text, LineBreak_Pre, Col, LineBreak_Post);
            this.AddTime = buAddTime;
            this.AddDate = buAddDate;
        }

        public void AddStatusStringNoDateTime(string text, Color Col)
        {
            AddStatusStringNoDateTime(text, true, Col);
        }

        /// <summary>
        /// Adds the status string.
        /// </summary>
        /// <remarks>
        /// Thread safe
        /// </remarks>
        public void AddStatusString(string text)
        {
            AddStatusString(text, true, Color.Black);
        }

        delegate void AddStatusStringDelegate(string text, Color Col);

        /// <summary>
        /// Adds the status string.
        /// </summary>
        /// <remarks>
        /// Thread safe
        /// </remarks>
        public void AddStatusString(string text, bool LineBreak_Pre, Color Col, bool LineBreak_Post = false)
        {
            if (this.InvokeRequired)
            {
                AddStatusStringDelegate addStatusString = new AddStatusStringDelegate(AddStatusString);
                Invoke(addStatusString, new object[] { text, Col });
            }
            else
            {
                if (ClearLines != 0)
                {
                    if (this.Lines.Length >= ClearLines) this.Clear();
                }

                /*MaxLines check*/
                if (MaxLines > 0 && this.Lines.Length > MaxLines)
                {
                    List<string> lines = new List<string>(this.Lines);
                    while (lines.Count >= MaxLines)
                    {
                        lines.RemoveAt(0);
                    }
                    this.Lines = lines.ToArray();
                }

                string pref = "";
                if (AddDate && AddTime)
                {
                    pref = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString() + ": ";
                }
                else if (AddDate)
                {
                    pref = DateTime.Now.ToShortDateString() + ": ";
                }
                else if (AddTime)
                {
                    pref = DateTime.Now.ToShortTimeString() + ": ";
                }

                if (LineBreak_Pre) AddLineBreak();

                if (Col == Color.Red)
                    RedTextAddedd = true;

                try
                {
                    this.SelectionColor = Col;
                    this.AppendText(pref + text);

                    if (LineBreak_Post) AddLineBreak();
                    this.ScrollToCaret();
                }
                catch { }
                finally { }
            }
        }

        private void AddLineBreak ()
        {
            try
            {
                this.AppendText(Environment.NewLine);
            }
            catch (Exception) { }
            finally { }
        }


        public void AddStatusString(string text, Color Col)
        {
            AddStatusString(text, true, Col);
        }

        #endregion
    }
}