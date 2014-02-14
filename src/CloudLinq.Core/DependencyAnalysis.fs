namespace CloudLinq.Core
    
    open System
    open System.Collections
    open System.Collections.Generic
    open System.Linq
    open System.Linq.Expressions
    open System.Reflection
    open System.Collections.Concurrent

    type private DependencyAnalysisVisitor () =
        inherit ExpressionVisitor() with

            let dependencies = HashSet<Assembly>()

            member this.GetDependencies() : Assembly seq = 
                let rec traverse (assemblies : Assembly list) acc =
                    match assemblies with
                    | [] -> acc
                    | h :: t ->
                        if h.GlobalAssemblyCache then
                            traverse t acc
                        else
                            let d = h.GetReferencedAssemblies() 
                                    |> Seq.choose (fun a -> AppDomain.CurrentDomain.GetAssemblies() 
                                                            |> Array.tryFind (fun n -> n.GetName() = a))
                                    |> Seq.toList
                            traverse (d @ t) (h :: acc)
                traverse (Seq.toList dependencies) [] :> _

            let addAssembly (asm : Assembly) =
                if not asm.GlobalAssemblyCache then
                    dependencies.Add(asm) |> ignore

            override this.VisitNew(expr : NewExpression) =
                addAssembly expr.Type.Assembly
                expr.Update(this.Visit expr.Arguments) :> _
                
            override this.VisitMember(expr : MemberExpression) =
                addAssembly expr.Member.DeclaringType.Assembly
                expr.Update(this.Visit expr.Expression) :> _

            override this.VisitMethodCall(expr : MethodCallExpression) =
                addAssembly expr.Method.DeclaringType.Assembly
                expr.Update(this.Visit expr.Object, this.Visit expr.Arguments) :> _

            override this.VisitParameter(expr : ParameterExpression) =
                addAssembly expr.Type.Assembly
                expr :> _

            override this.VisitUnary(expr : UnaryExpression) =
                if expr.NodeType = ExpressionType.Convert then 
                    addAssembly expr.Type.Assembly
                expr.Update(this.Visit expr.Operand) :> _

            override this.Visit(expr : Expression) =
                if expr <> null then addAssembly expr.Type.Assembly
                base.Visit(expr) 

    module DependencyAnalysis =
        let GetAssemblies(expr : Expression) : seq<Assembly>  =
            let da = new DependencyAnalysisVisitor()
            da.Visit(expr) |> ignore
            da.GetDependencies()

            
