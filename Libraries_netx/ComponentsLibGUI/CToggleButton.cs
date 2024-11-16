using System.Runtime.Versioning;

namespace ComponentsLibGUI
{
    [SupportedOSPlatform("windows")]
    public class CToggleButton : Button
    {
        // Fields
        private string _TextState1 = string.Empty;
        private string _TextState2 = string.Empty;
        private Color _ColorState1;
        private Color _ColorState2;
        private bool _AcceptChange;

        // Constructor
        public CToggleButton()
        {
            // Set Default toggled Text
            _TextState1 = "State1";
            _TextState2 = "State2";

            // Set Default toggled Color
            _ColorState1 = Color.Gray;
            _ColorState2 = BackColor;

            _AcceptChange = true;
        }

        public string TextState1
        {
            get { return _TextState1; }
            set
            {
                _TextState1 = value;
                {
                    Text = _TextState1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the text state2.
        /// </summary>
        /// <value>
        /// The text state2.
        /// </value>
        public string TextState2
        {
            get { return _TextState2; }
            set { _TextState2 = value; }
        }

        /// <summary>
        /// Gets or sets the color state1.
        /// </summary>
        /// <value>
        /// The color state1.
        /// </value>
        public Color ColorState1
        {
            get { return _ColorState1; }
            set
            {
                _ColorState1 = value;
                {
                    BackColor = _ColorState1;
                }
            }
        }

        /// <summary>
        /// Gets or sets the color state2.
        /// </summary>
        /// <value>
        /// The color state2.
        /// </value>
        public Color ColorState2
        {
            get { return _ColorState2; }
            set { _ColorState2 = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [accept change].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [accept change]; otherwise, <c>false</c>.
        /// </value>
        public bool AcceptChange
        {
            get { return _AcceptChange; }
            set { _AcceptChange = value; }
        }

        public event EventHandler? ToState2;
        protected virtual void OnToState2(EventArgs e)
        {
            ToState2?.Invoke(this, e);
        }

        public event EventHandler? ToState1;
        protected virtual void OnToState1(EventArgs e)
        {
            ToState1?.Invoke(this, e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e); // Call the CheckBox Baseclass

            // Set Text and Color according to the current state
            _AcceptChange = true;
            if (Text == _TextState2)
            {
                //State 2 -> State 1
                OnToState1(new EventArgs());
                if (_AcceptChange)
                {
                    Text = _TextState1;
                    BackColor = _ColorState1;
                }
            }
            else
            {
                //State 1 -> State 2
                OnToState2(new EventArgs());
                if (_AcceptChange)
                {
                    Text = _TextState2;
                    BackColor = _ColorState2;
                }
            }
        }

        /// <summary>
        /// Lets Button go to State1
        /// </summary>
        /// <param name="TriggerEvent">if set to <c>true</c>corresponding event is triggered</param>
        public void GoToState1(bool TriggerEvent)
        {
            //this.Text = this._TextState1;
            Invoke(new Action(() => Text = _TextState1));
            Invoke(new Action(() => BackColor = _ColorState1));

            //this.BackColor = this._ColorState1;
            if (TriggerEvent) OnToState1(new EventArgs());
        }

        /// <summary>
        /// Lets Button go to State2
        /// </summary>
        /// <param name="TriggerEvent">if set to <c>true</c>corresponding event is triggered</param>
        public void GoToState2(bool TriggerEvent)
        {
            //this.Text = this._TextState2;
            //this.BackColor = this._ColorState2;
            Invoke(new Action(() => Text = _TextState2));
            Invoke(new Action(() => BackColor = _ColorState2));

            if (TriggerEvent) OnToState2(new EventArgs());
        }
    }
}


