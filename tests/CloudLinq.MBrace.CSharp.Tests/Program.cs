﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using Nessos.MBrace;
using Nessos.MBrace.Client;
using System.Runtime.Serialization;
using Serialize.Linq.Serializers;
using CloudLinq.MBrace.CSharp;
using System.IO;
using CloudLinq.MBrace.CSharp.Tests.AssemblyDependencyTest;

namespace CloudLinq.MBrace.CSharp.Tests
{
    public class Program
    {
        static void Main(string[] args)
        {
            MBraceSettings.MBracedExecutablePath = @"C:\dev\github-repositories\CloudLinq\lib\mbraced.exe";
            MBraceSettings.StoreProvider = StoreProvider.LocalFS;
            var rt = MBrace.InitLocal(3);

            var q1 = Extensions.Range(1, 10)
                     .Select(x => -MyType.Abs(x));

            var r1 = q1.Run(rt);

            var q2 = Extensions.Range(1, 10)
                     .Select(x => MyType.Binomial(x,2));

            var r2 = q2.Run(rt);
        }
    }
}
