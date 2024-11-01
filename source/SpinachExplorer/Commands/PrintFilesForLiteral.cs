namespace SpinachExplorer;

using Spinach.Utils;
using System.Diagnostics;

internal static partial class Program
{
  private static void PrintLiteralsForTrigram()
  {
    Console.WriteLine();

    Console.Write("Enter string literal: ");
    string literal = Console.ReadLine();
    Console.WriteLine();

    // FastLiteralEnumerable enumerable = TextSearchIndex.GetFastLiteralEnumerable(literal);
    //
    // var stopwatch = Stopwatch.StartNew();
    // int count = enumerable.Count();
    // stopwatch.Stop();

    var enumerator = new FastLiteralEnumerator2(literal, TextSearchManager);
    int matches = 0;

    while (enumerator.MoveNext())
    {
      matches++;
      Console.Write($"@{enumerator.CurrentData.MatchPosition} : ");
      Console.Write($"User: {enumerator.CurrentData.User.Name}, ");
      Console.Write($"Repo: {enumerator.CurrentData.Repository.Name}, ");
      Console.WriteLine($"{enumerator.CurrentData.Document.ExternalIdOrPath}");
      FileUtils.PrintFile(enumerator.CurrentData.Document.ExternalIdOrPath, (int) enumerator.CurrentData.MatchPosition + 1,
        literal.Length);
    }

    Console.WriteLine($"Total matches: {matches}");
    Console.WriteLine();
    Pause();
  }
}
