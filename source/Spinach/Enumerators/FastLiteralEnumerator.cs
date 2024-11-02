namespace Spinach.Enumerators;

public class FastLiteralEnumerator : IFastEnumerator<ulong, long>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerator(TextSearchIndex textSearchIndex, string literal)
  {
    TextSearchIndex = textSearchIndex;
    Literal = literal;
    Offset = (ulong)(Literal.Length - 3);
    int trigramKey1 = TrigramHelper.GetLeadingTrigramKey(Literal);
    int trigramKey2 = TrigramHelper.GetTrailingTrigramKey(Literal);
    Enumerable1 = TextSearchIndex.GetFastTrigramEnumerable(trigramKey1);
    Enumerable2 = TextSearchIndex.GetFastTrigramEnumerable(trigramKey2);
    Enumerator1 = Enumerable1.GetFastEnumerator();
    Enumerator2 = Enumerable2.GetFastEnumerator();
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private FastTrigramEnumerable Enumerable1 { get; }

  private FastTrigramEnumerable Enumerable2 { get; }

  private IFastEnumerator<MatchWithRepoOffsetKey, ulong> Enumerator1 { get; }

  private IFastEnumerator<MatchWithRepoOffsetKey, ulong> Enumerator2 { get; }

  private string Literal { get; }

  private ulong Offset { get; }

  private TextSearchIndex TextSearchIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public long Current => CurrentData;

  public long CurrentData { get; private set; }

  public ulong CurrentKey { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext() =>
    // bool hasValue1 = Enumerator1.MoveNext();
    // bool hasValue2 = Enumerator2.MoveNext();
    //
    // while (hasValue1 && hasValue2)
    // {
    //   if (Enumerator1.CurrentKey < Enumerator2.CurrentKey - Offset)
    //   {
    //     hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey - Offset);
    //   }
    //   else if (Enumerator1.CurrentKey > Enumerator2.CurrentKey - Offset)
    //   {
    //     hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey + Offset);
    //   }
    //   else
    //   {
    //     CurrentKey = Enumerator1.CurrentKey;
    //     CurrentData = Enumerator1.CurrentData;
    //     return true;
    //   }
    // }
    //
    // return false;
    throw new NotImplementedException();

  public bool MoveUntilGreaterThanOrEqual(ulong target) =>
    // bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    // bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);
    //
    // while (hasValue1 && hasValue2)
    // {
    //   if (Enumerator1.CurrentKey < Enumerator2.CurrentKey - Offset)
    //   {
    //     hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey - Offset);
    //   }
    //   else if (Enumerator1.CurrentKey > Enumerator2.CurrentKey - Offset)
    //   {
    //     hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey + Offset);
    //   }
    //   else
    //   {
    //     CurrentKey = Enumerator1.CurrentKey;
    //     CurrentData = Enumerator1.CurrentData;
    //     return true;
    //   }
    // }
    //
    // return false;
    throw new NotImplementedException();

  public void Reset()
  {
    Enumerator1.Reset();
    Enumerator2.Reset();
  }
}
