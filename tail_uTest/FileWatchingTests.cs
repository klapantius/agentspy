using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using tail;

namespace tail_uTest
{
    [TestFixture]
    class FileWatchingTests
    {
        private static string TestFileName = Environment.ExpandEnvironmentVariables(@"%TEMP%\TailTestFile.log");

        [TestCase]
        public void WatchingWhileTheTargetGrows()
        {
            var firstLines = new List<string>()
            {
                "hello world",
                "foo bar",
                "lorem ipsum"
            };
            var additionalLines = new List<string>()
            {
                "monday",
                "tuesday",
                "wednesday",
                "thursday",
                "friday"
            };

            var tail = new Tail(TestFileName, Encoding.ASCII);
            var results = new List<string>();
            var changeEventRaised = 0;
            tail.Changed += (o, e) => { ++changeEventRaised; results.AddRange(e.NewLines); };
            tail.Watch();
            try
            {
                WriteTestLines(false, firstLines);
                Assert.Greater(changeEventRaised, 0, "Change event has not been raised at all.");
                Assert.AreEqual(firstLines.Count, results.Count, "Mismatching count of collected lines.");

                changeEventRaised = 0;
                results.Clear();
                WriteTestLines(true, additionalLines);
                Assert.Greater(changeEventRaised, 0, "Change event has not been raised at all.");
                Assert.AreEqual(additionalLines.Count, results.Count, "Mismatching count of collected lines.");
            }
            finally
            {
                tail.StopWatching();
            }
        }

        private static StreamWriter CreateWriter(bool append=false)
        {
            return new StreamWriter(TestFileName, append, Encoding.ASCII, 4096) {AutoFlush = false};
        }

        private static void Finalize(StreamWriter writer)
        {
            writer.Flush();
            writer.Close();
            Thread.Sleep(1000);
        }

        private static void WriteTestLines(bool append, IEnumerable<string> lines)
        {
            var w = CreateWriter(append);
            lines.ToList().ForEach(l => w.WriteLine(l));
            Finalize(w);
        }

    }

}
