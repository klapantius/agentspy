using System.IO;

namespace agentspy.net
{
    class LocalLogReader: ILogReader
    {
        private readonly StreamReader myStreamReader;

        public LocalLogReader(string path)
        {
            var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            this.myStreamReader = new StreamReader(fileStream);
        }

        public string ReadLine()
        {
            return this.myStreamReader != null ? this.myStreamReader.ReadLine() : string.Empty;
        }

        public bool EndOfStream { get { return this.myStreamReader == null || this.myStreamReader.EndOfStream; } }

        public void Dispose()
        {
            if (myStreamReader != null) myStreamReader.Dispose();
        }
    
    }
}
