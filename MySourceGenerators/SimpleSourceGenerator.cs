using Microsoft.CodeAnalysis.Text;
using Microsoft.CodeAnalysis;

namespace MySourceGenerators
{
    [Generator]
    public class SimpleSourceGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {            
            var sourceCode = @"
        namespace generated
        {
            public static class HelloWorld
            {
                public static void SayHello() => System.Console.WriteLine(""Hello from generated code!"");
            }
        }";
            
            context.AddSource("HelloWorldGenerated.g.cs", SourceText.From(sourceCode, System.Text.Encoding.UTF8));
        }
    }

}
