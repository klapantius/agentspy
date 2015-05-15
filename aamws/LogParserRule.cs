using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using aamcommon;
using System.Collections.Concurrent;

namespace aamws
{
    public class LogParserRule : ILogParserRule
    {
        private readonly Regex regex;

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

        public IDictionary<Field, string> Parse(string line)
        {
            if (!IsMatching(line)) return null;

            var result = new ConcurrentDictionary<Field, string>();
            var groups = regex.Match(line).Groups;
            foreach (var groupName in regex.GetGroupNames().Where(n => n!="0"))
            {
                var field = (Field) Enum.Parse(typeof (Field), groupName);
                while (!result.TryAdd(field, groups[groupName].Value))
                {
                    Thread.Sleep(1000);
                }
            }

            return result;
        }

    }
}