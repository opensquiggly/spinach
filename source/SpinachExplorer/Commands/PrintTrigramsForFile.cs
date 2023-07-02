namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForTrigram()
  {
    Console.WriteLine();

    Console.Write("Enter trigram: ");
    string trigram = Console.ReadLine();

    int key = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);

    // TrigramFileEnumerator enumerable1 = TextSearchIndex.GetTrigramFileEnumerable(key);
    // foreach (TrigramFileInfo tfi in enumerable1)
    // {
    //   Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position}");
    // }

    FastTrigramFileEnumerable enumerable2 = TextSearchIndex.GetFastTrigramFileEnumerable(key);
    foreach (TrigramFileInfo tfi in enumerable2)
    {
      Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position}");
    }

    Pause();
  }
}
