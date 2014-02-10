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

    let initLocalRuntime(nodes : int) =
        MBrace.InitLocal(nodes)

    let private compile_aux(expr : CloudQueryExpr) =
        let e = CloudQueryExpr.toQueryExpr expr
        let f = LinqOptimizer.Core.CoreHelpers.Compile<'T>(e, fun e -> e)
        f.Invoke

    [<Cloud>]
    let loadAssembly (rawAssembly : byte []) =
        <@ cloud { Assembly.Load(rawAssembly) |> ignore } @>

    [<Cloud>]
    let compile<'T>(expr : CloudQueryExpr) : ICloud<'T> =
        cloud { 
            let f = compile_aux(expr)   
            return f() 
        }

    [<Cloud>]
    let compileAsExpr<'T>(expr : CloudQueryExpr) =
        <@ compile<'T>(expr)  @>

    let gatherTypesInObjectGraph (obj:obj) =
        let gathered = new HashSet<Type>()
        let objCounter = new ObjectIDGenerator()
        let inline add t = gathered.Add t |> ignore
        
        let rec traverseObj (obj : obj) =
            match obj with
            | null -> ()
            | :? Type as t -> traverseType t
            | :? MemberInfo as m -> traverseType m.DeclaringType
            | _ ->
                let _,firstTime = objCounter.GetId obj
                if firstTime then
                    let t = obj.GetType()
                    traverseType t

                    if t.IsPrimitive || t = typeof<string> then ()
                    elif t.IsArray then
                        let et = t.GetElementType()
                        if et.IsPrimitive then ()
                        else
                            for e in obj :?> Array do
                                traverseObj e
                    else
                        let fields = t.GetFields(BindingFlags.Instance ||| BindingFlags.NonPublic ||| BindingFlags.Public)

                        for f in fields |> Array.filter (fun f -> not f.FieldType.IsPrimitive) do
                            let o = f.GetValue(obj)
                            traverseObj o

        and traverseType (t : Type) =   
            if t.IsArray || t.IsByRef || t.IsPointer then
                traverseType <| t.GetElementType()
            elif t.IsGenericType && not t.IsGenericTypeDefinition then
                add <| t.GetGenericTypeDefinition()
                for ta in t.GetGenericArguments() do
                    traverseType ta
            else
                add t

        do traverseObj obj
        gathered |> Seq.toArray



      // recursively traverses dependency graph
    let gatherDependencies  (assemblies : seq<Assembly>) =

        let ignoredAssemblies = 
            [| typeof<int> ; typeof<int option> ; |]
            |> Array.map (fun t -> t.Assembly)

        let isIgnoredAssembly a =
            Array.exists ((=) a) ignoredAssemblies

        let tryResolveAssembly (an : AssemblyName) =
            System.AppDomain.CurrentDomain.GetAssemblies() 
            |> Array.find(fun a -> a.GetName() = an)

        let rec traverse (remaining : Assembly list) acc =
            match remaining with
            | [] -> Seq.toArray acc
            | dep :: tail when isIgnoredAssembly dep -> traverse tail acc
            | dep :: tail -> 
                let newAssemblies = dep.GetReferencedAssemblies() |> Seq.map tryResolveAssembly |> Seq.toList
                traverse  (newAssemblies @ tail) (dep :: acc)

        traverse  (Seq.toList assemblies) []