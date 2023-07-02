namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForLiteralIntersection()
  {
    Console.WriteLine();

    Console.Write("Enter literal 1: ");
    string literal1 = Console.ReadLine();

    Console.Write("Enter literal 2: ");
    string literal2 = Console.ReadLine();

    FastLiteralEnumerable enumerable1 = TextSearchIndex.GetFastLiteralEnumerable(literal1);
    FastLiteralEnumerable enumerable2 = TextSearchIndex.GetFastLiteralEnumerable(literal2);

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
