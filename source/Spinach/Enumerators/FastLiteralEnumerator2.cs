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
    Offset = Literal.Length - 3;
    Enumerable1 = new FastTrigramEnumerable2(Literal.Substring(0, 3), context);
    Enumerable2 = new FastTrigramEnumerable2(Literal.Substring(Literal.Length - 3, 3), context, Offset);
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

  private int Offset { get; }

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
    bool hasValue1 = Enumerator1.MoveNext();
    bool hasValue2 = Enumerator2.MoveNext();

    while (hasValue1 && hasValue2)
    {
      long adjustedOffset = ((long)Enumerator2.CurrentKey.Offset) - ((long)Offset);
      var adjustedKey = new MatchWithRepoOffsetKey()
      {
        UserType = Enumerator2.CurrentKey.UserType,
        UserId = Enumerator2.CurrentKey.UserId,
        RepoType = Enumerator2.CurrentKey.RepoType,
        RepoId = Enumerator2.CurrentKey.RepoId,
        Offset = Math.Max(adjustedOffset, 0)
      };

      if (Enumerator1.CurrentKey.CompareTo(adjustedKey) < 0 && adjustedOffset >= 0)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(adjustedKey);
      }
      else if (Enumerator1.CurrentKey.CompareTo(adjustedKey) > 0 || adjustedOffset < 0)
      {
        adjustedKey.UserType = Enumerator1.CurrentKey.UserType;
        adjustedKey.UserId = Enumerator1.CurrentKey.UserId;
        adjustedKey.RepoType = Enumerator1.CurrentKey.RepoType;
        adjustedKey.RepoId = Enumerator1.CurrentKey.RepoId;
        adjustedKey.Offset = Enumerator1.CurrentKey.Offset + (long)Offset;

        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(adjustedKey);
      }
      else if (!string.Equals(Enumerator1.CurrentData.Document.Content.Substring((int) Enumerator1.CurrentData.MatchPosition, Literal.Length), Literal, StringComparison.CurrentCultureIgnoreCase))
      {
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
    bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    while (hasValue1 && hasValue2)
    {
      long adjustedOffset = ((long)Enumerator2.CurrentKey.Offset) - ((long)Offset);
      var adjustedKey = new MatchWithRepoOffsetKey()
      {
        UserType = Enumerator2.CurrentKey.UserType,
        UserId = Enumerator2.CurrentKey.UserId,
        RepoType = Enumerator2.CurrentKey.RepoType,
        RepoId = Enumerator2.CurrentKey.RepoId,
        Offset = Math.Max(adjustedOffset, 0)
      };

      if (Enumerator1.CurrentKey.CompareTo(adjustedKey) < 0 && adjustedOffset >= 0)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(adjustedKey);
      }
      else if (Enumerator1.CurrentKey.CompareTo(adjustedKey) > 0 || adjustedOffset < 0)
      {
        adjustedKey.UserType = Enumerator1.CurrentKey.UserType;
        adjustedKey.UserId = Enumerator1.CurrentKey.UserId;
        adjustedKey.RepoType = Enumerator1.CurrentKey.RepoType;
        adjustedKey.RepoId = Enumerator1.CurrentKey.RepoId;
        adjustedKey.Offset = Enumerator1.CurrentKey.Offset + (long)Offset;

        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(adjustedKey);
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
