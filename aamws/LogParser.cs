using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using aamcommon;
using tail;

[assembly: InternalsVisibleTo("aamws_test")]

namespace aamws
{
    public class LogParser : ILogParser
    {
        public const string FileNameOfAgentLog = "VSTTAgentProcess.log";

        public static readonly List<ILogParserRule> Rules = new List<ILogParserRule>()
        {
            new LogParserRule(
                @"(?<" + Field.LogType + @"\w)" +
                @", \d*, (?<" + Field.JobId + @">\d*), " +
                @"(?<" + Field.LastUpdated + @">\d*/\d*/\d*, \d*:\d*:\d*.\d*), " +
                @".*.exe, StateMachine\(AgentState\): calling state handler for (?<" + Field.Status + @">.*)"),
            new LogParserRule(@"^TestAssemblies=(?<" + Field.Assembly + @">.*)$"),
            new LogParserRule(@"HumanReadableId=.(?<" + Field.TC + @">.*)., Id=")
        };

        internal ITail Tail { get; set; }
        internal List<IJob> Jobs { get; set; }

        public LogParser()
        {
            Jobs = new List<IJob>();
        }

        public void Start()
        {
            if (null == Tail) Tail = new Tail(FileNameOfAgentLog, Encoding.Default);

            Tail.Changed += TailUpdateHandler;
        }

        private void Stop()
        {
            Tail.Changed -= TailUpdateHandler;
            Tail = null;
        }

        public void Dispose()
        {
            Stop();
            Jobs.Clear();
        }

        public void TailUpdateHandler(object o, TailEventArgs e)
        {
            foreach (var line in e.NewLines)
            {
                var matching = Rules.SingleOrDefault(r => r.IsMatching(line));
                if (null == matching) continue;
                var result = matching.Parse(line);
                if (!string.IsNullOrEmpty(result[Field.JobId]))
                {
                    if (Jobs.All(j => j.Id != result[Field.JobId]))
                    {
                        var newJob = new Job(result[Field.JobId]);
                        newJob.Update(result);
                        Jobs.Add(newJob);
                    }
                }
                else
                {

                }
            }
        }

        public void Remove(string jobid)
        {
            throw new NotImplementedException();
        }
    }
}
