using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CloudLinq.Core;
using CloudLinq.Base;
using Nessos.MBrace.Client;
using Microsoft.FSharp.Core;

namespace CloudLinq.MBrace.CSharp
{
    public class MBraceCloudContext : ICloudContext
    {
        protected MBraceRuntime runtime;

        public MBraceCloudContext(MBraceRuntime runtime)
        {
            this.runtime = runtime;
        }

        public void Run(ICloudQueryExpr query)
        {
            var package = new CloudQueryPackage(query.Expr);
            var expr = MBraceQueryCompiler.compilePackageAsExpr<Unit>(package);
            var _ = runtime.Run<Unit>(expr, FSharpOption<string>.None);
        }

        public T Run<T>(ICloudQueryExpr<T> query)
        {
            var package = new CloudQueryPackage(query.Expr);
            var expr = MBraceQueryCompiler.compilePackageAsExpr<T>(package);
            return runtime.Run<T>(expr, FSharpOption<string>.None);
        }

        public virtual void Dispose()
        {
            runtime.Shutdown();
        }
    }

    public class MBraceLocalCloudContext : MBraceCloudContext
    {
        public MBraceLocalCloudContext(int nodes)
            : base(MBrace.InitLocal(nodes))
        {
            
        }

        public override void Dispose()
        {
            base.runtime.Kill();
        }
    }
}
