namespace CloudLinq.MBrace

open Nessos.MBrace
open Nessos.MBrace.Client
open CloudLinq.Core
open LinqOptimizer.Core
open Microsoft.FSharp.Quotations

module MBraceCompiler =

    let initLocalRuntime(nodes : int) =
        MBrace.InitLocal(nodes)

    let private compile_aux(expr : CloudQueryExpr) =
        let e = CloudQueryExpr.toQueryExpr expr
        let f = LinqOptimizer.Core.CoreHelpers.Compile<'T>(e, fun e -> e)
        f.Invoke

    [<Cloud>]
    let compile<'T>(expr : CloudQueryExpr) : Expr<ICloud<'T>> =
        <@ cloud { 
            let f = compile_aux(expr)   
            return f() 
            //return Unchecked.defaultof<'T>
        } @>