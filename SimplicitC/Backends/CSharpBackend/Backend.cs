using SimplicitC.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Antlr4.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;

namespace SimplicitC.Backends.CSharpBackend
{
    class Backend : ISimCBackend
    {

        private IModule module;
        private SyntaxTree syntaxTree;
        private Assembly assembly;

        readonly List<Error> errors = new List<Error>();

        public IEnumerable<Error> Errors => errors;

        public bool Compile(string code)
        {
            syntaxTree = Parse(code);
            assembly = Build(syntaxTree);
            if (assembly == null) //Compilation failed
            {
                return false;
            }
            Type type = assembly.GetType("SimplicitC.Runtime.SimCMainModule");
            object obj = Activator.CreateInstance(type);
            module = (IModule)obj;
            return true;
        }

        private SyntaxTree Parse(string code)
        {

            var stream = new AntlrInputStream(code);
            var lexer = new SimplicitCLexer(stream);
            var tokenStream = new CommonTokenStream(lexer);
            var parser = new SimplicitCParser(tokenStream);
            var translator = new Translator();
            SyntaxTree syntaxTree = translator.ParseTree(parser.program());
            Console.WriteLine(syntaxTree.ToString());
            return syntaxTree;
        }
        private Assembly Build(SyntaxTree syntaxTree)
        {
            string assemblyName = Path.GetRandomFileName();

            MetadataReference[] references = new MetadataReference[]
            {
    MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
    MetadataReference.CreateFromFile(typeof(SimplicitCStdLib.Globals).Assembly.Location),
            };

            CSharpCompilation compilation = CSharpCompilation.Create(
    assemblyName,
    syntaxTrees: new[] { syntaxTree },
    references: references,
    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
            using (var ms = new MemoryStream())
            {
                EmitResult result = compilation.Emit(ms);

                if (!result.Success)
                {
                    IEnumerable<Diagnostic> failures = result.Diagnostics.Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                    foreach (Diagnostic diagnostic in failures)
                    {
                        Error e = new Error(diagnostic.ToString());
                        errors.Add(e);
                    }
                }
                else
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    Assembly assembly = Assembly.Load(ms.ToArray());

                    return assembly;
                }
            }
            return null;
        }

        public int Execute()
        {
            module._EntryPoint();
            return 0;
        }
    }
}
