using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceMerger
{
    public class UsingsCollector
    {
        public (HashSet<string> usings, List<string> classes) CollectUsings(IEnumerable<string> sources)
        {
            var usingWalker = new UsingWalker();
            foreach (var source in sources)
            {
                var tree = CSharpSyntaxTree.ParseText(source);
                usingWalker.Visit(tree.GetRoot());
            }

            return (usingWalker.Usings, usingWalker.Classes);
        }

        private class UsingWalker : CSharpSyntaxWalker
        {
            public HashSet<string> Usings;
            public List<string> Classes;

            public UsingWalker()
            {
                Usings = new HashSet<string>();
                Classes = new List<string>();
            }

            public override void VisitUsingDirective(UsingDirectiveSyntax node)
            {
                Usings.Add(node.Name.ToFullString());
            }

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                Classes.Add(node.ToFullString());
            }
        }
    }
}
