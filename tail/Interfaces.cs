using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tail_uTest")]

namespace tail
{
    interface ITail
    {
        List<string> GetNewLines();
    }

    interface IFileReader
    {
        string FileName { get; }
        byte[] ReadLastNBytes(long n);
        byte[] ReadNewBytes(long max = -1);
        void ResetPosition();
    }

    interface IMockableFileStream : IDisposable
    {
        long Length { get; }
        long Seek(long offset, SeekOrigin origin);
        int Read(byte[] array, int offset, int count);
        long Position { get; }
        void Close();
    }

}
