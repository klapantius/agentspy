using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace agentspy.net
{
    class InfoItem
    {
        public string Name { get; private set; }
        private const char ChangeFlag = '`';
        private bool justChanged;
        public bool MarkChanges { get; set; }
        public bool ChangesOnly { get; set; }
        internal string FormatString { get; set; }
        internal List<Regex> myPatterns = new List<Regex>() { new Regex(".*") };
        public List<string> UndefinedInStatus = new List<string>();
        public static string CurrentState;

        internal string myValue = "n/a";
        public string Value
        {
            get
            {
                var v = this.myValue;
                this.myValue = this.myValue.TrimEnd(ChangeFlag);
                return v;
            }
            internal set
            {
                if (0 == string.Compare(myValue.TrimEnd(ChangeFlag), value, StringComparison.InvariantCultureIgnoreCase))
                {
                    justChanged = false;
                    return;
                }
                this.myValue = value + (this.MarkChanges ? ChangeFlag.ToString(CultureInfo.InvariantCulture) : "");
                justChanged = true;
            }
        }
        public override string ToString()
        {
            return string.Format(FormatString, justChanged || !ChangesOnly ? Value : "");
        }

        public InfoItem(string name, string defaultValue = "n/a", string formatString = "{0}", params Regex[] patterns)
        {
            this.Name = name;
            this.myValue = defaultValue;
            this.FormatString = formatString;
            if (patterns != null) this.myPatterns = patterns.ToList();
            this.MarkChanges = true;
            this.ChangesOnly = false;
        }

        public bool Evaluate(string line)
        {
            var matchingPattern = this.myPatterns.FirstOrDefault(p => p.IsMatch(line));
            if (null == matchingPattern) return false;
            this.Value = matchingPattern.Match(line).Groups[1].Value;
            return justChanged;
        }
    }
}