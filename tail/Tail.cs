using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tail
{
    class Tail : ITail
    {
        internal IFileReader myFileReader;
        internal Encoding myFileType = Encoding.Default;

        public Tail(string fileName, Encoding fileType)
        {
            if (null == myFileReader) myFileReader = new FileReader(fileName);
            if (!Equals(fileType, Encoding.Default))
            {
                myFileType = fileType;
            }
            else try
                {
                    var b = myFileReader.ReadNewBytes(2);

                    if (b[0] == 255 && b[1] == 254)
                    {
                        myFileType = Encoding.Unicode;
                    }
                    else if (b[0] == 239 && b[1] == 187)
                    {
                        myFileType = Encoding.UTF8;
                    }
                    else if (b[0] == 254 && b[1] == 255)
                    {
                        myFileType = Encoding.BigEndianUnicode;
                    }
                    else
                    {
                        myFileType = Encoding.ASCII;
                        myFileReader.ResetPosition();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    myFileType = Encoding.ASCII;
                }
        }

        public List<string> GetNewLines()
        {
            var buf = myFileReader.ReadNewBytes();
            if (buf == null || buf.Length == 0) return new List<string>();
            buf = Encoding.Convert(myFileType, Encoding.ASCII, buf);
            var sbOutstring = new StringBuilder();
            foreach (var b in buf)
            {
                sbOutstring.Append(Convert.ToChar(b));
            }
            return sbOutstring.ToString().Split(Environment.NewLine.ToCharArray()).Where(line => line.Trim().Length > 0).ToList();
        }

        #region changed event
        public delegate void ChangedEventHandler(object sender, TailEventArgs e);
        public event ChangedEventHandler Changed;

        protected virtual void OnChanged(TailEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        #endregion

        #region watching
        private bool iAmRunning;
        private FileSystemWatcher fsWatcher;

        public void StopWatching()
        {
            iAmRunning = false;

            fsWatcher.Changed -= fsWatcher_Changed;
        }

        public void Watch()
        {
            iAmRunning = true;

            if (null != fsWatcher || iAmRunning) return;

            fsWatcher = new FileSystemWatcher(Path.GetDirectoryName(myFileReader.FileName), Path.GetFileName(myFileReader.FileName));
            fsWatcher.Changed += fsWatcher_Changed;
        }

        void fsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed || e.ChangeType != WatcherChangeTypes.Created) return;

            var newLines = GetNewLines();
            if (newLines.Count > 0) OnChanged(new TailEventArgs(newLines));
        }

        #endregion

    }

}