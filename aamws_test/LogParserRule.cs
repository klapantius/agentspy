using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using aamcommon;
using aamws;
using NUnit.Framework;

namespace aamws_test
{
    [TestFixture]
    class LogParserRuleTests
    {
        [TestCase]
        public void CanReturnMatchingField()
        {
            const string testRule = "(?<TC>.*)";
            const string testString = "DontThinkTooMuchAboutTheName";

            var parser = new LogParserRule(testRule);

            var result = parser.Parse(testString);

            Assert.IsTrue(result.ContainsKey(Field.TC), "TC could not be parsed. (Current result: {0})", string.Join(", ", result.Keys));
            StringAssert.AreEqualIgnoringCase(testString, result[Field.TC], "Wrong parsing.");
        }

        [TestCase]
        public void CanReturnMatchingFields()
        {
            const string testRule = @"(?<Assembly>\w*)(?<Build>.*)";
            var expected = new Dictionary<Field, string>()
            {
                {Field.Assembly, "DontThinkTooMuchAboutTheAssembly"},
                {Field.Build, "DontThinkTooMuchAboutTheBuild"}
            };

            var parser = new LogParserRule(testRule);

            var result = parser.Parse(string.Join(" ", expected.Select(i => i.Value)));

            foreach (var field in new[] { Field.Assembly, Field.Build })
            {
                Assert.IsTrue(result.ContainsKey(field), "{0} could not be parsed. (Current result: {1})", field, string.Join(", ", result.Keys));
                StringAssert.Contains(field.ToString(), result[field], "Wrong parsing.");
            }
        }

    }
}
