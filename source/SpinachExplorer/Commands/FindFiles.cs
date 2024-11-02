namespace SpinachExplorer;

using Spinach.Misc;

internal static partial class Program
{
  private static void FindFiles()
  {
    Console.WriteLine();
    Console.Write("Enter regex: ");
    string regex = Console.ReadLine();

    // IEnumerable<RegexEnumerable.MatchingFile> matches = TextSearchIndex.RegexEnumerable(regex);
    //
    // var stopwatch = Stopwatch.StartNew();
    // int count = matches.Count();
    // stopwatch.Stop();

    var matches = new RegexEnumerable2(regex, TextSearchManager);

    foreach (MatchData match in matches)
    {
      Console.Write($"@{match.MatchPosition} : ");
      Console.Write($"User: {match.User.Name}, ");
      Console.Write($"Repo: {match.Repository.Name}, ");
      Console.WriteLine($"{match.Document.ExternalIdOrPath}");
    }

    // foreach (RegexEnumerable.MatchingFile matchingFile in TextSearchIndex.RegexEnumerable(regex))
    // {
    //   Console.WriteLine($"{matchingFile.FileName}");
      // foreach (RegexEnumerable.MatchingPosition matchingPosition in matchingFile.Matches)
      // {
      //   Console.WriteLine($"  From {matchingPosition.StartIndex} to {matchingPosition.EndIndex}");
      // }
    // }

    // Console.WriteLine();
    // Console.WriteLine($"Found {count} matches in {stopwatch.ElapsedMilliseconds} milliseconds");

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
