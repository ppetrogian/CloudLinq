﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nessos.MBrace;
using Nessos.MBrace.Client;

namespace CloudLinq.MBrace.CSharp.Tests
{
    class Program
    {
        static void Main(string[] args)
        {
            var rt = Extensions.InitLocal(8, @"C:\data\repositories\CloudLinq\lib\mbraced.exe");

            var q = Enumerable.Range(1, 10).AsCloudQueryExpr().Select(x => x * x);

            var r = q.RunInDaCloud(rt);
        }
    }
}
