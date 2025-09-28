using System;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        Dump("ImGuiSharp");
        Dump("StbTrueTypeSharp");
    }

    static void Dump(string name)
    {
        try
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == name) ?? Assembly.Load(name);
            Console.WriteLine($"Loaded: {asm.FullName}");
            foreach (var t in asm.GetExportedTypes().OrderBy(t => t.FullName))
            {
                Console.WriteLine(t.FullName);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to load {name}: {ex.Message}");
        }
    }
}
