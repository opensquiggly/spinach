namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForTrigram()
  {
    Console.WriteLine();
    Console.Write("Enter trigram: ");
    string trigram = Console.ReadLine();

    int key = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);
    FastTrigramFileEnumerable enumerable = TextSearchIndex.GetFastTrigramFileEnumerable(key);

    foreach (TrigramFileInfo tfi in enumerable)
    {
      long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
      DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
      Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position} : {nameString.GetValue()}");
    }

    Pause();
  }
}
