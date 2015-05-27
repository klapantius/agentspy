﻿using System.Linq;
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

                args.LastOrDefault(a => Enum.TryParse(a.Trim('/', '-'), true, out executionMode));
                Console.WriteLine("execution mode: {0}", executionMode);

                switch (executionMode)
                {
                    case ExecutionMode.agent:
                        var a = new Agent();
                        break;

                    case ExecutionMode.observer:
                        var o = new Observer();
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
