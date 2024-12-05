namespace SpinachExplorer;

using System.Diagnostics;

internal static partial class Program
{
  private static void PrintFilesForTrigramIntersection()
  {
    Console.WriteLine();

    Console.Write("Enter trigram 1: ");
    string trigram1 = Console.ReadLine();

    Console.Write("Enter trigram 2: ");
    string trigram2 = Console.ReadLine();

    int key1 = TrigramHelper.GetTrigramKey(trigram1);
    int key2 = TrigramHelper.GetTrigramKey(trigram2);

    // FastTrigramFileEnumerable enumerable1 = TextSearchIndex.GetFastTrigramFileEnumerable(key1);
    // FastTrigramFileEnumerable enumerable2 = TextSearchIndex.GetFastTrigramFileEnumerable(key2);
    var enumerable1 = new FastTrigramEnumerable2(trigram1, TextSearchManager);
    var enumerable2 = new FastTrigramEnumerable2(trigram2, TextSearchManager);

    // FastIntersectEnumerable<TrigramFileInfo, int> intersection = enumerable1.FastIntersect(enumerable2);
    FastIntersectEnumerable<Spinach.Keys.MatchWithRepoOffsetKey, Spinach.Misc.MatchData> intersection = enumerable1.FastIntersect(enumerable2);

    // FastTrigramEnumerable enumerable1 = TextSearchIndex.GetFastTrigramEnumerable(key1);
    // FastTrigramEnumerable enumerable2 = TextSearchIndex.GetFastTrigramEnumerable(key2);
    // FastIntersectEnumerable<ulong, long> intersection = enumerable1.FastIntersect(enumerable2);

    try
    {
      // Add timer here

      var stopwatch = Stopwatch.StartNew();
      int count = intersection.Count();
      stopwatch.Stop();

      // foreach (TrigramFileInfo tfi in intersection)
      // {
      //   // long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
      //   // DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
      //   // Console.WriteLine($"Match on FileId = {tfi.FileId} at Position {tfi.Position} : {nameString.GetValue()}");
      //
      //   // Console.WriteLine($"Match at position {tfi.Position + 1} in file {nameString}");
      //   // Console.WriteLine("------------------------------------");
      //   // FileUtils.PrintFile(nameString.GetValue(), (int) tfi.Position + 1, 3);
      // }

      Console.WriteLine($"Found {count} matches in {stopwatch.ElapsedMilliseconds} milliseconds");

      // foreach (ulong fileOffset in intersection)
      // {
      //   (_, InternalFileInfoTable.InternalFileInfo fileInfo) =
      //     TextSearchIndex.InternalFileInfoTable.FindWithLastOffsetLessThanOrEqual(0, fileOffset);
      //
      //   string header = $"Match at position {fileOffset - fileInfo.StartingOffset + 1} in file {fileInfo.Name}";
      //   Console.WriteLine(header);
      //   Console.WriteLine(new string('-', header.Length));
      //
      //   FileUtils.PrintFile(fileInfo.Name, (int) fileOffset - (int)fileInfo.StartingOffset + 1, 3);
      //
      //   Console.WriteLine();
      // }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }

    Pause();
  }
}
