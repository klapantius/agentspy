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

            Assert.AreEqual(Status.NA.ToString(),r[Field.Status], "Unexpected initial status");
        }

        [TestCase]
        public void UpdateOfStatusFieldResultsInChangeOfStatusString()
        {
            var r = new TestAgentStatusRecord();
            var expectedContent = ExpectedContent(Field.Status, Status.Offline.ToString());
            r.Update("x", new Dictionary<Field, string>()
            {
                {Field.Status, Status.Offline.ToString()}
            });
            StringAssert.Contains(expectedContent, r.StatusString, "Wrong or missing status in StatusString.");
        }

        [TestCase]
        public void UpdateOfStatusStringResultsInChangeOfStatusField()
        {
            var r = new TestAgentStatusRecord()
            {
                StatusString = ExpectedContent(Field.Status, Status.Offline.ToString())
            };
            StringAssert.AreEqualIgnoringCase(Status.Offline.ToString(), r[Field.Status], "Unexpected status filed value after StatusString changed.");
        }

        private string ExpectedContent(Field field, string value)
        {
            return string.Format("{0}{1}{2}", field.ToString(), TestAgentStatusRecord.FieldValueSeparator, value);
        }
    }
}
