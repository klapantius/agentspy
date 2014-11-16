using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace agentspy.net
{
    class Program
    {
        private static void Main(string[] args)
        {
            var spy = new AgentSpy("DEERLN1TBL24V04");
            spy.ScanVSTTAgentProcess_ColorByJobId();
            Console.WriteLine("done, press a key to finish");
            Console.ReadKey();
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

            public string Computer { get; private set; }

            public AgentSpy(string computer)
            {
                Computer = computer;
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
                using (var file = new RemoteLogReader(Computer))
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
                using (var file = new RemoteLogReader(Computer))
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
    }
}
