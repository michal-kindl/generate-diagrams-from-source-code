using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using System.IO;
using Microsoft.CodeAnalysis.Text;
using System.Text;

namespace Diagrams
{   
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("****************************************************************************");
            Console.WriteLine("*                                                                          *");
            Console.WriteLine("*   Diagram  generator for PlantUML                                        *");
            Console.WriteLine("*   To generata a class diagram specify a file .cs or a directory          *");
            Console.WriteLine("*                                                                          *");
            Console.WriteLine("****************************************************************************");
            Console.WriteLine("Usage: ");
            Console.WriteLine("dotnet run -- [source dir] [output dir] [showMethParams] [showMethParamTypes]");

            Console.WriteLine(args);

            // to test Class Diagram
            // args = new[] { @"ClassDiagramGenerator.cs", @"..\uml" }; 

            if (args.Length < 1)
            {
                Console.WriteLine("Specify a file or directory");
                Console.ReadKey();
                return;
            }

            var input = args[0];            

            IEnumerable<string> files;
            if (Directory.Exists(input))
            {
                files = Directory.EnumerateFiles(Path.GetFullPath(input), "*.cs", SearchOption.AllDirectories);
            }
            else if (File.Exists(input))
            {                
                try
                {
                    var fullname = Path.GetFullPath(input);
                    files = new[] { fullname };
                }
                catch
                {                    
                    Console.WriteLine("Invalid name");
                    Console.ReadKey();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Specify an existing file or directory");
                return;
            }

            var outputDir = "";
            if (args.Length >= 2)
            {
                if (Directory.Exists(args[1]))
                {
                    outputDir = args[1];
                }
            }

            if (outputDir == "")
            {
                outputDir = Path.Combine(Path.GetDirectoryName(files.First()), "uml");
                Directory.CreateDirectory(outputDir);
            }

            List<string> outputFiles = new List<string>();

            bool _showMethodParameters = Boolean.Parse(args[2]);
            Console.WriteLine(args[2]);
            bool _showMethodParameterTypes = Boolean.Parse(args[3]);

            foreach (var file in files)
            {
                Console.WriteLine($"Generation PlantUML text for {file}...");
                string outputFile = Path.Combine(outputDir, Path.GetFileNameWithoutExtension(file));                

                using (var stream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var tree = CSharpSyntaxTree.ParseText(SourceText.From(stream));
                    var root = tree.GetRoot();

                    using (var writer = new StreamWriter(new FileStream(outputFile + ".ClassDiagram.plantuml", FileMode.OpenOrCreate, FileAccess.Write)))
                    {
                        writer.WriteLine("@startuml");

                        var gen = new ClassDiagramGenerator(writer, "    ", _showMethodParameters, _showMethodParameterTypes);
                        gen.Visit(root);

                        writer.Write("@enduml");
                     }                     
                }

                outputFiles.Add(file);
            }

            //--- generate diagram index
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("# Class diagram index");

            outputFiles.ForEach(a => {
                string className = Path.GetFileNameWithoutExtension(a);

                if (className.Contains("AssemblyInfo") ||
                    className.Contains("TemporaryGeneratedFile_")) {
                    return;
                }

                string classPath = Path.GetDirectoryName(a);
                string classNamespace = new DirectoryInfo(classPath).Parent.Name;
                sb.AppendLine($"## #{classNamespace}.{className}");
                sb.AppendLine();
                sb.AppendLine("@startuml");
                sb.AppendLine($@"!include %dirpath%/../../uml/classes/{className}.ClassDiagram.plantuml");
                sb.AppendLine("@enduml");
                sb.AppendLine();
            });

            string indexFullPath = Path.GetFullPath($"{outputDir}/../..");
            File.WriteAllText($"{indexFullPath}/classes_index.md", sb.ToString());

            Console.WriteLine("Completed");
            //Console.ReadKey();
        }
    }
}

