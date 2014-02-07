
#r @"..\..\lib\Nessos.MBrace.dll"
#r @"..\..\lib\Nessos.MBrace.Actors.dll"
#r @"..\..\lib\Nessos.MBrace.Client.dll"
#r @"..\..\lib\Nessos.MBrace.Common.dll"
#r @"..\..\lib\Nessos.MBrace.Core.dll"
#r @"..\..\lib\Nessos.MBrace.Store.dll"
#r @"..\..\lib\Nessos.MBrace.Utils.dll"

#r @"..\..\packages\LinqOptimizer.FSharp.0.5.2\lib\LinqOptimizer.Base.dll"
#r @"..\..\packages\LinqOptimizer.FSharp.0.5.2\lib\LinqOptimizer.Core.dll"
#r @"..\..\packages\LinqOptimizer.FSharp.0.5.2\lib\LinqOptimizer.FSharp.dll"
#r @"..\..\packages\Serialize.Linq.1.1.4\lib\net45\Serialize.Linq.dll"
#r @".\bin\Debug\CloudLinq.Core.dll"

open CloudLinq.Core
open Nessos.MBrace
open Nessos.MBrace.Client
open LinqOptimizer.FSharp
open LinqOptimizer.Base
open LinqOptimizer.Core

open System
open System.Linq.Expressions
open System.Runtime.Serialization

let rt = MBrace.InitLocal 4

let i = Query.range(1,10) 
        |> Query.map(fun x -> x * x)

let cq = CloudQueryExpr.ofQueryExpr(i.Expr)

[<Cloud>]
let ic () = CloudQueryExpr.compile<int seq>(cq)

rt.Run <@ ic () @>

rt.Run <@ cloud { return "Hi" } @>