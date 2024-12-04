namespace SpinachExplorer;

using Spinach.Keys;
using System.Diagnostics;
using Spinach.Misc;

internal static partial class Program
{
  private static void PrintFilesForLiteralIntersection()
  {
    Console.WriteLine();

    Console.Write("Enter literal 1: ");
    string literal1 = Console.ReadLine();

    Console.Write("Enter literal 2: ");
    string literal2 = Console.ReadLine();

    var enumerable1 = new FastLiteralEnumerable2(literal1, TextSearchManager);
    var enumerable2 = new FastLiteralEnumerable2(literal2, TextSearchManager);
    FastIntersectEnumerable<MatchWithRepoOffsetKey, MatchData> intersection = enumerable1.FastIntersect(enumerable2);

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
