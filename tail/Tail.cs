using System;
using System.Collections.Generic;
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

    }

}
