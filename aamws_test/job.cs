using System;
using System.Collections.Generic;
using System.Linq;
using aamcommon;
using aamws;
using NUnit.Framework;

namespace aamws_test
{
    [TestFixture]
    public class JobTests
    {
        private const string TestJobId = "x";

        [TestCase]
        public void InitialStatusIsNA()
        {
            var job = new Job(TestJobId);

            Assert.AreEqual(JobStatus.Na, job.Status, "Unexpected initial job.Status");
            Assert.AreEqual(JobStatus.Na.ToString(), job.Field(Field.Status), "Unexpected initial job.Field(Field.Status)");
        }

        [TestCase]
        public void EnabledStatusTransitionsGetThrough()
        {
            var job = new Job(TestJobId);

            foreach (var enabledStatusTransition in Job.EnabledStatusTransitions)
            {
                var fromState = enabledStatusTransition.Key;
                foreach (var toState in enabledStatusTransition.Value)
                {
                    job.Update(new Dictionary<Field, string>()
                    {
                        {Field.Status, fromState.ToString()},
                        {Field.Error, string.Empty}
                    });
                    Console.WriteLine("{0} => {1}", job.Field(Field.Status), toState);
                    job.Update(new Dictionary<Field, string>() { { Field.Status, toState.ToString() } });
                    Assert.AreEqual(toState.ToString(), job.Field(Field.Status), "Unexpected result of job.Field(Status) after a transition from {0} to {1}.", fromState, toState);
                    StringAssert.AreEqualIgnoringCase(string.Empty, job.Field(Field.Error), "Unexpected error after a transition from {0} to {1}.", fromState, toState);
                }
            }
        }

        [TestCase]
        public void UnexpectedStatusTransitionsEndUpInError()
        {
            var job = new Job(TestJobId);

            foreach (var f in Enum.GetValues(typeof(JobStatus)))
            {
                var fromState = (JobStatus)f;
                foreach (var t in Enum.GetValues(typeof(JobStatus)))
                {
                    var toState = (JobStatus)t;
                    if (fromState == toState ||
                        !Job.EnabledStatusTransitions.ContainsKey(fromState) ||
                        Job.EnabledStatusTransitions[fromState].Any(s => s == toState))
                    {
                        continue;
                    }
                    job.Update(new Dictionary<Field, string>()
                    {
                        {Field.Status, fromState.ToString()},
                        {Field.Error, string.Empty}
                    });
                    Console.WriteLine("{0} => {1}", job.Field(Field.Status), toState);
                    job.Update(new Dictionary<Field, string>() { { Field.Status, toState.ToString() } });
                    StringAssert.AreNotEqualIgnoringCase(string.Empty, job.Field(Field.Error), "No error after a transition from {0} to {1}.", fromState, toState);
                }
            }
        }

    }
}
