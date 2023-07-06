namespace Spinach.Enumerators;

public class RegexEnumerable : IEnumerable<RegexEnumerable.MatchingFile>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public RegexEnumerable(TextSearchIndex textSearchIndex, string regex)
  {
    Regex = regex;
    TextSearchIndex = textSearchIndex;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private RE2 CompiledRegex { get; set; }

  private string Regex { get; }

  private TextSearchIndex TextSearchIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public IEnumerator<MatchingFile> GetEnumerator()
  {
    Spinach.Regex.Types.NormalizedRegex normalized = RegexParser.Parse(Regex);
    Spinach.Regex.Types.LiteralQueryNode queryNode = LiteralQueryBuilder.BuildQuery(normalized);

    IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int> queryEnumerable =
      LiteralQueryBuilder.BuildEnumerable(TextSearchIndex, queryNode);
    CompiledRegex = RE2.CompileCaseInsensitive(Regex);

    long currentFile = 0;

    foreach (TrigramFileInfo tfi in queryEnumerable)
    {
      if (tfi.FileId > currentFile)
      {
        currentFile = tfi.FileId;
        long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
        DiskImmutableString nameString =
          TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        string fileName = nameString.GetValue();
        string contents = File.ReadAllText(nameString.GetValue());

        if (CompiledRegex.Match(contents))
        {
          yield return new MatchingFile(this, tfi.FileId, fileName, contents);
        }
      }
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Inner Classes
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public class MatchingFile
  {
    public MatchingFile(RegexEnumerable parent, long fileId, string fileName, string contents)
    {
      Parent = parent;
      Contents = contents;
      FileId = fileId;
      FileName = fileName;
    }

    private RegexEnumerable Parent { get; }

    public string Contents { get; }

    public long FileId { get; }

    public string FileName { get; }

    public IEnumerable<MatchingPosition> Matches
    {
      get
      {
        IList<int[]> matches = Parent.CompiledRegex.FindAllIndex(Contents, -1);
        if (matches != null)
        {
          foreach (int[] position in matches)
          {
            yield return new MatchingPosition(position[0], position[1]);
          }
        }
      }
    }
  }

  public class MatchingPosition
  {
    public MatchingPosition(long startIndex, long endIndex)
    {
      StartIndex = startIndex;
      EndIndex = endIndex;
    }

    public long StartIndex { get; }

    public long EndIndex { get; }
  }
}
