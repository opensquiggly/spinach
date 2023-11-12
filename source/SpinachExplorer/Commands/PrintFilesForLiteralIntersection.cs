namespace SpinachExplorer;

using System.Diagnostics;

internal static partial class Program
{
  private static void PrintFilesForLiteralIntersection()
  {
    Console.WriteLine();

    Console.Write("Enter literal 1: ");
    string literal1 = Console.ReadLine();

    Console.Write("Enter literal 2: ");
    string literal2 = Console.ReadLine();

    FastLiteralFileEnumerable enumerable1 = TextSearchIndex.GetFastLiteralFileEnumerable(literal1);
    FastLiteralFileEnumerable enumerable2 = TextSearchIndex.GetFastLiteralFileEnumerable(literal2);

    FastIntersectEnumerable<TrigramFileInfo, int> intersection =
      enumerable1.FastIntersect(enumerable2);

    var stopwatch = Stopwatch.StartNew();
    int count = intersection.Count();
    stopwatch.Stop();

    // foreach (TrigramFileInfo tfi in intersection)
    // {
    //   // Console.WriteLine(fileOffset);
    //   // long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
    //   // DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
    //   // Console.WriteLine($"Match on FileId = {tfi.FileId} at Position {tfi.Position} : {nameString.GetValue()}");
    // }

    Console.WriteLine($"Found {count} matches in {stopwatch.ElapsedMilliseconds} milliseconds");

    Pause();
  }
}
