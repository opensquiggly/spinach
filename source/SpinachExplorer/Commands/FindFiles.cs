namespace SpinachExplorer;

internal static partial class Program
{
  private static void FindFiles()
  {
    Console.WriteLine();
    Console.Write("Enter regex: ");
    string regex = Console.ReadLine();

    IEnumerable<RegexEnumerable.MatchingFile> matches = TextSearchIndex.RegexEnumerable(regex);

    var stopwatch = Stopwatch.StartNew();
    int count = matches.Count();
    stopwatch.Stop();

    foreach (RegexEnumerable.MatchingFile matchingFile in TextSearchIndex.RegexEnumerable(regex))
    {
      Console.WriteLine($"{matchingFile.FileName}");
      // foreach (RegexEnumerable.MatchingPosition matchingPosition in matchingFile.Matches)
      // {
      //   Console.WriteLine($"  From {matchingPosition.StartIndex} to {matchingPosition.EndIndex}");
      // }
    }

    Console.WriteLine();
    Console.WriteLine($"Found {count} matches in {stopwatch.ElapsedMilliseconds} milliseconds");

    // If Spinach were thread-safe, we could do this
    // var matchingFiles = TextSearchIndex.RegexEnumerable(regex).AsParallel().AsOrdered();
    //
    // matchingFiles.ForAll(matchingFile =>
    // {
    //   Console.WriteLine($"{matchingFile.FileName}");
    //
    //   matchingFile.Matches.AsParallel().ForAll(matchingPosition =>
    //   {
    //     Console.WriteLine($"  From {matchingPosition.StartIndex} to {matchingPosition.EndIndex}");
    //   });
    // });

    Pause();
  }
}
