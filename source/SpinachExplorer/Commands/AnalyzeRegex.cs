namespace SpinachExplorer;

using Spinach.Regex;
using Spinach.Regex.Analyzers;

internal static partial class Program
{
  private static void AnalyzeRegex()
  {
    Console.WriteLine();
    Console.Write("Enter regex: ");
    string regex = Console.ReadLine();

    Spinach.Regex.Types.NormalizedRegex normalized = RegexParser.Parse(regex);

    Console.WriteLine();
    Console.WriteLine("Normalized Regex Structure");
    Console.WriteLine("--------------------------");
    RegexPrinter.Print(normalized);

    Spinach.Regex.Types.LiteralQueryNode queryNode = LiteralQueryBuilder.BuildQuery(normalized);
    Console.WriteLine();
    Console.WriteLine("Candidate Documents Query");
    Console.WriteLine("-------------------------");
    LiteralQueryBuilder.Print(queryNode);

    Pause();
  }
}
