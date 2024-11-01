namespace Spinach.Enumerators;

using Misc;

public class FastLiteralEnumerator2 : IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerator2(string literal, ITextSearchEnumeratorContext context)
  {
    Literal = literal;
    Context = context;
    Offset = (ulong)(Literal.Length - 3);
    Enumerable1 = new FastTrigramEnumerable2(Literal.Substring(0, 3), context);
    Enumerable2 = new FastTrigramEnumerable2(Literal.Substring(Literal.Length - 3, 3), context);
    Enumerator1 = Enumerable1.GetFastEnumerator();
    Enumerator2 = Enumerable2.GetFastEnumerator();
    CurrentKey = new TrigramMatchPositionKey();
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private ITextSearchEnumeratorContext Context { get; }

  private FastTrigramEnumerable2 Enumerable1 { get; }

  private FastTrigramEnumerable2 Enumerable2 { get; }

  private IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData> Enumerator1 { get; }

  private IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData> Enumerator2 { get; }

  private string Literal { get; }

  private ulong Offset { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public TextSearchMatchData Current => CurrentData;

  public TextSearchMatchData CurrentData { get; private set; }

  public TrigramMatchPositionKey CurrentKey { get; private set; }

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
      var adjustedKey = new TrigramMatchPositionKey()
      {
        UserType = Enumerator2.CurrentKey.UserType,
        UserId = Enumerator2.CurrentKey.UserId,
        RepoType = Enumerator2.CurrentKey.RepoType,
        RepoId = Enumerator2.CurrentKey.RepoId,
        Offset = (ulong) Math.Max(adjustedOffset, 0)
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
        adjustedKey.Offset = Enumerator1.CurrentKey.Offset + Offset;

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

  public bool MoveUntilGreaterThanOrEqual(TrigramMatchPositionKey target)
  {
    bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    while (hasValue1 && hasValue2)
    {
      long adjustedOffset = ((long)Enumerator2.CurrentKey.Offset) - ((long)Offset);
      var adjustedKey = new TrigramMatchPositionKey()
      {
        UserType = Enumerator2.CurrentKey.UserType,
        UserId = Enumerator2.CurrentKey.UserId,
        RepoType = Enumerator2.CurrentKey.RepoType,
        RepoId = Enumerator2.CurrentKey.RepoId,
        Offset = (ulong) Math.Max(adjustedOffset, 0)
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
        adjustedKey.Offset = Enumerator1.CurrentKey.Offset + Offset;

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
