namespace CloudLinq.MBrace

open Nessos.MBrace
open Nessos.MBrace.Client
open CloudLinq.Core
open LinqOptimizer.Core
open Microsoft.FSharp.Quotations

module MBraceQueryCompiler =

    let initLocalRuntime(nodes : int) =
        MBrace.InitLocal(nodes)

    let private compile_aux(expr : CloudQueryExpr) =
        let e = CloudQueryExpr.toQueryExpr expr
        let f = LinqOptimizer.Core.CoreHelpers.Compile<'T>(e, fun e -> e)
        f.Invoke

    [<Cloud>]
    let compile<'T>(expr : CloudQueryExpr) : ICloud<'T> =
        cloud { 
            System.IO.File.WriteAllText(@"c:\users\konstantinos\desktop\1.txt", sprintf "Compiling CloudQueryExpr %A" expr)
            let f = compile_aux(expr)   
            System.IO.File.WriteAllText(@"c:\users\konstantinos\desktop\2.txt", sprintf "Compiled" )
            return f() 
            //return Unchecked.defaultof<'T>
        }

    [<Cloud>]
    let compileAsExpr<'T>(expr : CloudQueryExpr) =
        <@ compile<'T>(expr) @>