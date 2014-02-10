namespace CloudLinq.Core

open LinqOptimizer.FSharp
open LinqOptimizer.Base
open LinqOptimizer.Core

open System
open System.IO
open System.Reflection
open System.Linq.Expressions
open System.Runtime.Serialization

open Serialize.Linq
open Serialize.Linq.Interfaces
open Serialize.Linq.Extensions
open Serialize.Linq.Nodes
open Serialize.Linq.Serializers

[<Serializable>]
type SerializableExpression(expr : Expression, raw : byte []) =
    
    member this.Expression = expr

    interface ISerializable with
        override this.GetObjectData(info : SerializationInfo, context : StreamingContext) =
            let es = new ExpressionSerializer(new BinarySerializer())
            let bin = es.SerializeBinary(expr)
            info.AddValue("bin", bin, typeof<byte []>)
            info.AddValue("raw", raw, typeof<byte []>)

    new(info : SerializationInfo, context : StreamingContext) =
        let es = new ExpressionSerializer(new BinarySerializer())
        let bin = info.GetValue("bin", typeof<byte []>) :?> byte []
        let raw = info.GetValue("raw", typeof<byte []>) :?> byte []
        Assembly.Load(raw) |> ignore
        let expr = es.DeserializeBinary(bin)
        SerializableExpression(expr, raw)

[<System.Runtime.CompilerServices.Extension>]
type ExpressionExtensions =
    [<System.Runtime.CompilerServices.Extension>]
    static member AsSerializable(expr : Expression) = 
        let loc = Assembly.GetEntryAssembly().Location
        let asm = File.ReadAllBytes(loc)
        new SerializableExpression(expr, asm)