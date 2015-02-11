using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("tail_uTest")]

namespace tail
{
    public delegate void ChangedEventHandler(object sender, TailEventArgs e);
    
    public interface ITail
    {
        event ChangedEventHandler Changed;
        List<string> GetNewLines();
        void Watch();
        void StopWatching();
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
