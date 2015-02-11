using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace tail
{
    public class Tail : ITail
    {
        internal IFileReader myFileReader;
        internal Encoding myFileType = Encoding.Default;
        internal bool encodingDetected = false;

        public Tail(string fileName, Encoding fileType)
        {
            if (null == myFileReader) myFileReader = new FileReader(fileName);
            if (!Equals(fileType, Encoding.Default))
            {
                myFileType = fileType;
            }
            else try
            {
                DetectEncoding();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                myFileType = Encoding.ASCII;
            }
        }

        private void DetectEncoding()
        {
            var b = myFileReader.ReadNewBytes(2);
            if (b == null || b.Length < 2)
            {
                myFileReader.ResetPosition();
                return;
            }

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
            encodingDetected = true;
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
        public event ChangedEventHandler Changed;

        protected virtual void OnChanged(TailEventArgs e)
        {
            if (Changed != null)
                Changed(this, e);
        }

        #endregion

        #region watching
        private FileSystemWatcher fsWatcher;

        public void StopWatching()
        {
            if (null != fsWatcher) fsWatcher.EnableRaisingEvents = false;
        }

        public void Watch()
        {
            if (null != fsWatcher && fsWatcher.EnableRaisingEvents) return;

            var dir = Path.GetDirectoryName(myFileReader.FileName);
            if (string.IsNullOrEmpty(dir)) dir = Environment.CurrentDirectory;
            fsWatcher = new FileSystemWatcher(dir, Path.GetFileName(myFileReader.FileName));
            fsWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.LastAccess | NotifyFilters.CreationTime | NotifyFilters.Size;
            fsWatcher.Changed += fsWatcher_Changed;
            fsWatcher.Created += fsWatcher_Changed;
            fsWatcher.Deleted += fsWatcher_Changed;
            fsWatcher.Renamed += fsWatcher_Changed;
            fsWatcher.EnableRaisingEvents = true;
        }

        void fsWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed && e.ChangeType != WatcherChangeTypes.Created) return;
            if (!encodingDetected)
            {
                DetectEncoding();
                if (!encodingDetected) return;
            }

            var newLines = GetNewLines();
            if (newLines.Count > 0) OnChanged(new TailEventArgs(newLines));
        }

        #endregion

    }

}