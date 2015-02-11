using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using aamws;
using Moq;
using NUnit.Framework;
using tail;

namespace aamws_test
{
    [TestFixture]
    class LogParserTests
    {
        const string TestLine = @"V, 4764, 8, 2014/11/15, 06:23:26.890, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Completing";
        private static readonly Dictionary<string, string> TestLineDictionary = new Dictionary<string, string>()
        {
            {"Queuing", @"V, 4764, 8, 2014/11/15, 04:42:02.568, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Queuing"},
	        {"Deploying", @"V, 4764, 8, 2014/11/15, 04:42:02.583, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Deploying"},
	        {"Deployed", @"V, 4764, 8, 2014/11/15, 04:42:02.630, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Deployed"},
	        {"RunSetupScript", @"V, 4764, 8, 2014/11/15, 04:42:02.630, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for RunSetupScript"},
	        {"InitializeDataCollectors", @"V, 4764, 8, 2014/11/15, 04:42:27.715, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for InitializeDataCollectors"},
	        {"Synchronizing", @"V, 4764, 8, 2014/11/15, 04:42:27.731, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Synchronizing"},
	        {"Starting", @"V, 4764, 8, 2014/11/15, 04:42:27.731, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Starting"},
	        {"Running", @"V, 4764, 8, 2014/11/15, 04:42:27.731, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Running"},
	        {"Completing", @"V, 4764, 8, 2014/11/15, 04:44:59.020, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Completing"},
	        {"Waiting", @"V, 4764, 8, 2014/11/15, 04:44:59.020, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Waiting"},
	        {"CleanupDataCollectors", @"V, 4764, 8, 2014/11/15, 04:44:59.035, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for CleanupDataCollectors"},
	        {"RunCleanupScript", @"V, 4764, 8, 2014/11/15, 04:45:02.920, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for RunCleanupScript"},
	        {"Cleanup", @"V, 4764, 8, 2014/11/15, 04:45:18.052, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Cleanup"},
	        {"RunCompleted", @"V, 4764, 8, 2014/11/15, 04:45:20.891, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for RunCompleted"},
	        {"Online", @"V, 4764, 8, 2014/11/15, 04:45:20.907, DEERLN1TBL24V04\QTAgent_40.exe, StateMachine(AgentState): calling state handler for Online"},
        };
            
            [TestCase]
        public void CanParseStatusLines()
        {
            var v = LogParser.Rules.First().Parse(TestLine);
            Assert.IsTrue(LogParser.Rules.Any(r => r.IsMatching(TestLine)), "Sample status line should match to a rule.");
        }

        [TestCase]
        public void CanCreateNewJob()
        {
            var parser = new LogParser();
            Assert.AreEqual(0, parser.Jobs.Count, "Unexpected job(s) while parser is just initialized.");
            parser.TailUpdateHandler(null, new TailEventArgs(new List<string>() { TestLine }));
            Assert.Greater(parser.Jobs.Count, 0, "No job created while trying to parse a status line.");
        }

    }
}
