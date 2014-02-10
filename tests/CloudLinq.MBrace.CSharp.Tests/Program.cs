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
        static int sqr(int x)
        {
            return x * x;
        }

        static void Main(string[] args)
        {
            var rt = Extensions.InitLocal(3, @"C:\dev\github-repositories\CloudLinq\lib\mbraced.exe");

            var q = Extensions.Range(1,10).Select(x => sqr(x));

            var r = q.Run(rt);
        }
    }
}
