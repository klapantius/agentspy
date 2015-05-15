using aamcommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using tail;

[assembly: InternalsVisibleTo("aamws_test")]

namespace aamws
{
    public class LogParser : ILogParser
    {
        public const string FileNameOfAgentLog = @"C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\IDE\VSTTAgentProcess.log";
        private string lastJobId = null;
        private bool init;

        public static readonly List<ILogParserRule> Rules = new List<ILogParserRule>()
        {
            new LogParserRule(
                @"(?<" + Field.LogType + @">\w)" +
                @", \d*, (?<" + Field.JobId + @">\d*), " +
                @"(?<" + Field.TimeStamp + @">\d*/\d*/\d*, \d*:\d*:\d*.\d*), " +
                @"(?<" + Field.AgentName + @">[\w\d_]+)\\.*.exe" +
                @", StateMachine\(AgentState\): calling state handler for (?<" + Field.Status + @">.*)"),
            new LogParserRule(@"AgentSetting ControllerName==(?<" + Field.Controller + @">.*)"),
            new LogParserRule(@"^TestAssemblies=(?<" + Field.Assembly + @">[\d\w\._]+)\.dll$"),
            new LogParserRule(@"HumanReadableId=.(?<" + Field.TC + @">.*)., Id="),
            new LogParserRule(@"logDirGuardedExec=.*tfssysint\$\\(?<" + Field.Build + @">.*)\\logs"),
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

            init = true;

            Tail.Changed += TailUpdateHandler;
            Tail.Watch();
        }

        private void Stop()
        {
            Tail.Changed -= TailUpdateHandler;
            Tail.StopWatching();
            Tail = null;
        }

        public void Dispose()
        {
            Stop();
            Jobs.Clear();
        }

        #region changed event
        public event LogParserEventHandler Changed;

        protected virtual void OnChanged(string jobid, IDictionary<Field, string> fieldsToBeUpdated)
        {
            if (Changed != null) Changed(jobid, fieldsToBeUpdated, !init);
        }

        #endregion
        public void TailUpdateHandler(object o, TailEventArgs e)
        {
            IDictionary<Field, string> result = null;
            foreach (var line in e.NewLines)
            {
                var matching = Rules.SingleOrDefault(r => r.IsMatching(line));
                if (null == matching) continue;
                result = matching.Parse(line);
                //if (result.ContainsKey(Field.TimeStamp)) Console.WriteLine(result[Field.TimeStamp]);
                var jobid = result.ContainsKey(Field.JobId) ? result[Field.JobId] : "";
                if (!string.IsNullOrEmpty(jobid))
                {
                    var job = Jobs.SingleOrDefault(j => j.Id == jobid);
                    if (job == null) Jobs.Add(job = new Job(jobid));
                    job.Update(result);
                    if (result.ContainsKey(Field.Status)) result[Field.Status] = job.Status.ToString();
                    lastJobId = jobid;
                    OnChanged(jobid, result);
                }
                else
                {
                    if (string.IsNullOrEmpty(lastJobId)) continue;
                    Jobs.Single(j => j.Id == lastJobId).Update(result);
                    OnChanged(jobid, result);
                }
            }
            if (init)
            {
                init = false;
                if (null!=result) OnChanged(lastJobId, result);
            }
        }

        public void Remove(string jobid)
        {
            throw new NotImplementedException();
        }
    }
}
