namespace Spinach.Enumerators;

public class FastLiteralFileEnumerable : IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralFileEnumerable(
    TextSearchIndex textSearchIndex,
    InternalFileInfoTable internalFileInfoTable,
    string literal
  )
  {
    TextSearchIndex = textSearchIndex;
    InternalFileInfoTable = internalFileInfoTable;
    Literal = literal;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private TextSearchIndex TextSearchIndex { get; }

  private InternalFileInfoTable InternalFileInfoTable { get; }

  private string Literal { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<int> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramFileInfo, int> GetFastEnumerator() =>
    new FastLiteralFileEnumerator(
      TextSearchIndex,
      InternalFileInfoTable,
      Literal
    );
}
