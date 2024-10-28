namespace SpinachExplorer;


internal static partial class Program
{
  private static void PrintFilesForTrigram()
  {
    Console.WriteLine();
    Console.Write("Enter trigram: ");
    string trigram = Console.ReadLine();

    int key = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);

    var trigramFound = TextSearchIndex.TrigramTree.TryFind(key, out long trigramMatchesAddress, out _, out _);
    Console.WriteLine($"Found = {trigramFound}");
    Console.WriteLine($"Address = {trigramMatchesAddress}");
    var trigramMatches = TextSearchIndex.TrigramMatchesFactory.LoadExisting(trigramMatchesAddress);
    var cursor = new DiskBTreeCursor<TrigramMatchKey, long>(trigramMatches);

    while (cursor.MoveNext())
    {
      Console.WriteLine($"User Type = {cursor.CurrentKey.UserType}, User Id = {cursor.CurrentKey.UserId}, RepoId = {cursor.CurrentKey.RepoId}");
      var postingsList = TextSearchIndex.DiskBlockManager.SortedVarIntListFactory.LoadExisting(cursor.CurrentData);
      var postingsListCursor = new DiskSortedVarIntListCursor(postingsList);
      while (postingsListCursor.MoveNext())
      {
        Console.WriteLine($"  Offset = {postingsListCursor.CurrentKey}");
      }
    }

    // Console.WriteLine($"key = {key}");
    // DiskSortedVarIntList postingsList = TextSearchIndex.LoadOrAddTrigramPostingsList(key, out bool _);
    //
    // int count = 0;
    // int index = 0;
    // var cursor = new DiskSortedVarIntListCursor(postingsList);
    //
    // while (cursor.MoveNext())
    // {
    //   InternalFileInfoTable.InternalFileInfo fileInfo;
    //   ulong startingIndex = 0;
    //
    //   ulong fileOffset = (ulong) (cursor.Current ?? 0L);
    //   (startingIndex, fileInfo) = TextSearchIndex.InternalFileInfoTable.FindWithLastOffsetLessThanOrEqual(startingIndex, fileOffset);
    //
    //   if (!fileInfo.Name.EndsWith(".cs") && !fileInfo.Name.EndsWith(".md") && !fileInfo.Name.EndsWith(".js") && !fileInfo.Name.EndsWith(".ps1"))
    //   {
    //     count++;
    //     // index++;
    //     continue;
    //   }
    //
    //   Console.WriteLine($"Match at position {fileOffset - fileInfo.StartingOffset + 1} in file {fileInfo.Name}");
    //   Console.WriteLine("------------------------------------");
    //   if (fileOffset < fileInfo.StartingOffset || fileOffset > fileInfo.StartingOffset + (ulong)fileInfo.Length)
    //   {
    //     Console.WriteLine("  ERROR: File offset is outside of file range");
    //   }
    //   FileUtils.PrintFile(fileInfo.Name, (int) fileOffset - (int)fileInfo.StartingOffset + 1, 3);
    //   Console.WriteLine();
    //
    //   // Console.WriteLine($"  Name = {fileInfo.Name}");
    //   // Console.WriteLine($"  InternalId = {fileInfo.InternalId}");
    //   // Console.WriteLine($"  StartingOffset = {fileInfo.StartingOffset}");
    //   // Console.WriteLine($"  Length = {fileInfo.Length}");
    //   count++;
    //   index++;
    // }
    //
    // Console.WriteLine();
    // Console.WriteLine($"Total results found = {count}");

    // FastTrigramEnumerable enumerable = TextSearchIndex.GetFastTrigramEnumerable(key);

    // var stopwatch = Stopwatch.StartNew();
    // int count = enumerable.Count();
    // stopwatch.Stop();

    // foreach (ulong fileOffset in enumerable)
    // {
    //   (_, InternalFileInfoTable.InternalFileInfo fileInfo) =
    //     TextSearchIndex.InternalFileInfoTable.FindWithLastOffsetLessThanOrEqual(0, fileOffset);
    //
    //   string header = $"Match at position {fileOffset - fileInfo.StartingOffset + 1} in file {fileInfo.Name}";
    //   Console.WriteLine(header);
    //   Console.WriteLine(new string('-', header.Length));
    //   FileUtils.PrintFile(fileInfo.Name, (int) fileOffset - (int)fileInfo.StartingOffset + 1, 3);
    //   Console.WriteLine();
    // }

    // Console.WriteLine($"Found {count} matches in {stopwatch.ElapsedMilliseconds} milliseconds");

    // FastTrigramFileEnumerable enumerable = TextSearchIndex.GetFastTrigramFileEnumerable(key);
    //
    // foreach (TrigramFileInfo tfi in enumerable)
    // {
    //   long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
    //   DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
    //   // Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position} : {nameString.GetValue()}");
    //
    //   Console.WriteLine($"Match at position {tfi.Position + 1} in file {nameString}");
    //   Console.WriteLine("------------------------------------");
    //   FileUtils.PrintFile(nameString.GetValue(), (int) tfi.Position + 1, 3);
    // }

    Pause();
  }
}
