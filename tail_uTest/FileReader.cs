using NUnit.Framework;
using System.Linq;
using tail;

namespace tail_uTest
{
    [TestFixture]
    public class FileReaderTests
    {
        [TestCase]
        public void ReadLastNBytes_GetLastTwo()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader("dummyFileName") { myFileStream = mockedFileStream };

            var lastTwoBytes = reader.ReadLastNBytes(2);
            Assert.AreEqual(2, lastTwoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(4, lastTwoBytes[0], "Unexpected result.");
            Assert.AreEqual(5, lastTwoBytes[1], "Unexpected result.");
        }

        [TestCase]
        public void ReadLastNBytes_GetMoreThanPossible()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader("dummyFileName") { myFileStream = mockedFileStream };

            var last7Bytes = reader.ReadLastNBytes(7);
            Assert.AreEqual(5, last7Bytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(1, last7Bytes[0], "Unexpected result.");
            Assert.AreEqual(5, last7Bytes[4], "Unexpected result.");
        }

        [TestCase]
        public void ReadNewBytes_ReadThrough5ItemsBy2()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader("dummyFileName") { myFileStream = mockedFileStream };

            var twoBytes = reader.ReadNewBytes(2);
            Assert.AreEqual(2, twoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(1, twoBytes[0], "Unexpected result after 1st 2 bytes read.");
            Assert.AreEqual(2, twoBytes[1], "Unexpected result after 1st 2 bytes read.");

            twoBytes = reader.ReadNewBytes(2);
            Assert.AreEqual(2, twoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(3, twoBytes[0], "Unexpected result after 2nd 2 bytes read.");
            Assert.AreEqual(4, twoBytes[1], "Unexpected result after 2nd 2 bytes read.");

            twoBytes = reader.ReadNewBytes(2);
            Assert.AreEqual(1, twoBytes.Count(), "Wrong number of returned bytes.");
            Assert.AreEqual(5, twoBytes[0], "Unexpected result after 3rd 2 bytes read.");
        }

        [TestCase]
        public void ReadNewBytes_ContinuousReadingAfterNewItemsAdded()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader("dummyFileName") { myFileStream = mockedFileStream };

            var bytes = reader.ReadNewBytes();
            Assert.AreEqual(5, bytes.Count(), "Wrong number of returned bytes in the 1st round.");
            Assert.AreEqual(1, bytes[0], "Unexpected result in the 1st round.");
            Assert.AreEqual(5, bytes[4], "Unexpected result in the 1st round.");

            mockedFileStream.AddItems(new byte[] { 7, 8, 9 });
            bytes = reader.ReadNewBytes();
            Assert.AreEqual(3, bytes.Count(), "Wrong number of returned bytes in the 2nd round.");
            Assert.AreEqual(7, bytes[0], "Unexpected result in the 2nd round.");
            Assert.AreEqual(8, bytes[1], "Unexpected result in the 2nd round.");
            Assert.AreEqual(9, bytes[2], "Unexpected result in the 2nd round.");

        }

        [TestCase]
        public void ReadNewBytes_ContinuousReadingAfterSourceReset()
        {
            var mockedFileStream = new MockedFileStream(new byte[] { 1, 2, 3, 4, 5 });
            var reader = new FileReader("dummyFileName") { myFileStream = mockedFileStream };

            var bytes = reader.ReadNewBytes();
            Assert.AreEqual(5, bytes.Count(), "Wrong number of returned bytes in the 1st round.");
            Assert.AreEqual(1, bytes[0], "Unexpected result in the 1st round.");
            Assert.AreEqual(5, bytes[4], "Unexpected result in the 1st round.");

            mockedFileStream.Reset();
            mockedFileStream.AddItems(new byte[] { 7, 8, 9 });
            bytes = reader.ReadNewBytes();
            Assert.AreEqual(3, bytes.Count(), "Wrong number of returned bytes in the 2nd round.");
            Assert.AreEqual(7, bytes[0], "Unexpected result in the 2nd round.");
            Assert.AreEqual(8, bytes[1], "Unexpected result in the 2nd round.");
            Assert.AreEqual(9, bytes[2], "Unexpected result in the 2nd round.");
        
        }

    }

}
