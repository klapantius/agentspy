using aamcommon;
using NUnit.Framework;
using System.Collections.Generic;

namespace aamcommon_test
{
    [TestFixture]
    public class TestAgentStatusRecordTests
    {
        private const string TestJobId = "x";
        private const string TestBuild = "test_build";
        private const string TestAssembly = "test.assembly.dll";

        [TestCase]
        public void InitialStatusIsNaAndFieldsAreEmpty()
        {
            var r = new TestAgentStatusRecord();

            Assert.AreEqual(Status.NA.ToString(), r[Field.Status], "Unexpected initial status.");
            Assert.IsTrue(string.IsNullOrEmpty(r[Field.Build]), "Build should be empty.");
        }

        [TestCase]
        public void UpdateOfStatusFieldResultsInChangeOfStatusString()
        {
            var r = new TestAgentStatusRecord();
            var expectedContent = ExpectedContent(Field.Status, Status.Offline.ToString());
            r.Update(TestJobId, new Dictionary<Field, string>()
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

        //[TestCase]
        //public void TransitionSetupToTestExecutionTakesBuildAndAssemblyInfo()
        //{
        //    var r = new TestAgentStatusRecord();
        //    r.Update(TestJobId, new Dictionary<Field, string>()
        //    {
        //        {Field.Status, Status.Online.ToString()},
        //        {Field.Build, TestBuild},
        //        {Field.Assembly, TestAssembly}
        //    });
        //}

        private static string ExpectedContent(Field field, string value)
        {
            return string.Format("{0}{1}{2}", field.ToString(), TestAgentStatusRecord.FieldValueSeparator, value);
        }
    }
}
