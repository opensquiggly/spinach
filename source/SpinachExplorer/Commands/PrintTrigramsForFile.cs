namespace SpinachExplorer;


internal static partial class Program
{
  private static void PrintFilesForTrigram()
  {
    Console.WriteLine();

    Console.Write("Enter trigram: ");
    string trigram = Console.ReadLine();

    int key = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);

    var enumerator = TextSearchIndex.GetTrigramFileEnumerator(key);

    foreach (TrigramFileInfo tfi in enumerator)
    {
      Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position}");      
    }

    Pause();
  }
}
