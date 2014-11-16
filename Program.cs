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

            spy.ScanVSTTAgentProcess_ColorByJobId();
            Console.WriteLine("done, press a key to finish");
            Console.ReadKey();
        }


    }

    class AgentSpy
    {
        private List<InfoItem> fields = new List<InfoItem>()
        {
            new InfoItem("linenr", "", "{0,7}") {MarkChanges = false},
                
            new InfoItem("procid", "", "{0,7}", new [] { new Regex(@"^., (\d+),"),}) {MarkChanges = false},

            new InfoItem("jobid", "", "{0,4}", new [] { new Regex(@"^., \d+, (\d+),"),}) {MarkChanges = false},

            new InfoItem("datum", "", "{0,-10}", new [] { new Regex(@"\d+, (\d{4}/\d{1,2}/\d{1,2}),"),}) {MarkChanges = false},

            new InfoItem("time", "", "{0,10}", new [] { new Regex(@"\d+, (\d{2}:\d{2}:\d{2}.\d{3}),"), }) {MarkChanges = false},

            new StateItem("state", "n/a", "{0,-17}", new[] { new Regex("SetNextState (.*) ")}) { MarkChanges = false },

            new InfoItem("build", "n/a", "{0,-50}", new[] { new Regex("logDirGuardedExec=.*\\\\(.*)\\\\logs") })
            {UndefinedInStatus = new List<string>(){"Online", "Deploying"}},
                
            new InfoItem("assembly", "n/a", "{0,-50}", new[]
            {
                new Regex("UtfTest.Storage=.*\\\\(.*).dll"),
                //new Regex("TestAssemblies=.*([a-z,A-Z,0-9,\\.,_]*).dll(.*)")
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
        public void ScanVSTTAgentProcess(int max = int.MaxValue)
        {
            using (var file = CreateNewLogReader())
            {
                var count = 0;
                while (!file.EndOfStream && count++ < max)
                {
                    var line = file.ReadLine() ?? string.Empty;
                    var changeHappened = fields.AsParallel().Select(f => f.Evaluate(line)).ToList();
                    this["linenr"].Value = count.ToString();
                    if (changeHappened.Any(c => c == true))
                    {
                        fields.AsParallel().Where(f =>
                            f.UndefinedInStatus.Any(s => s == this["state"].Value)).
                            ForAll(f => f.Value = "");
                        Console.WriteLine(string.Join(" ", fields));
                    }
                }
            }
        }

        public void ScanVSTTAgentProcess_ColorByJobId(int max = int.MaxValue)
        {
            using (var file = CreateNewLogReader())
            {
                var count = 0;
                var colors = new ConsoleColor[]
                {
                    ConsoleColor.Yellow,
                    ConsoleColor.Red,
                    ConsoleColor.Green,
                    ConsoleColor.Blue,
                    ConsoleColor.Magenta,
                    ConsoleColor.White,
                    ConsoleColor.DarkYellow,
                    ConsoleColor.DarkRed,
                    ConsoleColor.DarkGreen,
                    ConsoleColor.DarkCyan,
                    ConsoleColor.DarkMagenta,
                };
                int colorPointer = 0;
                var jobColors = new Dictionary<string, ConsoleColor>();
                ConsoleColor currentColor;
                while (!file.EndOfStream && count++ < max)
                {
                    var line = file.ReadLine() ?? string.Empty;
                    var changeHappened = fields.AsParallel().Select(f => f.Evaluate(line)).ToList();
                    this["linenr"].Value = count.ToString();
                    if (changeHappened.Any(c => c == true))
                    {
                        fields.AsParallel().Where(f =>
                            f.UndefinedInStatus.Any(s => s == this["state"].Value)).
                            ForAll(f => f.Value = "");
                        var jobid = this["jobid"].Value;
                        if (!string.IsNullOrEmpty(jobid))
                        {
                            if (!jobColors.ContainsKey(jobid))
                            {
                                currentColor = colors[colorPointer++ % colors.Count()];
                                jobColors[jobid] = currentColor;
                            }
                            currentColor = jobColors[jobid];
                        }
                        else
                        {
                            currentColor = ConsoleColor.Gray;
                        }
                        Console.ForegroundColor = currentColor;
                        Console.WriteLine("{0} {1} {2} {3}", this["jobid"], this["state"], this["assembly"], this["tc"]);
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
