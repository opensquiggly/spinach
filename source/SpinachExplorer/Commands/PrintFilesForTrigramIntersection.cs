namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForTrigramIntersection()
  {
    Console.WriteLine();

    Console.Write("Enter trigram 1: ");
    string trigram1 = Console.ReadLine();

    Console.Write("Enter trigram 2: ");
    string trigram2 = Console.ReadLine();

    int key1 = char.ToLower(trigram1[0]) * 128 * 128 + char.ToLower(trigram1[1]) * 128 + char.ToLower(trigram1[2]);
    int key2 = char.ToLower(trigram2[0]) * 128 * 128 + char.ToLower(trigram2[1]) * 128 + char.ToLower(trigram2[2]);

    FastTrigramFileEnumerable enumerable1 = TextSearchIndex.GetFastTrigramFileEnumerable(key1);
    FastTrigramFileEnumerable enumerable2 = TextSearchIndex.GetFastTrigramFileEnumerable(key2);

    FastIntersectEnumerable<TrigramFileInfo, int> intersection = enumerable1.FastIntersect(enumerable2);

    try
    {
      foreach (TrigramFileInfo tfi in intersection)
      {
        long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
        DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        Console.WriteLine($"Match on FileId = {tfi.FileId} at Position {tfi.Position} : {nameString.GetValue()}");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }

    Pause();
  }
}
