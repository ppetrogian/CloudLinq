using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLinq.MBrace.CSharp.Tests.AssemblyDependencyTest
{
    public class MyType
    {
        public static double Binomial(int n, int k)
        {
            return MathNet.Numerics.SpecialFunctions.Binomial(n, k);
        }

        public static int Abs(int n)
        {
            return Math.Abs(n);
        }
    }
}
