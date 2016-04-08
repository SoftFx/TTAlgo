using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
