namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    internal static class AssertX
    {
        public static void Greater(double expected, double real)
        {
            Assert.IsTrue(expected > real, expected + " must be greater than " + real);
        }
    }
}
