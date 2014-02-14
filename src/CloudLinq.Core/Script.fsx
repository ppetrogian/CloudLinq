
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
#r @"..\..\packages\FsPickler.0.8.5.1\lib\net45\FsPickler.dll"
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

open Serialize.Linq
open Serialize.Linq.Interfaces
open Serialize.Linq.Extensions
open Serialize.Linq.Nodes
open Serialize.Linq.Serializers

open FsPickler

let rt = MBrace.InitLocal 4

type Helper =
    static member ofFunc(f : Expression<Func<'T,'R>>) = f

let f x = x + 1
type Foo =
    static member F(x : int) = x * x

let i = Query.range(1,10) 
        |> Query.map(fun x -> f x)

let cq = CloudQueryExpr.ofQueryExpr(i.Expr)
let e = Helper.ofFunc(fun x -> Foo.F x)
let es = new ExpressionSerializer(new BinarySerializer())
let bin = es.SerializeBinary(e)
let u = es.DeserializeBinary(bin)


[<Cloud>]
let ic () = CloudQueryExpr.compile<int seq>(cq)

rt.Run <@ ic () @>

rt.Run <@ cloud { return "Hi" } @>

let cq = CloudQueryExpr.RangeGenerator(new SerializableExpression(Expression.Constant(1)), new SerializableExpression(Expression.Constant(10)))
let pkg = new CloudQueryPackage(cq)
pkg.GetQuery()

let fsp = new FsPickler()
let ms = new IO.MemoryStream()
fsp.Serialize<CloudQueryPackage>(ms, pkg)
ms.Position <- 0L

let pkg' = fsp.Deserialize<CloudQueryPackage>(ms) 
