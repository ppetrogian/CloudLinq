using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq.Expressions;
using CloudLinq.MBrace;
using CloudLinq.Base;
using LinqOptimizer.Core;
using Nessos.MBrace.Client;
using CloudLinq.Core;
using Microsoft.FSharp.Core;

namespace CloudLinq.MBrace.CSharp
{
    /// <summary>
    /// Provides a set of static methods for querying objects that implement IGpuQueryExpr.
    /// </summary>
    public static class Extensions
    {
        public static MBraceRuntime InitLocal(int nodes, string path)
        {
            MBraceSettings.MBracedExecutablePath = path;
            MBraceSettings.StoreProvider = StoreProvider.LocalFS;

            return MBraceRuntime.InitLocal(nodes, 
                        FSharpOption<string>.None,
                        FSharpOption<StoreProvider>.None,
                        FSharpOption<bool>.None,
                        FSharpOption<bool>.None);
        }

        public static T RunInDaCloud<T>(this ICloudQueryExpr<T> query, MBraceRuntime runtime)
        {
            var expr = MBraceQueryCompiler.compileAsExpr<T>(query.Expr);
            return runtime.Run<T>(expr, FSharpOption<string>.None);
        }
        

        /// <summary>
        /// Enables a gpu query.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements in the source array.</typeparam>
        /// <param name="source">An array to convert to an IGpuQueryExpr.</param>
        /// <returns>A query that returns the elements of the source array.</returns>
        public static ICloudQueryExpr<IEnumerable<TSource>> AsCloudQueryExpr<TSource>(this IEnumerable<TSource> source)
        {
            //return new CloudQueryExpr<IEnumerable<TSource>>(QueryExpr.NewSource(Expression.Constant(source), typeof(TSource), QueryExprType.Sequential));
            var cq = CloudQueryExpr.NewSource(Expression.Constant(source).AsSerializable(), typeof(TSource), QueryExprType.Sequential);
            return new CloudQueryExpr<IEnumerable<TSource>>(cq);
        }

        public static ICloudQueryExpr<IEnumerable<int>> Range(int start, int count)
        {
            var cq = CloudQueryExpr.NewRangeGenerator(Expression.Constant(start).AsSerializable(), Expression.Constant(count).AsSerializable());
            return new CloudQueryExpr<IEnumerable<int>>(cq);
        }

        #region Combinators
        /// <summary>
        /// Creates a new query that projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the query.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by selector.</typeparam>
        /// <param name="query">A query whose values to invoke a transform function on.</param>
        /// <param name="selector">A transform function to apply to each element.</param>
        /// <returns>A query whose elements will be the result of invoking the transform function on each element of source.</returns>
        public static ICloudQueryExpr<IEnumerable<TResult>> Select<TSource, TResult>(this ICloudQueryExpr<IEnumerable<TSource>> query, Expression<Func<TSource, TResult>> selector)
        {
            var cq = CloudQueryExpr.NewTransform(selector.AsSerializable(), query.Expr);
            return new CloudQueryExpr<IEnumerable<TResult>>(cq);
        }

        /// <summary>
        /// Creates a new query that filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of source.</typeparam>
        /// <param name="query">An query whose values to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>A query that contains elements from the input query that satisfy the condition.</returns>
        public static ICloudQueryExpr<IEnumerable<TSource>> Where<TSource>(this ICloudQueryExpr<IEnumerable<TSource>> query, Expression<Func<TSource, bool>> predicate)
        {
            var cq = CloudQueryExpr.NewFilter(predicate.AsSerializable() , query.Expr);
            return new CloudQueryExpr<IEnumerable<TSource>>(cq);
        }
        #endregion
    }

}
