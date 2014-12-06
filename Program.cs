using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace agentspy.net
{
    class Program
    {
        public static OptionsStore Options = new OptionsStore();

        private static void Main(string[] args)
        {
            Options.Add(new[] { "/computer:" }, "agent name");
            Options.Add(new[] { "/testdata:" }, "local path to a log file");

            var spy = new AgentSpy();
            Console.ForegroundColor = ConsoleColor.Gray;

            //spy.ScanVSTTAgentProcess();
            //spy.ScanVSTTAgentProcess_ColorByJobId();
            spy.GroupByJobId();

            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("done, press a key to finish");
            Console.ReadKey();
        }

    }

    class AgentSpy
    {
        public static readonly string[] IsRunningStates = new[] { "started", "running", "completed", "disposed" };
        public static bool IsRunning(string state) { return IsRunningStates.Any(irs => 0 == string.Compare(irs, state, true)); }
        public static readonly string[] IsMakingEnvStates = new[] { "runsetupscript", "runcleanupscript" };
        public static bool IsMakingEnvironment(string state) { return IsMakingEnvStates.Any(es => 0 == string.Compare(es, state, true)); }

        private List<InfoItem> fields = new List<InfoItem>()
        {
            new InfoItem("linenr", "", "{0,7}") {MarkChanges = false},
                
            new InfoItem("procid", "", "{0,7}", new [] { new Regex(@"^., (\d+),.*StateMachine"),}) {MarkChanges = false},

            new InfoItem("jobid", "", "{0,4}", new [] { new Regex(@"^., \d+, (\d+),.*StateMachine"),}) {MarkChanges = false},

            new InfoItem("datum", "", "{0,-10}", new [] { new Regex(@"\d+, (\d{4}/\d{1,2}/\d{1,2}),.*StateMachine"),}) {MarkChanges = false},

            new InfoItem("time", "", "{0,10}", new [] { new Regex(@"\d+, (\d{2}:\d{2}:\d{2}.\d{3}),.*StateMachine"), }) {MarkChanges = false},

            new StateItem("state", "n/a", "{0,-17}", new[] { new Regex("calling state handler for (.*)")}) { MarkChanges = false },

            new InfoItem("build", "n/a", "{0,-50}", new[] { new Regex("logDirGuardedExec=.*\\\\(.*)\\\\logs") })
            {UndefinedInStatus = new List<string>(){"Online", "Deploying"}},
                
            new InfoItem("assembly", "n/a", "{0,-50}", new[]
            {
                new Regex("UtfTest.Storage=.*\\\\(.*).dll"),
                new Regex("TestAssemblies=.*([a-z,A-Z,0-9,\\.,_]*).dll")
            })
            {UndefinedInStatus = new List<string>(){"Online", "Deploying"}},

            new InfoItem("tc", "", "{0}", new[] { new Regex("HumanReadableId=.(.*).,"), })
            {UndefinedInStatus = new List<string>(){"Online", "Deploying", "Waiting", "RunCleanupScript", "RunCompleted"}},
        };

        private ILogReader myLogReader;

        private ILogReader CreateNewLogReader()
        {
            if (Program.Options.IsDefined("testdata"))
            {
                return new LocalLogReader(Program.Options["testdata"]);
            }
            else if (Program.Options.IsDefined("computer"))
            {
                return new RemoteLogReader(Program.Options["computer"]);
            }
            else
            {
                throw new Exception("Neither an agent name nor a local path is defined.");
            }
        }

        public InfoItem this[string fieldName]
        {
            get
            {
                var field = fields.SingleOrDefault(f => f.Name == fieldName);
                if (field == null) throw new IndexOutOfRangeException(string.Format("There is no field like \"{0}\"", fieldName));
                return field;
            }
        }

        public InfoItem Field(string fieldName)
        {
            var field = fields.SingleOrDefault(f => f.Name == fieldName);
            if (field == null) throw new IndexOutOfRangeException(string.Format("There is no field like \"{0}\"", fieldName));
            return field;
        }

        public class Job
        {
            public string jobid;
            public List<Dictionary<string, string>> lines = new List<Dictionary<string, string>>();

            public Job(string id)
            {
                jobid = id;
            }
        }

        public class JobCollection
        {
            private List<Job> jobs = new List<Job>();
            public Job this[string id]
            {
                get
                {
                    var result = jobs.SingleOrDefault(j => j.jobid == id);
                    if (null == result)
                    {
                        result = new Job(id);
                        jobs.Add(result);
                    }
                    return result;
                }
            }
            public IEnumerator<Job> GetEnumerator() { return jobs.GetEnumerator(); }
        }

        public void GroupByJobId(int max = int.MaxValue)
        {
            using (var file = CreateNewLogReader())
            {
                var count = 0;
                var jobs = new JobCollection();
                var executingJob = string.Empty;
                var environmentJob = string.Empty;
                Job currentJob;
                while (!file.EndOfStream && count++ < max)
                {
                    var line = file.ReadLine() ?? string.Empty;
                    // reset all fields
                    fields.ForEach(f => f.Reset());
                    // match all fields, skip line if no field is matching
                    if (!fields.Select(f => f.Evaluate(line)).ToList().Any()) continue;
                    // skip line if it is neither a state nor an environment line
                    if (new[] { "state", "assembly", "tc" }.All(field => string.IsNullOrEmpty(this[field].Value.Trim()))) continue;
                    var state = this["state"].Value;
                    // store job id of an executing or environment job line
                    if (!string.IsNullOrEmpty(state))
                    {
                        this["linenr"].Value = count.ToString();
                        currentJob = jobs[this["jobid"].Value];
                        if (IsRunning(state))
                        {
                            if (!string.IsNullOrEmpty(executingJob) && executingJob != currentJob.jobid)
                            {
                                Console.WriteLine("Cannot identify exactly one test executing job ({0} vs {1} at {2})", executingJob, currentJob.jobid, count);
                            }
                            executingJob = currentJob.jobid;
                        }
                        else if (IsMakingEnvironment(state))
                        {
                            if (!string.IsNullOrEmpty(environmentJob) && environmentJob != currentJob.jobid)
                            {
                                Console.WriteLine("Cannot identify exactly one environment job ({0} vs {1} at {2})", environmentJob, currentJob.jobid, count);
                            }
                            environmentJob = currentJob.jobid;
                        }
                    }
                    // transfer job id, assembly and test case data from active jobs
                    else
                    {
                        var assembly = this["assembly"].Value.Trim();
                        var tc = this["tc"].Value.Trim();
                        if (!string.IsNullOrEmpty(tc) && executingJob != string.Empty)
                        {
                            this["jobid"].Value = executingJob;
                            this["assembly"].Value = assembly;
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(assembly) && environmentJob != string.Empty)
                            {
                                this["jobid"].Value = environmentJob;
                            }
                        }
                        currentJob = jobs[this["jobid"].Value];
                        if (null == currentJob) continue;
                    }
                    currentJob.lines.Add(fields.ToDictionary(f => f.Name, f => f.ToString()));
                }
                foreach (var job in jobs)
                {
                    Console.WriteLine(job.jobid);
                    foreach (var line in job.lines)
                    {
                        Console.WriteLine("\t{0} {1} {2} {3} {4}", line["linenr"], line["time"], line["state"], line["assembly"], line["tc"]);
                    }
                }
            }
        }

    }

    public class OptionsStore
    {
        public class OptionItem
        {
            public List<string> Names { get; private set; }
            public string Description { get; private set; }
            public string DefaultValue { get; private set; }
            public string Value { get; set; }

            public OptionItem(string[] names, string description = "", string defaultValue = "")
            {
                Names = new List<string>(names);
                Description = description;
                DefaultValue = defaultValue;
                Value = defaultValue;
            }
        }

        public void Add(string[] names, string description = null, string defaultValue = null)
        {
            myOptions.Add(new OptionItem(names, description, defaultValue));
        }

        private bool isInitialized = false;
        private List<OptionItem> myOptions = new List<OptionItem>();
        private List<string> parsingErrors = new List<string>();
        public List<string> ParsingErrors { get { return parsingErrors; } }

        private void ParseCommandLine()
        {
            parsingErrors.Clear();
            foreach (var arg in Environment.GetCommandLineArgs().Skip(1))
            {
                try
                {
                    var argParser = new Regex(@"[/,-](\w*):?(.*)");
                    if (!argParser.IsMatch(arg))
                    {
                        throw new InvalidOperationException();
                    }
                    var argParts = argParser.Match(arg).Groups;
                    var argName = argParts[1].Value;
                    var opt = FindMyOption(argName);
                    if (argParts.Count > 2)
                    {
                        opt.Value = argParts[2].Value;
                    }
                    else
                    {
                        opt.Value = "true";
                    }
                }
                catch (InvalidOperationException)
                {
                    parsingErrors.Add(string.Format("Error: could not parse \"{0}\"", arg));
                }
            }
            isInitialized = true;
        }

        private OptionItem FindMyOption(string name)
        {
            var nameFinder = new Regex(string.Format(@"[/,-]({0})", name.Replace("/", "").Replace("-", "")));
            return myOptions.SingleOrDefault(o => o.Names.Any(n => nameFinder.IsMatch(n)));
        }

        public string this[string name]
        {
            get
            {
                if (!isInitialized) ParseCommandLine();
                var opt = FindMyOption(name);
                return opt != null ? opt.Value : null;
            }
        }

        public bool IsDefined(string optionName)
        {
            if (!isInitialized) ParseCommandLine();
            var opt = FindMyOption(optionName);
            return null != opt && null != opt.Value;
        }

    }

}
