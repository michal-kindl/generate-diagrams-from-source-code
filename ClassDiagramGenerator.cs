using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;

// ClassDiagramGenerator from https://github.com/pierre3/PlantUmlClassDiagramGenerator
namespace Diagrams
{
    /// <summary>
    /// Class to generate PlantUML class diagram from C # source code
    /// </summary>
    public class ClassDiagramGenerator : CSharpSyntaxWalker
    {
        private TextWriter writer;
        private string indent;
        private int nestingDepth = 0;
        private bool showMethodParameterTypes = true;
        private bool showMethodParameters = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="writer">TextWriter to output the result</param>
        /// <param name="indent">String to be used as indent</param>
        public ClassDiagramGenerator(TextWriter writer, string indent, bool showMethodParameters, bool showMethodParameterTypes)
        {
            this.writer = writer;
            this.indent = indent;
            this.showMethodParameters = showMethodParameters;
            this.showMethodParameterTypes = showMethodParameterTypes;
        }

        /// <summary>
        /// Output interface definition in PlantUML format
        /// </summary>
        public override void VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
        {
            VisitTypeDeclaration(node, () => base.VisitInterfaceDeclaration(node));
        }

        /// <summary>
        /// Output definition of class in PlantUML format
        /// </summary>        
        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            VisitTypeDeclaration(node, () => base.VisitClassDeclaration(node));
        }

        /// <summary>
        /// Output the structure definition in PlantUML format
        /// </summary>
        public override void VisitStructDeclaration(StructDeclarationSyntax node)
        {
            var name = node.Identifier.ToString();
            var typeParam = node.TypeParameterList?.ToString() ?? "";

            WriteLine($"class **{name}{typeParam}** <<struct>> {{");

            nestingDepth++;
            base.VisitStructDeclaration(node);
            nestingDepth--;

            WriteLine("}");
        }

        /// <summary>
        /// Output definition of enum type in PlantUML format
        /// </summary>
        /// <param name="node"></param>
        public override void VisitEnumDeclaration(EnumDeclarationSyntax node)
        {
            WriteLine($"{node.EnumKeyword} **{node.Identifier}** {{");

            nestingDepth++;
            base.VisitEnumDeclaration(node);
            nestingDepth--;

            WriteLine("}");
        }

        /// <summary>
        /// Output definition of type (class, interface, structure) in PlantUML format
        /// </summary>
        private void VisitTypeDeclaration(TypeDeclarationSyntax node, Action visitBase)
        {
            var modifiers = GetTypeModifiersText(node.Modifiers);
            var keyword = (node.Modifiers.Any(SyntaxKind.AbstractKeyword) ? "abstract " : "")
                + node.Keyword.ToString();
            var name = node.Identifier.ToString();
            var typeParam = node.TypeParameterList?.ToString() ?? "";

            WriteLine($"{keyword} **{name}{typeParam}** {modifiers}{{");

            nestingDepth++;
            visitBase();
            nestingDepth--;

            WriteLine("}");
            
            if (node.BaseList != null)
            {             
                foreach (var b in node.BaseList.Types)
                {
                    var nodeBaseType = b.Type.ToFullString();
                    var shortNodeBaseType = nodeBaseType.Split('<').First();

                    if (nodeBaseType.Contains("<"))
                    {                        
                        WriteLine($"class \"{shortNodeBaseType}\" as {nodeBaseType}\n");
                    }

                    WriteLine($"{name} <|-- {shortNodeBaseType}");
                }
            }
        }

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var modifiers = GetMemberModifiersText(node.Modifiers);
            var name = node.Identifier.ToString();

            var args = showMethodParameters
                ? node.ParameterList.Parameters.Select(p =>
                    showMethodParameterTypes
                    ? $"{p.Identifier}:{p.Type}"
                    : $"{p.Identifier}")
                : new string[] { };
            //var args = node.ParameterList.Parameters.Select(p => $"{p.Identifier}:{p.Type}");

