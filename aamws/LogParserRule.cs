using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using aamcommon;

namespace aamws
{
    public class LogParserRule : ILogParserRule
    {
        private Regex regex;

        public LogParserRule(string rule)
        {
            Rule = rule;
            regex=new Regex(Rule);
        }

        public string Rule { get; set; }

        public bool IsMatching(string line)
        {
            return regex.IsMatch(line);
        }

        public Dictionary<Field, string> Parse(string line)
        {
            if (!IsMatching(line)) return null;

            var result = new Dictionary<Field, string>();
            var groups = regex.Match(line).Groups;
            foreach (var groupName in regex.GetGroupNames().Where(n => n!="0"))
            {
                var field = (Field) Enum.Parse(typeof (Field), groupName);
                result.Add(field, groups[groupName].Value);
            }

            return result;
        }

    }
}