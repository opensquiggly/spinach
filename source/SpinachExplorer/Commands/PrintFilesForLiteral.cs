namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintLiteralsForTrigram()
  {
    Console.WriteLine();

    Console.Write("Enter string literal: ");
    string literal = Console.ReadLine();

    FastLiteralEnumerable enumerable2 = TextSearchIndex.GetFastLiteralEnumerable(literal);
    foreach (TrigramFileInfo tfi in enumerable2)
    {
      long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
      DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
      Console.WriteLine($"FileId = {tfi.FileId}, Position = {tfi.Position} : {nameString.GetValue()}");
    }

    Pause();
  }
}