            WriteLine($"{modifiers}**{name}**({string.Join(", ", args)})");
        }
        /// <summary>
        /// Output field definition
        /// </summary>
        public override void VisitFieldDeclaration(FieldDeclarationSyntax node)
        {
            var modifiers = GetMemberModifiersText(node.Modifiers);
            var typeName = node.Declaration.Type.ToString();
            var variables = node.Declaration.Variables;
            foreach (var field in variables)
            {
                var useLiteralInit = field.Initializer?.Value?.Kind().ToString().EndsWith("LiteralExpression") ?? false;
                var initValue = "";// useLiteralInit ? (" = " + field.Initializer.Value.ToString()) : "";

                WriteLine($"{modifiers}{field.Identifier} : {typeName}{initValue}");
            }
        }

        /// <summary>
        /// Output property definition
        /// </summary>        
        public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
        {
            var modifiers = GetMemberModifiersText(node.Modifiers);
            var name = node.Identifier.ToString();
            var typeName = node.Type.ToString();
            var accessor = node.AccessorList.Accessors
                .Where(x => !x.Modifiers.Select(y => y.Kind()).Contains(SyntaxKind.PrivateKeyword))
                .Select(x => $"<<{(x.Modifiers.ToString() == "" ? "" : (x.Modifiers.ToString() + " "))}{x.Keyword}>>");

            var useLiteralInit = node.Initializer?.Value?.Kind().ToString().EndsWith("LiteralExpression") ?? false;
            var initValue = useLiteralInit ? (" = " + node.Initializer.Value.ToString()) : "";

            WriteLine($"{modifiers}**{name}**: {typeName} {string.Join(" ", accessor)}{initValue}");
        }

        /// <summary>
        /// Output method definition
        /// </summary>
        public override void VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var modifiers = GetMemberModifiersText(node.Modifiers);
            var name = node.Identifier.ToString();
            var returnType = node.ReturnType.ToString();
            var args = showMethodParameters 
                ? node.ParameterList.Parameters.Select(p =>
                    showMethodParameterTypes 
                    ? $"{p.Identifier}:{p.Type}"
                    : $"{p.Identifier}")
                : new string[] { };
            //var args = node.ParameterList.Parameters.Select(p => $"{p.Identifier}:{p.Type}");
            WriteLine($"{modifiers}**{name}**({string.Join(", ", args)}) : {returnType}");
        }

        /// <summary>
        /// Output enum type members
        /// </summary>
        /// <param name="node"></param>
        public override void VisitEnumMemberDeclaration(EnumMemberDeclarationSyntax node)
        {
            WriteLine($"{node.Identifier}{node.EqualsValue},");
        }

        /// <summary>
        /// Write one line to the result output TextWriter.
        /// </summary>
        private void WriteLine(string line)
        {
            //Append indentation to the beginning of the line by the nesting level
            var space = string.Concat(Enumerable.Repeat(indent, nestingDepth));
            writer.WriteLine(space + line);
        }

        /// <summary>
        /// Convert qualifier of type (class, interface, structure) to a string
        /// </summary>
        /// <param name="modifiers">TokenList of the modifier</param>
        /// <returns>String after conversion</returns>
        private string GetTypeModifiersText(SyntaxTokenList modifiers)
        {
            var tokens = modifiers.Select(token =>
            {
                switch (token.Kind())
                {
                    case SyntaxKind.PublicKeyword:
                    case SyntaxKind.PrivateKeyword:
                    case SyntaxKind.ProtectedKeyword:
                    case SyntaxKind.InternalKeyword:
                    case SyntaxKind.AbstractKeyword:
                        return "";
                    default:
                        return $"<<{token.ValueText}>>";
                }
            }).Where(token => token != "");

            var result = string.Join(" ", tokens);
            if (result != string.Empty)
            {
                result += " ";
            };
            return result;
        }

        /// <summary>
        /// Convert qualifier of type member to string
        /// </summary>
        /// <param name="modifiers">TokenList of the modifier</param>
        /// <returns></returns>
        private string GetMemberModifiersText(SyntaxTokenList modifiers)
        {
            var tokens = modifiers.Select(token =>
            {
                switch (token.Kind())
                {
                    case SyntaxKind.PublicKeyword:
                        return "+";
                    case SyntaxKind.PrivateKeyword:
                        return "-";
                    case SyntaxKind.ProtectedKeyword:
                        return "#";
                    case SyntaxKind.AbstractKeyword:
                    case SyntaxKind.StaticKeyword:
                        return $"{{{token.ValueText}}}";
                    case SyntaxKind.InternalKeyword:
                    default:
                        return $"<<{token.ValueText}>>";
                }
            });

            var result = string.Join(" ", tokens);
            if (result != string.Empty)
            {
                result += " ";
            };
            return result;
        }
    }
}
