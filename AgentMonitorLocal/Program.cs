using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using aamcommon;
using aamws;

namespace AgentMonitorLocal_prototype
{
    class Program
    {
        private const string INPUTFILENAME = "VSTTAgentProcess.test.log";

        static void Main(string[] args)
        {
            if (!File.Exists(INPUTFILENAME))
            {
                Console.WriteLine("cannot find {0}", INPUTFILENAME);
                Console.ReadKey();
            }
            var input = new StreamReader(INPUTFILENAME);
            var output = new StreamWriter(LogParser.FileNameOfAgentLog, false);

            var timer = new Timer(state =>
            {
                var line = input.ReadLine();
                //Console.WriteLine(line);
                output.WriteLine(line);
                output.Flush();
            });
            timer.Change(0, 10);

            var rec = new TestAgentStatusRecord();
            rec.Changed += (sender, s) => Console.WriteLine("{0} - {1}", DateTime.Now.ToLongTimeString(), s);

            var parser = new LogParser();
            parser.Changed += rec.Update;
            //parser.Changed += (jobid, updated) => Console.WriteLine("{0}: {1}", jobid, string.Join("; ", updated.Select(u => string.Format("{0}:{1}", u.Key, u.Value))));
            parser.Start();

            Console.WriteLine("Tailing. Press any key to abort.{0}{0}", Environment.NewLine);
            Console.ReadKey();
        }
    }
}
