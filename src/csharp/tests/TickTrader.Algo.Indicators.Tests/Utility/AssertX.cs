namespace Microsoft.VisualStudio.TestTools.UnitTesting
{
    internal static class AssertX
    {
        public static void Greater(double expected, double real)
        {
            Assert.IsTrue(expected > real, expected + " must be greater than " + real);
        }

        public static void Greater(double expected, double real, string message)
        {
            Assert.IsTrue(expected > real, message);
            //if (expected > real)
            //    return;
            //System.Console.WriteLine(message);
        }
    }
}
