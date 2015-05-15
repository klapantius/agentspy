using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aamcommon;

namespace AgentMonitorLocal_prototype
{
    class Observer
    {
        private List<TestAgentStatusRecord> Records = new List<TestAgentStatusRecord>();
        private HttpInterface myHttpInterface = new HttpInterface();

        public Observer()
        {
        }

    }
}
