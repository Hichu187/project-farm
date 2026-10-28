using NUnit.Framework;
using VeQue.Logic;

namespace VeQue.Tests
{
    public class SmokeTests
    {
        [Test]
        public void LogicAssembly_IsReferenced()
        {
            Assert.AreEqual(LogicAssembly.Name, typeof(LogicAssembly).Assembly.GetName().Name);
        }
    }
}
