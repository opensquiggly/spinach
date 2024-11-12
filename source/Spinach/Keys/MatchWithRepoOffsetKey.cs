namespace Spinach.Keys;

public class MatchWithRepoOffsetKey : IComparable<MatchWithRepoOffsetKey>, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public MatchWithRepoOffsetKey()
  {
  }

  public MatchWithRepoOffsetKey(ushort userType, uint userId, ushort repoType, uint repoId, long offset, long adjustedOffset)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
    Offset = offset;

    // The AdjustedOffset is useful when we are intersecting trigrams to find string literals. For example,
    // suppose we are intersecting trigram enumerators "for" and "ach" in the hopes of finding candidate
    // documents that contain the literal "foreach".
    //
    // In this case we want to ensure that the "ach" trigram not only appears within the document, but appears
    // at the correct offset relative to "for", thus forming a for/ach trigram pair, both at the correct
    // offsets to form the literal "foreach". To perform such as intersection, we would set the AdjustedOffset
    // for the "for" enumerator key to 0 and set the AdjustedOffset for the "ach" enumerator key to -4.
    //
    // Suppose now that the "for" enumerator key is at Offset=120 and the "ach" enumerator key is at Offset=124.
    // This is a candidate match for the literal "foreach". Thus, we want the equality operator to return true
    // in this case, which it would: 120 + 0 == 124 + (-4).
    AdjustedOffset = adjustedOffset;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public ushort RepoType { get; set; }

  public uint RepoId { get; set; }

  public long Offset { get; set; }

  public long AdjustedOffset { get; set; }

  public long EffectiveOffset => Offset + AdjustedOffset;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Equality and HashCode
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public override bool Equals(object obj)
  {
    if (null == obj || GetType() != obj.GetType())
    {
      return false;
    }

    return this.CompareTo(obj) == 0;
  }

  public override int GetHashCode() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    HashCode.Combine(UserType, UserId, RepoType, RepoId, Offset + AdjustedOffset);

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // IComparable Implementation
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public int CompareTo(MatchWithRepoOffsetKey other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoType < other.RepoType) return -1;
    if (RepoType > other.RepoType) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (EffectiveOffset < other.EffectiveOffset) return -1;
    if (EffectiveOffset > other.EffectiveOffset) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((MatchWithRepoOffsetKey)obj);

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Operator Overloads
  // /////////////////////////////////////////////////////////////////////////////////////////////

  // public static bool operator ==(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right)
  // {
  //   if (null == left) throw new ArgumentNullException(nameof(left));
  //   return left.CompareTo(right) == 0;
  // }

  // public static bool operator !=(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right)
  // {
  //   if (left == null) throw new ArgumentNullException(nameof(left));
  //   return left.CompareTo(right) != 0;
  // }

  public static bool operator <(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right)
  {
    if (null == left) throw new ArgumentNullException(nameof(left));
    return left.CompareTo(right) < 0;
  }

  public static bool operator >(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right)
  {
    if (null == left) throw new ArgumentNullException(nameof(left));
    return left.CompareTo(right) > 0;
  }

  public static bool operator <=(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right)
  {
    if (left == null) throw new ArgumentNullException(nameof(left));
    return left.CompareTo(right) <= 0;
  }

  public static bool operator >=(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right)
  {
    if (left == null) throw new ArgumentNullException(nameof(left));
    return left.CompareTo(right) >= 0;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Copy(MatchWithRepoOffsetKey other)
  {
    UserType = other.UserType;
    UserId = other.UserId;
    RepoType = other.RepoType;
    RepoId = other.RepoId;
    Offset = other.Offset;
    AdjustedOffset = other.AdjustedOffset;
  }

  public MatchWithRepoOffsetKey Dup() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new MatchWithRepoOffsetKey(UserType, UserId, RepoType, RepoId, Offset, AdjustedOffset);

  public MatchWithRepoOffsetKey CreateForNextRepo() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new MatchWithRepoOffsetKey(UserType, UserId, RepoType, RepoId + 1, 0, AdjustedOffset);

  public MatchWithRepoOffsetKey CreateForNextUser() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new MatchWithRepoOffsetKey(UserType, UserId + 1, 0, 0, 0, AdjustedOffset);

  public MatchWithRepoOffsetKey WithAdjustedOffset(long adjustedOffset, ref MatchWithRepoOffsetKey destination)
  {
    destination.UserType = UserType;
    destination.UserId = UserId;
    destination.RepoType = RepoType;
    destination.RepoId = RepoId;
    destination.Offset = Offset;
    destination.AdjustedOffset = adjustedOffset;

    return destination;
  }

  public MatchWithRepoOffsetKey AddOffset(long increaseBy, ref MatchWithRepoOffsetKey destination)
  {
    destination.Offset += increaseBy;
    return destination;
  }

  public MatchWithRepoOffsetKey WithZeroOffsets() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new MatchWithRepoOffsetKey(UserType, UserId, RepoType, RepoId, 0, 0);

  public static bool IsSameUser(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right) =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    left.UserType == right.UserType && left.UserId == right.UserId;

  public static bool IsSameUser(MatchWithRepoOffsetKey left, TrigramMatchKey right) =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    left.UserType == right.UserType && left.UserId == right.UserId;

  public static bool IsSameRepo(MatchWithRepoOffsetKey left, MatchWithRepoOffsetKey right) =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    left.UserType == right.UserType && left.UserId == right.UserId && left.RepoType == right.RepoType && left.RepoId == right.RepoId;

  public static bool IsSameRepo(MatchWithRepoOffsetKey left, TrigramMatchKey right) =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    left.UserType == right.UserType && left.UserId == right.UserId && left.RepoType == right.RepoType && left.RepoId == right.RepoId;
}
