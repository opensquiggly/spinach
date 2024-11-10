namespace Spinach.Enumerators;

using Misc;

public class FastLiteralEnumerator2 : IFastEnumerator<MatchWithRepoOffsetKey, MatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerator2(string literal, ITextSearchEnumeratorContext context)
  {
    Literal = literal;
    Context = context;
    AdjustedOffset = 3 - Literal.Length;
    Enumerable1 = new FastTrigramEnumerable2(Literal.Substring(0, 3), context);
    Enumerable2 = new FastTrigramEnumerable2(Literal.Substring(Literal.Length - 3, 3), context, AdjustedOffset);
    Enumerator1 = Enumerable1.GetFastEnumerator();
    Enumerator2 = Enumerable2.GetFastEnumerator();
    CurrentKey = new MatchWithRepoOffsetKey();
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private ITextSearchEnumeratorContext Context { get; }

  private FastTrigramEnumerable2 Enumerable1 { get; }

  private FastTrigramEnumerable2 Enumerable2 { get; }

  private IFastEnumerator<MatchWithRepoOffsetKey, MatchData> Enumerator1 { get; }

  private IFastEnumerator<MatchWithRepoOffsetKey, MatchData> Enumerator2 { get; }

  private string Literal { get; }

  private int AdjustedOffset { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public MatchData Current => CurrentData;

  public MatchData CurrentData { get; private set; }

  public MatchWithRepoOffsetKey CurrentKey { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    MatchWithRepoOffsetKey lastKey = Enumerator1.CurrentKey.Dup();

    var lastData = new MatchData()
    {
      Document = Enumerator1.CurrentData.Document,
      MatchPosition = Enumerator1.CurrentData.MatchPosition
    };

    bool hasValue1 = Enumerator1.MoveNext();
    bool hasValue2 = Enumerator2.MoveNext();

    while (hasValue1 && hasValue2)
    {
      if (Enumerator1.CurrentKey < Enumerator2.CurrentKey)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey.ToZeroAdjustedOffset());
      }
      else if (Enumerator1.CurrentKey > Enumerator2.CurrentKey)
      {
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey.WithAdjustedOffset(AdjustedOffset));
      }
      else if (!string.Equals(Enumerator1.CurrentData.Document.Content.Substring((int)Enumerator1.CurrentData.MatchPosition, Literal.Length), Literal, StringComparison.CurrentCultureIgnoreCase))
      {
        hasValue1 = Enumerator1.MoveNext();
        hasValue2 = Enumerator2.MoveNext();
      }
      else if (Context.Options.DocMatchType == DocMatchType.FirstMatchOnly && lastKey.CompareTo(Enumerator1.CurrentKey) == 0 && lastData.MatchPosition == Enumerator1.CurrentData.MatchPosition)
      {
        // Move to next greater than or equal to last key + 1
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(new MatchWithRepoOffsetKey()
        {
          UserType = lastKey.UserType,
          UserId = lastKey.UserId,
          RepoType = lastKey.RepoType,
          RepoId = lastKey.RepoId,
          Offset = lastKey.Offset + 1
        });

        hasValue1 = Enumerator1.MoveNext();
        hasValue2 = Enumerator2.MoveNext();
      }
      else
      {
        CurrentKey = Enumerator1.CurrentKey;
        CurrentData = Enumerator1.CurrentData;
        return true;
      }
    }

    return false;
  }

  public bool MoveUntilGreaterThanOrEqual(MatchWithRepoOffsetKey target)
  {
    MatchWithRepoOffsetKey lastKey = Enumerator1.CurrentKey.Dup();

    var lastData = new MatchData()
    {
      Document = Enumerator1.CurrentData.Document,
      MatchPosition = Enumerator1.CurrentData.MatchPosition
    };

    bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    while (hasValue1 && hasValue2)
    {
      if (Enumerator1.CurrentKey < Enumerator2.CurrentKey)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey.ToZeroAdjustedOffset());
      }
      else if (Enumerator1.CurrentKey > Enumerator2.CurrentKey)
      {
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey.WithAdjustedOffset(AdjustedOffset));
      }
      else if (Context.Options.DocMatchType == DocMatchType.FirstMatchOnly && lastKey.CompareTo(Enumerator1.CurrentKey) == 0 && lastData.MatchPosition == Enumerator1.CurrentData.MatchPosition)
      {
        // Move to next greater than or equal to last key + 1
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(new MatchWithRepoOffsetKey()
        {
          UserType = lastKey.UserType,
          UserId = lastKey.UserId,
          RepoType = lastKey.RepoType,
          RepoId = lastKey.RepoId,
          Offset = lastKey.Offset + 1
        });

        hasValue1 = Enumerator1.MoveNext();
        hasValue2 = Enumerator2.MoveNext();
      }
      else
      {
        CurrentKey = Enumerator1.CurrentKey;
        CurrentData = Enumerator1.CurrentData;
        return true;
      }
    }

    return false;
  }

  public void Reset()
  {
    Enumerator1.Reset();
    Enumerator2.Reset();
  }
}
