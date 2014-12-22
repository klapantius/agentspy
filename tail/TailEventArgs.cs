using System.Collections.Generic;

namespace tail
{
    public class TailEventArgs
    {
        public List<string> NewLines { get; private set; }

        public TailEventArgs(List<string> newLines)
        {
            NewLines = newLines;
        }

    }
}
