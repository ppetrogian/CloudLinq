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
type SerializableExpression(expr : Expression) =
    
    member this.Expression = expr

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

[<System.Runtime.CompilerServices.Extension>]
type ExpressionExtensions =
    [<System.Runtime.CompilerServices.Extension>]
    static member AsSerializable(expr : Expression) = 
        new SerializableExpression(expr)