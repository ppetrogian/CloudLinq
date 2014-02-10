using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CloudLinq.Core;

namespace CloudLinq.Base
{
    /// <summary>
    ///  This interface represents an optimized query.
    /// </summary>
    public interface ICloudQueryExpr
    {
        /// <summary>
        /// The expression representing the query.
        /// </summary>
        CloudQueryExpr Expr { get; }
    }

    /// <summary>
    /// This interface represents an optimized query.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    public interface ICloudQueryExpr<out TQuery> : ICloudQueryExpr { }


    /// <summary>
    /// The concrete implementation of an optimized query.
    /// </summary>
    /// <typeparam name="T">The type of the query.</typeparam>
    public class CloudQueryExpr<T> : ICloudQueryExpr<T>
    {
        private CloudQueryExpr _expr;
        /// <summary>
        /// The expression representing the query.
        /// </summary>
        public CloudQueryExpr Expr { get { return _expr; } }

        /// <summary>
        /// Initializes a new instance of the <see cref="CloudQueryExpr{T}"/> class.
        /// </summary>
        /// <param name="query">The expression.</param>
        public CloudQueryExpr(CloudQueryExpr query)
        {
            _expr = query;
        }
    }
}
