namespace SpinachExplorer;

internal static partial class Program
{
  private static void FindFiles()
  {
    Console.WriteLine();
    Console.Write("Enter regex: ");
    string regex = Console.ReadLine();

    Spinach.Regex.Types.NormalizedRegex normalized = RegexParser.Parse(regex);
    Spinach.Regex.Types.LiteralQueryNode queryNode = LiteralQueryBuilder.BuildQuery(normalized);

    IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int> queryEnumerable = LiteralQueryBuilder.BuildEnumerable(TextSearchIndex, queryNode);
    var compiled = RE2.CompileCaseInsensitive(regex);

    try
    {
      long currentFile = 0;

      foreach (TrigramFileInfo tfi in queryEnumerable)
      {
        if (tfi.FileId > currentFile)
        {
          currentFile = tfi.FileId;
          long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
          DiskImmutableString nameString =
            TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
          string text = File.ReadAllText(nameString.GetValue());
          IList<int[]> matches = compiled.FindAllIndex(text, 10);
          if (matches != null)
          {
            Console.WriteLine($"{nameString.GetValue()}");
            foreach (int[] position in matches)
            {
              Console.WriteLine($"  From {position[0]} to {position[1]}");
            }
          }
        }
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }

    Pause();
  }
}
