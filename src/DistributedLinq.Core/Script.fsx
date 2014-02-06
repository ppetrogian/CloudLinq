// Learn more about F# at http://fsharp.net. See the 'F# Tutorial' project
// for more guidance on F# programming.


#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.dll"
#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.Actors.dll"
#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.Client.dll"
#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.Common.dll"
#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.Core.dll"
#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.Store.dll"
#r @"C:\dev\github-repositories\DistributedLinq\lib\Nessos.MBrace.Utils.dll"

#r @"C:\dev\github-repositories\DistributedLinq\packages\LinqOptimizer.FSharp.0.5.2\lib\LinqOptimizer.Base.dll"
#r @"C:\dev\github-repositories\DistributedLinq\packages\LinqOptimizer.FSharp.0.5.2\lib\LinqOptimizer.Core.dll"
#r @"C:\dev\github-repositories\DistributedLinq\packages\LinqOptimizer.FSharp.0.5.2\lib\LinqOptimizer.FSharp.dll"

#load "./Library1.fs"
open DistributedLinq.Core

// Define your library scripting code here

open Nessos.MBrace
open Nessos.MBrace.Client
open LinqOptimizer.FSharp
open LinqOptimizer.Base
open LinqOptimizer.Core

open System
open System.Linq.Expressions
open System.Runtime.Serialization


[<Serializable>]
type CloudQueryExpr<'T>(expr : IQueryExpr<'T>) =
    
    interface ISerializable with
        override this.GetObjectData(info : SerializationInfo, context : StreamingContext) =
            
            ()

    new(info : SerializationInfo, context : StreamingContext) =
        CloudQueryExpr<_>(null)

let rt = MBrace.InitLocal 2

let q1 = Query.range(1,10) |> Query.map(fun x -> x * x)
//let q2 = CloudQueryExpr(q1)
let e = Expression.Constant(42)

rt.Run <@ cloud { return e } @>


//
//[<CustomPickler>]
//type Wrapper(expr : QueryExpr) =
//    
//    static 
//


#r @"C:\dev\github-repositories\DistributedLinq\packages\Serialize.Linq.1.1.4\lib\net45\Serialize.Linq.dll"

open Serialize.Linq
open Serialize.Linq.Interfaces
open Serialize.Linq.Extensions
open Serialize.Linq.Nodes
open Serialize.Linq.Serializers

type Helper =
    static member getExpr(func : Expression<Func<'T>>) = func
    static member getExpr(func : Expression<Func<'T,'R>>) = func
    static member getExpr(func : Expression<Func<'T,'R,'U>>) = func

let es = new ExpressionSerializer(new BinarySerializer())

let x = 
    let v = Expression.Parameter(typeof<int>, "v")
    Expression.Lambda(v, v)
let y = Helper.getExpr(fun (x : int) -> Some 42)
type Foo = Foo of int
let z = Helper.getExpr(fun l -> Foo l)
let p = Helper.getExpr(fun l -> Seq.take l [1..10])
let r = Helper.getExpr(fun l -> Seq.skip 42 (Seq.take l [1..10]) )

let b = es.SerializeBinary(r)
es.DeserializeBinary(b)


[<Serializable>]
type SerializableExpression(expr : Expression) =
    
    member this.Expression = expr

    member this.Invoke<'T,'R> (arg) = 
        (expr :?> Expression<Func<'T, 'R>>).Compile().Invoke(arg)

    interface ISerializable with
        override this.GetObjectData(info : SerializationInfo, context : StreamingContext) =
            let es = new ExpressionSerializer(new BinarySerializer())
            let bin = es.SerializeBinary(expr)
            info.AddValue("bin", bin, typeof<byte []>)

    new(info : SerializationInfo, context : StreamingContext) =
        let es = new ExpressionSerializer(new BinarySerializer())
        let bin = info.GetValue("bin", typeof<byte []>) :?> byte []
        let expr = es.DeserializeBinary(bin)
        SerializableExpression(expr)

let serializable e = new SerializableExpression(e)

type CloudQueryExpr =
    | Select of SerializableExpression * CloudQueryExpr
    | Where of SerializableExpression * CloudQueryExpr
    | Range of SerializableExpression * SerializableExpression 
    | Length of CloudQueryExpr

    static member ofQueryExpr(expr : QueryExpr) =
        match expr with
        | Transform(l,q)      -> Select(serializable l, CloudQueryExpr.ofQueryExpr q)
        | Filter(l,q)         -> Where(serializable l, CloudQueryExpr.ofQueryExpr q)
        | RangeGenerator(l,r) -> Range(serializable l, serializable r)
        | Count(q)            -> Length(CloudQueryExpr.ofQueryExpr q)
        | _                   -> failwithf "Unsupported %A" expr

    static member toQueryExpr(expr : CloudQueryExpr) =
        match expr with
        | Select(expr, cexpr) -> Transform(expr.Expression :?> LambdaExpression, CloudQueryExpr.toQueryExpr cexpr)
        | Where(expr, cexpr)  -> Filter(expr.Expression :?> LambdaExpression, CloudQueryExpr.toQueryExpr cexpr)
        | Range(lexpr, rexpr) -> RangeGenerator(lexpr.Expression , rexpr.Expression)
        | Length(cexpr)       -> Count(CloudQueryExpr.toQueryExpr cexpr)

    static member compile<'T>(expr : CloudQueryExpr) =
        let e = CloudQueryExpr.toQueryExpr expr
        let f = LinqOptimizer.Core.CoreHelpers.Compile<'T>(e, fun e -> e)
        f.Invoke()

[<Cloud>]
let compile<'T>(expr : CloudQueryExpr) =
    cloud {
        return CloudQueryExpr.compile<'T>(expr)
    }

let i = Query.range(1,10) 
        |> Query.map(fun x -> Foo x) 
//        |> Query.filter(fun _ -> true)
        |> Query.map(fun (Foo x) -> x)


let i' = CloudQueryExpr.ofQueryExpr(i.Expr)

rt.Run <@ compile<int seq> i' @>
