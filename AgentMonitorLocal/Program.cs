using System.Linq;
using aamcommon;
using aamws;
using System;
using System.IO;
using System.Threading;

namespace AgentMonitorLocal_prototype
{
    class Program
    {
        private enum ExecutionMode
        {
            agent,
            observer
        };

        private static ExecutionMode executionMode = ExecutionMode.agent;

        static void Main(string[] args)
        {
            try
            {
                var httpInterface = new HttpInterface();

                args.LastOrDefault(a => Enum.TryParse(a.Trim('/', '-'), true, out executionMode));
                Console.WriteLine("execution mode: {0}", executionMode);

                switch (executionMode)
                {
                    case ExecutionMode.agent:
                        var rec = new TestAgentStatusRecord();
                        rec.Changed += (sender) => Console.WriteLine("{0} - {1}", DateTime.Now.ToLongTimeString(), sender);
                        rec.Changed += (sender) => httpInterface.SendStatus(((TestAgentStatusRecord)sender).AsJson);

                        var parser = new LogParser();
                        parser.Changed += rec.Update;
                        parser.Start();

                        break;

                    case ExecutionMode.observer:
                        break;
                }
                Console.WriteLine("Press any key to abort.{0}{0}", Environment.NewLine);
                Console.ReadLine();

            }
            catch (Exception e)
            {
                Console.WriteLine("Terminated because of a {0}:\n{1}", e.GetType().Name, e.StackTrace);
            }
        }
    }
}
