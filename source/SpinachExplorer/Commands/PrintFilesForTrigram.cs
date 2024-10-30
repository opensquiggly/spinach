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
      Console.Write($"@{enumerator.CurrentData} ");
      Console.Write($"User: {enumerator.CurrentUser.Name}, ");
      Console.Write($"Repo: {enumerator.CurrentRepository.Name}, ");
      Console.WriteLine($"{enumerator.CurrentDocument.ExternalIdOrPath}");
    }

    Console.WriteLine($"Total matches: {matches}");
    Console.WriteLine();
    Pause();
  }
}
