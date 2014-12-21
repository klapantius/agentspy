using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
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
            var writer = new StreamWriter(TestFileName, false, Encoding.ASCII, 4096) { AutoFlush = false };

            var tail = new Tail(TestFileName, Encoding.ASCII);
            var results = new List<string>();
            var changeEventRaised = 0;
            tail.Changed += (o, e) => { ++changeEventRaised; results.AddRange(e.NewLines); };
            tail.Watch();

            firstLines.ForEach(l => writer.WriteLine(l));
            writer.Flush();
            Thread.Sleep(1000);

            tail.StopWatching();

            Assert.Greater(changeEventRaised, 0, "Change event has not been raised at all.");
            Assert.AreEqual(firstLines.Count, results.Count, "Mismatching count of collected lines.");
        }

    }

}
