using System.Text.RegularExpressions;

namespace agentspy.net
{
    class StateItem : InfoItem
    {
        public StateItem(string name, string defaultValue = "n/a", string formatString = "{0}", params Regex[] patterns)
            : base(name, defaultValue, formatString, patterns)
        {
        }

        new public string Value
        {
            get { return base.Value; }
            internal set
            {
                var ori = this.myValue;
                base.Value = value;
                if (this.myValue != ori) CurrentState = this.myValue;
            }
        }
    }
}