using System;
using System.Linq;
using ExCSS;

namespace temp_excss_test
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Testing ExCSS-Core 4.0.5 API");
            
            // Test basic parsing
            var parser = new StylesheetParser();
            var stylesheet = parser.Parse(".test { color: red; }");
            
            Console.WriteLine($"Stylesheet type: {stylesheet.GetType().Name}");
            Console.WriteLine($"Available properties: {string.Join(", ", stylesheet.GetType().GetProperties().Select(p => p.Name))}");
            
            // Test available types
            var assembly = typeof(StylesheetParser).Assembly;
            var types = assembly.GetTypes().Where(t => t.IsPublic).Select(t => t.Name).OrderBy(n => n);
            
            Console.WriteLine("\nAvailable public types:");
            foreach (var type in types)
            {
                Console.WriteLine($"  {type}");
            }
        }
    }
}