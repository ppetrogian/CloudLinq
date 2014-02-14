using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FSharp.Core;
using Nessos.MBrace.Client;
using System.Reflection;
using Runtime = Nessos.MBrace.Client.MBraceRuntime;
using System.Linq.Expressions;
using CloudLinq.Core;

namespace CloudLinq.MBrace.CSharp
{
    internal static class Helpers
    {
        internal static FSharpOption<T> Convert<T>(T value)
        {
            var def = default(T);
            if (EqualityComparer<T>.Default.Equals(value, def))
                return FSharpOption<T>.None;
            else
                return FSharpOption<T>.Some(value);
        }
    }

    public static class MBrace
    {
        public static Runtime InitLocal(int totalNodes, 
                                        string hostname = default(string),
                                        StoreProvider storeProvider = default(StoreProvider),
                                        bool debug = default(bool),
                                        bool background = default(bool))
        {
            var hostnameOpt         = Helpers.Convert<string>(hostname);
            var storeProviderOpt    = Helpers.Convert<StoreProvider>(storeProvider);
            var debugOpt            = Helpers.Convert<bool>(debug);
            var backgroundOpt       = Helpers.Convert<bool>(background);

            return Runtime.InitLocal(totalNodes,
                                     hostnameOpt,
                                     storeProviderOpt,
                                     debugOpt,
                                     backgroundOpt);
        }
    }
}
