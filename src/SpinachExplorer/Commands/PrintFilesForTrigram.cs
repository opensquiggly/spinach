namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForTrigram()
  {
    Console.WriteLine();
    Console.Write("Enter trigram: ");
    string trigram = Console.ReadLine();

    if (trigram is not { Length: 3 }) return;

    int matches = 0;

    var enumerator = new FastTrigramEnumerator2(trigram, TextSearchManager);

    while (enumerator.MoveNext())
    {
      matches++;
      Console.Write($"@{enumerator.CurrentData.MatchPosition} ");
      Console.Write($"User: {enumerator.CurrentData.User.Name}, ");
      Console.Write($"Repo: {enumerator.CurrentData.Repository.Name}, ");
      Console.WriteLine($"{enumerator.CurrentData.Document.ExternalIdOrPath}");
      if (enumerator.CurrentData.Document.CurrentLength <= 10000)
      {
        string fullPath = Path.Combine(enumerator.CurrentData.Repository.RootFolderPath,
          enumerator.CurrentData.Document.ExternalIdOrPath);
        FileUtils.PrintFile(fullPath,
          (int)enumerator.CurrentData.MatchPosition + 1, 3);
      }
    }

    Console.WriteLine($"Total matches: {matches}");
    Console.WriteLine();
    Pause();
  }
}
