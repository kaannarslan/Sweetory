using NUnit.Framework;

namespace Sweetory.Core.Tests
{
    // Test assembly'sinin Core'u görebildiğini doğrulayan geçici duman testi.
    public class CoreInfoTests
    {
        [Test]
        public void Name_IsSweetoryCore()
        {
            Assert.AreEqual("Sweetory.Core", CoreInfo.Name);
        }
    }
}
