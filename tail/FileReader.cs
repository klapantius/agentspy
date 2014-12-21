using System;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tail_uTest")]

namespace tail
{
    class FileReader : IFileReader
    {
        public string FileName { get; private set; }
        private long position;
        internal IMockableFileStream myFileStream;

        public FileReader(string fileName)
        {
            FileName = fileName;
            position = 0;
        }

        public byte[] ReadLastNBytes(long n)
        {
            using (var fs = myFileStream ?? new MockableFileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var numberOfBytesToDisplay = n < fs.Length ? n : fs.Length;

                byte[] buf = new byte[numberOfBytesToDisplay];

                fs.Seek(-numberOfBytesToDisplay, SeekOrigin.End);
                fs.Read(buf, 0, (int)numberOfBytesToDisplay);

                fs.Close();

                return buf;
            }
        }

        public byte[] ReadNewBytes(long max = -1)
        {
            using (var fs = myFileStream ?? new MockableFileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                // compare the size of the file to the last position
                // and return an empty array if nothing has changed since the last read
                if (fs.Length == position) return new byte[] { };

                long bytesToRead;
                // decide the start position and the number of bytes to read:
                // if no bytes read yet get the minimum of file size vs max enabled (if any)
                if (position == 0 || fs.Length < position) bytesToRead = GetMinOrFirst(fs.Length, max);
                // else get the minimum of difference between file size and last position vs max enabled (if any)
                else bytesToRead = GetMinOrFirst(fs.Length - position, max);

                // reset file pointer if the file has been reset since the last read
                // (assumed the file is still smaller than the last read position before reset / assumed it was at the end)
                if (fs.Length < position) position = 0;

                // read now
                var buf = new byte[bytesToRead];
                fs.Seek(position, SeekOrigin.Begin);
                fs.Read(buf, 0, (int)bytesToRead);

                // notice the new position
                position = fs.Position;

                fs.Close();

                return buf;
            }
        }

        public void ResetPosition()
        {
            position = 0;
        }

        /// <summary>
        /// selects the minimum of two values or the first one if the second one is lower than 1
        /// </summary>
        private static long GetMinOrFirst(long a, long b)
        {
            return Math.Min(a, b > 0 ? b : a);
        }

    }
}
