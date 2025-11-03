using System;
using System.IO;

namespace D2SLib
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string username;

                if (args.Length == 0)
                {
                    Console.Write("Enter character name: ");
                    username = Console.ReadLine();
                }
                else
                {
                    username = args[0];
                }

                if (string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Error: Username cannot be empty");
                    Environment.Exit(1);
                }

                using var exporter = new CharacterExporter();
                var base64Result = exporter.ExportCharacter(username);

                var characterBytes = Convert.FromBase64String(base64Result);
                var outputPath = Path.Combine(Globals.OUTPUT_DIR, $"{username}.d2s");

                Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

                File.WriteAllBytes(outputPath, characterBytes);

                Console.WriteLine($"Character exported successfully to: {outputPath}");
            }
            catch (ArgumentException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Details: {ex.InnerException.Message}");
                }
                Environment.Exit(1);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
    }
}