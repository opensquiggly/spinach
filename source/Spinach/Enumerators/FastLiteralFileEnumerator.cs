namespace Spinach.Enumerators;

public class FastLiteralFileEnumerator : IFastEnumerator<TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralFileEnumerator(
    TextSearchIndex textSearchIndex,
    InternalFileInfoTable internalFileInfoTable,
    string literal
  )
  {
    TextSearchIndex = textSearchIndex;
    InternalFileInfoTable = internalFileInfoTable;
    Literal = literal;

    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private TextSearchIndex TextSearchIndex { get; }

  private InternalFileInfoTable InternalFileInfoTable { get; }

  private string Literal { get; }

  private FastLiteralEnumerator FastLiteralEnumerator { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public int Current => CurrentData;

  public int CurrentData { get; }

  public TrigramFileInfo CurrentKey { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    bool hasValue = FastLiteralEnumerator.MoveNext();

    if (hasValue)
    {
      (_, InternalFileInfoTable.InternalFileInfo internalFileInfo) =
        InternalFileInfoTable.FindLastWithOffsetLessThanOrEqual(0L, FastLiteralEnumerator.CurrentKey);

      CurrentKey = new TrigramFileInfo(
        internalFileInfo.InternalId,
        (long)(FastLiteralEnumerator.CurrentKey - internalFileInfo.StartingOffset)
      );
    }

    return hasValue;
  }

  public bool MoveUntilGreaterThanOrEqual(TrigramFileInfo target)
  {
    InternalFileInfoTable.InternalFileInfo internalFileInfo =
      InternalFileInfoTable.FindById((ulong)target.FileId);

    ulong offsetTarget = internalFileInfo.StartingOffset + (ulong)target.Position;

    bool hasValue = FastLiteralEnumerator.MoveUntilGreaterThanOrEqual(offsetTarget);

    if (hasValue)
    {
      (_, internalFileInfo) =
        InternalFileInfoTable.FindLastWithOffsetLessThanOrEqual(0L, FastLiteralEnumerator.CurrentKey);

      CurrentKey = new TrigramFileInfo(
        internalFileInfo.InternalId,
        (long)(FastLiteralEnumerator.CurrentKey - internalFileInfo.StartingOffset)
      );
    }

    return hasValue;
  }

  public void Reset()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    FastLiteralEnumerator = new FastLiteralEnumerator(TextSearchIndex, Literal);

    FastLiteralEnumerator.Reset();
  }
}
