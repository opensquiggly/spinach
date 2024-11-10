namespace Spinach.Enumerators;

public class FastLiteralEnumerator2 : IFastEnumerator<MatchWithRepoOffsetKey, MatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private MatchWithRepoOffsetKey _tempKey;
  private MatchWithRepoOffsetKey _lastKey;

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
    _tempKey = new MatchWithRepoOffsetKey();
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

  private bool LastDocIsValid { get; set; }

  private uint LastDocId { get; set; }

  private long LastDocStartingOffset { get; set; }

  private long LastDocLength { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public MatchData Current => CurrentData;

  public MatchData CurrentData { get; private set; }

  public MatchWithRepoOffsetKey CurrentKey { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool IsSameDocumentAsLast()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return
      MatchWithRepoOffsetKey.IsSameRepo(Enumerator1.CurrentKey, _lastKey) &&
      Enumerator1.CurrentData.Document.IsValid &&
      LastDocIsValid &&
      Enumerator1.CurrentData.Document.DocId == LastDocId;
  }

  private void SaveLastKeyAndData()
  {
    _lastKey.Copy(Enumerator1.CurrentKey);
    LastDocIsValid = Enumerator1.CurrentData.Document.IsValid;
    LastDocId = Enumerator1.CurrentData.Document.DocId;
    LastDocStartingOffset = (long)Enumerator1.CurrentData.Document.StartingOffset;
    LastDocLength = Enumerator1.CurrentData.Document.Length;
  }

  private bool ConfirmLiteralMatch()
  {
    string targetLiteral = Enumerator1.CurrentData.Document.Content.Substring(
    (int)Enumerator1.CurrentData.MatchPosition,
      Literal.Length
    );

    return string.Equals(targetLiteral, Literal, Context.Options.MatchCase ?
      StringComparison.CurrentCulture :
      StringComparison.CurrentCultureIgnoreCase);
  }

  private bool Intersect(bool hasValue1, bool hasValue2)
  {
    while (hasValue1 && hasValue2)
    {
      if (Enumerator1.CurrentKey < Enumerator2.CurrentKey)
      {
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(Enumerator2.CurrentKey.WithAdjustedOffset(0, ref _tempKey));
      }
      else if (Enumerator1.CurrentKey > Enumerator2.CurrentKey)
      {
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(Enumerator1.CurrentKey.WithAdjustedOffset(AdjustedOffset, ref _tempKey));
      }
      else if (!ConfirmLiteralMatch())
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

  private bool MoveNextInternal()
  {
    bool hasValue1 = Enumerator1.MoveNext();
    bool hasValue2 = Enumerator2.MoveNext();

    return Intersect(hasValue1, hasValue2);
  }

  private bool MoveNext_FirstMatchOnly()
  {
    bool hasValue = MoveNextInternal();

    if (!hasValue) return false;
    if (!IsSameDocumentAsLast())
    {
      SaveLastKeyAndData();
      return true;
    }

    var nextKey = new MatchWithRepoOffsetKey()
    {
      UserType = _lastKey.UserType,
      UserId = _lastKey.UserId,
      RepoType = _lastKey.RepoType,
      RepoId = _lastKey.RepoId,
      Offset = LastDocStartingOffset + LastDocLength,
      AdjustedOffset = _lastKey.AdjustedOffset
    };

    bool result = MoveUntilGreaterThanOrEqualInternal(nextKey);
    SaveLastKeyAndData();

    return result;
  }

  private bool MoveNext_AllMatches() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    MoveNextInternal();

  private bool MoveUntilGreaterThanOrEqualInternal(MatchWithRepoOffsetKey target)
  {
    bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    return Intersect(hasValue1, hasValue2);
  }

  private bool MoveUntilGreaterThanOrEqual_FirstMatchOnly(MatchWithRepoOffsetKey target)
  {
    bool hasValue = MoveUntilGreaterThanOrEqualInternal(target);

    if (!hasValue) return false;
    if (!IsSameDocumentAsLast())
    {
      SaveLastKeyAndData();
      return true;
    }

    var nextKey = new MatchWithRepoOffsetKey()
    {
      UserType = _lastKey.UserType,
      UserId = _lastKey.UserId,
      RepoType = _lastKey.RepoType,
      RepoId = _lastKey.RepoId,
      Offset = LastDocStartingOffset + LastDocLength,
      AdjustedOffset = _lastKey.AdjustedOffset
    };

    bool result = MoveUntilGreaterThanOrEqualInternal(nextKey);
    SaveLastKeyAndData();

    return result;
  }

  private bool MoveUntilGreaterThanOrEqual_AllMatches(MatchWithRepoOffsetKey target) =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    MoveUntilGreaterThanOrEqualInternal(target);

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return Context.Options.DocMatchType switch
    {
      DocMatchType.FirstMatchOnly => MoveNext_FirstMatchOnly(),
      DocMatchType.AllMatchesInDocument => MoveNext_AllMatches(),
      _ => throw new ArgumentOutOfRangeException("Select FirstMatchOnly or AllMatchesInDocument")
    };
  }

  public bool MoveUntilGreaterThanOrEqual(MatchWithRepoOffsetKey target)
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return Context.Options.DocMatchType switch
    {
      DocMatchType.FirstMatchOnly => MoveUntilGreaterThanOrEqual_FirstMatchOnly(target),
      DocMatchType.AllMatchesInDocument => MoveUntilGreaterThanOrEqual_AllMatches(target),
      _ => throw new ArgumentOutOfRangeException("Select FirstMatchOnly or AllMatchesInDocument")
    };
  }

  public void Reset()
  {
    _lastKey = new MatchWithRepoOffsetKey();
    LastDocIsValid = false;
    LastDocId = 0;
    LastDocStartingOffset = 0;
    LastDocLength = 0;

    Enumerator1.Reset();
    Enumerator2.Reset();
  }
}
