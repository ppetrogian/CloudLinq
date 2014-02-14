namespace CloudLinq.Core

open LinqOptimizer.FSharp
open LinqOptimizer.Base
open LinqOptimizer.Core

open System
open System.IO
open System.Collections
open System.Collections.Generic
open System.Reflection
open System.Linq.Expressions
open System.Runtime.Serialization

open Serialize.Linq
open Serialize.Linq.Interfaces
open Serialize.Linq.Extensions
open Serialize.Linq.Nodes
open Serialize.Linq.Serializers

open FsPickler

type CloudQueryExpr =
    | Source of SerializableExpression * Type * QueryExprType
    | Generate of SerializableExpression * SerializableExpression * SerializableExpression * SerializableExpression
    | Transform of SerializableExpression * CloudQueryExpr 
    | TransformIndexed of SerializableExpression * CloudQueryExpr 
    | Filter of SerializableExpression * CloudQueryExpr 
    | FilterIndexed of SerializableExpression * CloudQueryExpr 
    | NestedQuery of (SerializableExpression * CloudQueryExpr) * CloudQueryExpr 
    | NestedQueryTransform of (SerializableExpression * CloudQueryExpr) * SerializableExpression * CloudQueryExpr 
    | Aggregate of SerializableExpression * SerializableExpression * CloudQueryExpr
    | Sum of CloudQueryExpr 
    | Count of CloudQueryExpr 
    | Take of SerializableExpression * CloudQueryExpr
    | TakeWhile of SerializableExpression * CloudQueryExpr
    | SkipWhile of SerializableExpression * CloudQueryExpr
    | Skip of SerializableExpression * CloudQueryExpr 
    | ForEach of SerializableExpression * CloudQueryExpr 
    | GroupBy of SerializableExpression * CloudQueryExpr * Type
    | OrderBy of (SerializableExpression * Order) list * CloudQueryExpr 
    | ToList of CloudQueryExpr
    | ToArray of CloudQueryExpr
    | RangeGenerator of SerializableExpression * SerializableExpression
    | RepeatGenerator of SerializableExpression * SerializableExpression
    | ZipWith of SerializableExpression * SerializableExpression * SerializableExpression

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module CloudQueryExpr =
    let lam (expr : SerializableExpression) = expr.Expression :?> LambdaExpression
    let param (expr : SerializableExpression) = expr.Expression :?> ParameterExpression
    let serializable (expr : Expression) = new SerializableExpression(expr)

    let ofQueryExpr(expr : QueryExpr) =
        let rec transform (expr : QueryExpr) : CloudQueryExpr =
            match expr with
            | QueryExpr.Source(e, t, qt)                        -> Source(serializable e, t, qt)                    
            | QueryExpr.Generate(e1, e2, e3, e4)                -> Generate(serializable e1,serializable  e2,serializable  e3,serializable  e4)           
            | QueryExpr.Transform(e, q)                         -> Transform(serializable e, transform q)                    
            | QueryExpr.TransformIndexed(e, q)                  -> TransformIndexed(serializable e, transform q)             
            | QueryExpr.Filter(e, q)                            -> Filter(serializable e, transform q)                       
            | QueryExpr.FilterIndexed(e, q)                     -> FilterIndexed(serializable e, transform q)                
            | QueryExpr.NestedQuery((e, q), q')                 -> NestedQuery((serializable e, transform q), transform q')            
            | QueryExpr.NestedQueryTransform((e,q), e', q')     -> NestedQueryTransform((serializable e, transform q),serializable  e',transform q')
            | QueryExpr.Aggregate(e1, e2, q)                    -> Aggregate(serializable e1,serializable  e2,transform  q)               
            | QueryExpr.Sum(q)                                  -> Sum(transform q)                             
            | QueryExpr.Count(q)                                -> Count(transform q)                           
            | QueryExpr.Take(e,q)                               -> Take(serializable e,transform q)                          
            | QueryExpr.TakeWhile(e,q)                          -> TakeWhile(serializable e,transform q)                     
            | QueryExpr.SkipWhile(e,q)                          -> SkipWhile(serializable e,transform q)                     
            | QueryExpr.Skip(e,q)                               -> Skip(serializable e,transform q)                          
            | QueryExpr.ForEach(e,q)                            -> ForEach(serializable e,transform q)                       
            | QueryExpr.GroupBy(e,q,t)                          -> GroupBy(serializable e,transform q,t)                     
            | QueryExpr.OrderBy(ls, q)                          -> let ls = ls |> List.map (fun (e,o) -> (serializable e, o)) in OrderBy(ls, transform q)                     
            | QueryExpr.ToList(q)                               -> ToList(transform q)                          
            | QueryExpr.ToArray(q)                              -> ToArray(transform q)                         
            | QueryExpr.RangeGenerator(e1, e2)                  -> RangeGenerator(serializable e1,serializable e2)             
            | QueryExpr.RepeatGenerator(e1, e2)                 -> RepeatGenerator(serializable e1,serializable e2)            
            | QueryExpr.ZipWith(e1, e2, e3)                     -> ZipWith(serializable e1,serializable e2,serializable e3)                
        transform expr

    let toQueryExpr(expr : CloudQueryExpr) =
        let rec transform (expr : CloudQueryExpr) : QueryExpr =
            match expr with
            | Source(e, t, qt)                    -> QueryExpr.Source(e.Expression, t, qt)                    
            | Generate(e1, e2, e3, e4)            -> QueryExpr.Generate(e1.Expression, lam e2, lam e3, lam e4)          
            | Transform(e, q)                     -> QueryExpr.Transform(lam e, transform q)                    
            | TransformIndexed(e, q)              -> QueryExpr.TransformIndexed(lam e, transform q)             
            | Filter(e, q)                        -> QueryExpr.Filter(lam e, transform q)                       
            | FilterIndexed(e, q)                 -> QueryExpr.FilterIndexed(lam e, transform q)                
            | NestedQuery((e, q), q')             -> QueryExpr.NestedQuery((param e, transform q), transform q')            
            | NestedQueryTransform((e,q), e', q') -> QueryExpr.NestedQueryTransform((param e, transform q), lam e',transform q')
            | Aggregate(e1, e2, q)                -> QueryExpr.Aggregate(e1.Expression,lam e2,transform  q)               
            | Sum(q)                              -> QueryExpr.Sum(transform q)                             
            | Count(q)                            -> QueryExpr.Count(transform q)                           
            | Take(e,q)                           -> QueryExpr.Take(e.Expression ,transform q)                          
            | TakeWhile(e,q)                      -> QueryExpr.TakeWhile(lam e,transform q)                     
            | SkipWhile(e,q)                      -> QueryExpr.SkipWhile(lam e,transform q)                     
            | Skip(e,q)                           -> QueryExpr.Skip(e.Expression,transform q)                          
            | ForEach(e,q)                        -> QueryExpr.ForEach(lam e,transform q)                       
            | GroupBy(e,q,t)                      -> QueryExpr.GroupBy(lam e,transform q,t)                     
            | OrderBy(ls, q)                      -> let ls = ls |> List.map (fun (e,o) -> (lam e, o)) in QueryExpr.OrderBy(ls, transform q)   
            | ToList(q)                           -> QueryExpr.ToList(transform q)                          
            | ToArray(q)                          -> QueryExpr.ToArray(transform q)                         
            | RangeGenerator(e1, e2)              -> QueryExpr.RangeGenerator(e1.Expression, e2.Expression)             
            | RepeatGenerator(e1, e2)             -> QueryExpr.RepeatGenerator(e1.Expression, e2.Expression)            
            | ZipWith(e1, e2, e3)                 -> QueryExpr.ZipWith(e1.Expression, e2.Expression,lam e3)                
        transform expr

    let getDependencies(expr : CloudQueryExpr) : Assembly seq =
        let assemblies = HashSet<Assembly>()
        let add (expr : SerializableExpression) =
            assemblies.UnionWith(DependencyAnalysis.GetAssemblies(expr.Expression))
        let rec traverse (expr : CloudQueryExpr) =
            match expr with
            | Source(e, t, qt)                    -> add e
            | Generate(e1, e2, e3, e4)            -> add e1; add e2; add e3; add e4;
            | Transform(e, q)         
            | TransformIndexed(e, q)  
            | Filter(e, q)            
            | FilterIndexed(e, q)                 -> add e; traverse q       
            | NestedQuery((e, q), q')             -> add e; traverse q; traverse q'    
            | NestedQueryTransform((e,q), e', q') -> add e; add e'; traverse q; traverse q'
            | Aggregate(e1, e2, q)                -> add e1; add e2; traverse q
            | Sum(q)                              -> traverse q                      
            | Count(q)                            -> traverse q                                   
            | Take(e,q)                           -> add e; traverse q                       
            | TakeWhile(e,q)                      -> add e; traverse q                       
            | SkipWhile(e,q)                      -> add e; traverse q                        
            | Skip(e,q)                           -> add e; traverse q                            
            | ForEach(e,q)                        -> add e; traverse q                           
            | GroupBy(e,q,t)                      -> add e; traverse q                      
            | OrderBy(ls, q)                      -> List.iter (fun (e,o) -> add e) ls; traverse q
            | ToList(q)                           -> traverse q                        
            | ToArray(q)                          -> traverse q                        
            | RangeGenerator(e1, e2)              -> add e1; add e2
            | RepeatGenerator(e1, e2)             -> add e1; add e2
            | ZipWith(e1, e2, e3)                 -> add e1; add e2; add e3
        traverse expr
        assemblies :> _

    let toRawQuery (query : CloudQueryExpr) =
        let fsp = new FsPickler()
        use ms = new MemoryStream()
        fsp.Serialize<CloudQueryExpr>(ms, query)
        ms.ToArray()

    let ofRawQuery (queryRaw : byte []) =
        let fsp = new FsPickler()
        use ms = new MemoryStream(queryRaw)
        fsp.Deserialize<CloudQueryExpr>(ms)        

[<Serializable>]
type CloudQueryPackage (queryRaw : byte [], assemblies : byte [] []) =

    member this.GetQuery () = 
        let appDomain = AppDomain.CurrentDomain.GetAssemblies()
        assemblies |> Array.iter(fun raw -> Assembly.Load(raw) |> ignore)
        CloudQueryExpr.ofRawQuery queryRaw

    new(query : CloudQueryExpr) =
        let assemblies = CloudQueryExpr.getDependencies query
                         |> Seq.map (fun asm -> let loc = asm.Location
                                                File.ReadAllBytes(loc))
                         |> Seq.toArray
        CloudQueryPackage(CloudQueryExpr.toRawQuery query, assemblies)