namespace CloudLinq.MBrace

open Nessos.MBrace
open Nessos.MBrace.Client
open CloudLinq.Core
open LinqOptimizer.Core
open Microsoft.FSharp.Quotations
open System.Reflection
open System
open System.Runtime.Serialization
open System.Collections
open System.Collections.Generic

module MBraceQueryCompiler =

    let private compile_aux(expr : CloudQueryExpr) =
        let e = CloudQueryExpr.toQueryExpr expr
        let f = LinqOptimizer.Core.CoreHelpers.Compile<'T>(e, fun e -> e)
        f.Invoke

    [<Cloud>]
    let private compilePackage<'T>(package : CloudQueryPackage) : ICloud<'T> =
        cloud { 
            let f = compile_aux(package.GetQuery())   
            return f() 
        }

    [<Cloud>]
    let compilePackageAsExpr<'T>(package : CloudQueryPackage) =
        <@ compilePackage<'T>(package)  @>