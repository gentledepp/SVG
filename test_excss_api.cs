using System;
using System.Linq;
using ExCSS;

class Program
{
    static void Main()
    {
        Console.WriteLine("Testing ExCSS 4.3.0 API compatibility");
        
        try
        {
            var parser = new StylesheetParser();
            var cssText = "div { color: red; font-size: 14px; } #test { background: blue; }";
            var stylesheet = parser.Parse(cssText);
            
            Console.WriteLine($"Parsed stylesheet with {stylesheet.StyleRules.Count} rules");
            
            foreach (var rule in stylesheet.StyleRules.OfType<StyleRule>())
            {
                Console.WriteLine($"Selector: {rule.Selector}");
                Console.WriteLine($"Selector Type: {rule.Selector.GetType().Name}");
                
                // Test Style property access
                Console.WriteLine($"Style declarations count: {rule.Style.Length}");
                
                foreach (var decl in rule.Style)
                {
                    Console.WriteLine($"  Property: {decl.Name} = {decl.Value}");
                }
                
                // Test specificity calculation
                try
                {
                    Console.WriteLine($"Selector string: {rule.Selector.ToString()}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error accessing selector: {ex.Message}");
                }
            }
            
            // Test font face rules
            foreach (var rule in stylesheet.FontfaceSetRules)
            {
                Console.WriteLine($"FontFace rule type: {rule.GetType().Name}");
                var props = rule.GetType().GetProperties();
                foreach (var prop in props)
                {
                    Console.WriteLine($"  Property: {prop.Name} ({prop.PropertyType.Name})");
                }
            }
            
            Console.WriteLine("API test completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}