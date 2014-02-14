using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CloudLinq.Base
{
    public interface ICloudContext : IDisposable
    {
        void Run    (ICloudQueryExpr    query);
        T    Run<T> (ICloudQueryExpr<T> query);
    }
}
