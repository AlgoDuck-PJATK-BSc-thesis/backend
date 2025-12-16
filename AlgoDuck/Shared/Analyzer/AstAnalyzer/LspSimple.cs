// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Classes;
// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils;
// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.NodeUtils.Enums;
// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.Statements;
// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TopLevelNodes;
// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.AstNodes.TypeMembers;
// using AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;
//
// namespace AlgoDuck.Shared.Analyzer.AstAnalyzer;
//
// public class LspSimple(AstNodeProgram program)
// {
// /* We are always giving recommendations from the perspective of the final char of the main method*/
//     private readonly AstNodeMemberFunc<AstNodeClass> _baselineMainSignature = CreateNewMainNode();
//
//     public AstNodeMemberFunc<AstNodeClass> FindEntrypoint()
//     {
//         program.ProgramCompilationUnits
//             .SelectMany(cu => cu.CompilationUnitTopLevelStatements)
//             .Where(tls => tls.IsT0 && tls.AsT0.ClassAccessModifier == AccessModifier.Public)
//             .SelectMany(tls => tls.AsT0.GetMembers().Select(m => m.ClassMember))
//             .Where(clm => clm.IsT0 )
//             .Select(clm => clm.AsT0)
//             .FirstOrDefault(func => func.)
//     }    
//     
//     private static AstNodeMemberFunc<AstNodeClass> CreateNewMainNode(AstNodeClass? ownerClass = null)
//     {
//         return new AstNodeMemberFunc<AstNodeClass>
//         {
//             AccessModifier = AccessModifier.Public,
//             Modifiers = [MemberModifier.Static],
//             FuncReturnType = SpecialMemberType.Void,
//             Identifier = new Token(TokenType.Ident, 0, "main"),
//             FuncArgs =
//             [
//                 new AstNodeScopeMemberVar
//                 {
//                     Type = new ArrayType { BaseType = MemberType.String, Dim = 1 },
//                     Identifier = new Token(TokenType.Ident, 0, "args")
//                 }
//             ],
//         };
//     }
// }
//
