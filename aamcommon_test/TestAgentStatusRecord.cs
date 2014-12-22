using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aamcommon;
using NUnit.Framework;

namespace aamcommon_test
{
    [TestFixture]
    public class TestAgentStatusRecordTests
    {
        [TestCase]
        public void InitialStatusIsNA()
        {
            var r = new TestAgentStatusRecord();

            Assert.AreEqual(TestAgentStatusRecord.Status.NA.ToString(),r.myFields[TestAgentStatusRecord.Field.Status], "Unexpected initial status");
        }
    }
}
