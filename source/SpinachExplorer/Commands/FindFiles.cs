namespace SpinachExplorer;

internal static partial class Program
{
  private static void FindFiles()
  {
    Console.WriteLine();
    Console.Write("Enter regex: ");
    string regex = Console.ReadLine();

    foreach (RegexEnumerable.MatchingFile matchingFile in TextSearchIndex.RegexEnumerable(regex))
    {
      Console.WriteLine($"{matchingFile.FileName}");
      foreach (RegexEnumerable.MatchingPosition matchingPosition in matchingFile.Matches)
      {
        Console.WriteLine($"  From {matchingPosition.StartIndex} to {matchingPosition.EndIndex}");
      }
    }

    Pause();
  }
}
