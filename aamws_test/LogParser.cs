using System.Collections.Generic;
using System.Data;
using System.Linq;
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
