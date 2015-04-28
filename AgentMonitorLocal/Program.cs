using aamcommon;
using aamws;
using System;
using System.IO;
using System.Threading;

namespace AgentMonitorLocal_prototype
{
    class Program
    {
        private const string INPUTFILENAME = "VSTTAgent.log"; //"VSTTAgentProcess.log";

        static void Main(string[] args)
        {
            try
            {
                var httpInterface = new HttpInterface();

                var rec = new TestAgentStatusRecord();
                rec.Changed += (sender, s) => Console.WriteLine("{0} - {1}", DateTime.Now.ToLongTimeString(), sender);
                rec.Changed += (sender, s) => httpInterface.SendStatus(((TestAgentStatusRecord)sender).StatusString);

                var parser = new LogParser();
                parser.Changed += rec.Update;
                parser.Start();

                Console.WriteLine("Tailing {1}. Press any key to abort.{0}{0}", Environment.NewLine, LogParser.FileNameOfAgentLog);
                Console.ReadLine();

            }
            catch (Exception e)
            {
                Console.WriteLine("Terminated because of a {0}", e.GetType().Name);
            }
        }
    }
}
