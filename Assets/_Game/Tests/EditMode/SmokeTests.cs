using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class SmokeTests
    {
        [Test]
        public void LogicAssembly_IsReferenced()
        {
            Assert.AreEqual("VeQue.Logic", typeof(Farm).Assembly.GetName().Name);
        }
    }
}
