using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using tail;

namespace tail_uTest
{
    [TestFixture]
    class TailTests
    {
        [TestCase]
        public void GetNewLines()
        {
            const string FIRSTLINE= "alma a fa alatt";
            var source = new MockedFileStream(FIRSTLINE);
            var reader = new FileReader("dummyFileName") { myFileStream = source };

            var tail = new Tail("dummyFileName", Encoding.Default) { myFileReader = reader };

            Assert.AreEqual(Encoding.ASCII, tail.myFileType, "Encoding detected wrong.");
            var lines = tail.GetNewLines();
            Assert.AreEqual(1, lines.Count, "Unexpected count of lines read.");
            StringAssert.AreEqualIgnoringCase(FIRSTLINE, lines.First(), "Wrong content.");
        }

        [TestCase]
        public void GetNewLines_ContinuousReading()
        {
            var input = new List<string>
            {
                "first line",
                "second line",
                "third line"
            };
            var additionalLines = new List<string>
            {
                "fourth line",
                "fifth line",
                "sixth line",
                "7th line"
            };
            var source = new MockedFileStream(input);
            var reader = new FileReader("dummyFileName") {myFileStream = source};
            var tail = new Tail("dummyFileName", Encoding.Default) {myFileReader = reader};

            var output = tail.GetNewLines();
            Assert.AreEqual(input.Count, output.Count, "Unexpected count of lines at start");
            for (var i = 0; i < input.Count; i++)
            {
                StringAssert.AreEqualIgnoringCase(input[i], output[i], "Unmatching line in the 1st round.");
            }

            source.AddNewLines(additionalLines, Encoding.Default);
            output = tail.GetNewLines();
            Assert.AreEqual(additionalLines.Count, output.Count, "Unexpected count of lines after new lines added.");
            for (var i = 0; i < additionalLines.Count; i++)
            {
                StringAssert.AreEqualIgnoringCase(additionalLines[i], output[i], "Unmatching line in the 2nd round.");
            }
        }

    }
}
