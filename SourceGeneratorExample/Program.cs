using generated;
namespace SourceGeneratorExample
{
    internal class Program
    {
        static void Main(string[] args)
        {
            HelloWorld.SayHello();

            Product p = new Product();
            p.Id = 1;
            p.Name = "Test";
            p.Price = 100;
           
            var s = p.Serialize();
        }
    }
}