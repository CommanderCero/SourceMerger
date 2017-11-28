using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SourceMerger
{
    public class SourceCollector : CSharpSyntaxWalker
    {
        public HashSet<string> SystemImports { get; } = new HashSet<string>();
        public List<string> Declarations { get; } = new List<string>();

        private readonly HashSet<string> collectedNamespaces = new HashSet<string>();
        private List<SourcePath> additionalSources;

        public void CollectSources(string sourceFolder, List<SourcePath> additionalSources)
        {
            this.additionalSources = additionalSources;
            CollectFolderContent(sourceFolder, SearchOption.AllDirectories);
        }

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            // Only collect "using XXX"
            if (node.UsingKeyword.IsMissing)
                return;

            var nodeName = node.Name.ToString();
            var namespaceParts = nodeName.Split('.');
            if (namespaceParts[0] == "System")
            {
                SystemImports.Add(node.ToString());
            }
            else if(collectedNamespaces.Contains(nodeName)) // Did we already collect the sources from the namespace?
            {
                // Find the configured Path for the Namespace
                var sourcePath = additionalSources.FirstOrDefault(x => x.Name == namespaceParts[0]);
                if(sourcePath == null)
                    throw new Exception($"The namespace '{nodeName}' is not configured in AdditionalSources");

                // Create the folder path
                // using Namespace.UnderNamespace => NamespacePath\UnderNamespace
                var folderPath = string.Join(@"\", sourcePath.Path, namespaceParts.Skip(1));
                if(!Directory.Exists(folderPath))
                    throw new Exception($"The folder '{folderPath}' for the namespace '{node.Name}' could not be found");

                // Collect all content from the files in the folder
                collectedNamespaces.Add(nodeName);
                CollectFolderContent(folderPath, SearchOption.TopDirectoryOnly);
            }
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            Declarations.Add(node.ToString());
        }

        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            Declarations.Add(node.ToString());
        }

        private void CollectFolderContent(string folderPath, SearchOption option)
        {
            var files = Directory.GetFiles(folderPath, "*.cs", option);
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                Visit(CSharpSyntaxTree.ParseText(content).GetRoot());
            }
        }
    }
}
