namespace SpinachExplorer;

using Spinach.Utils;
using System.Diagnostics;

internal static partial class Program
{
  private static void PrintLiteralsForTrigram()
  {
    Console.WriteLine();

    Console.Write("Enter string literal: ");
    string literal = Console.ReadLine();
    Console.WriteLine();

    FastLiteralEnumerable enumerable = TextSearchIndex.GetFastLiteralEnumerable(literal);

    var stopwatch = Stopwatch.StartNew();
    int count = enumerable.Count();
    stopwatch.Stop();

    // foreach (ulong fileOffset in enumerable)
    // {
    //   (_, InternalFileInfoTable.InternalFileInfo fileInfo) =
    //     TextSearchIndex.InternalFileInfoTable.FindWithLastOffsetLessThanOrEqual(0, fileOffset);
    //
    //   string header = $"Match at position {fileOffset - fileInfo.StartingOffset + 1} in file {fileInfo.Name}";
    //   Console.WriteLine(header);
    //   Console.WriteLine(new string('-', header.Length));
    //
    //   FileUtils.PrintFile(fileInfo.Name, (int) fileOffset - (int)fileInfo.StartingOffset + 1, literal.Length);
    //
    //   Console.WriteLine();
    // }

    Console.WriteLine($"Found {count} matches in {stopwatch.ElapsedMilliseconds} milliseconds");

    // foreach (ulong fileOffset in TextSearchIndex.GetFastLiteralFileEnumerable(literal))
    // {
    //   Console.WriteLine(fileOffset);
    //   long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
    //   DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
    //   Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position} : {nameString.GetValue()}");
    // }

    Pause();
  }
}
