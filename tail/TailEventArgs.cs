using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tail
{
    class TailEventArgs
    {
        public List<string> NewLines { get; private set; }

        public TailEventArgs(List<string> newLines)
        {
            NewLines = newLines;
        }

    }
}
