namespace Spinach.Keys;

public class MatchWithDocIdAndOffsetKey : IComparable<MatchWithDocIdAndOffsetKey>, IComparable
{
  public MatchWithDocIdAndOffsetKey()
  {
  }

  public MatchWithDocIdAndOffsetKey(ushort userType, uint userId, ushort repoType, uint repoId, uint docId, ulong position)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
    DocId = docId;
    Position = position;
  }

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public ushort RepoType { get; set; }

  public uint RepoId { get; set; }

  public uint DocId { get; set; }

  public ulong Position { get; set; }

  public int CompareTo(MatchWithDocIdAndOffsetKey other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoType < other.RepoType) return -1;
    if (RepoType > other.RepoType) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (DocId < other.DocId) return -1;
    if (DocId > other.DocId) return 1;
    if (Position < other.Position) return -1;
    if (Position > other.Position) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((MatchWithDocIdAndOffsetKey)obj);

  // All operator overloads
  public static bool operator <(MatchWithDocIdAndOffsetKey left, MatchWithDocIdAndOffsetKey right) => left.CompareTo(right) < 0;

  public static bool operator >(MatchWithDocIdAndOffsetKey left, MatchWithDocIdAndOffsetKey right) => left.CompareTo(right) > 0;

  public static bool operator <=(MatchWithDocIdAndOffsetKey left, MatchWithDocIdAndOffsetKey right) => left.CompareTo(right) <= 0;

  public static bool operator >=(MatchWithDocIdAndOffsetKey left, MatchWithDocIdAndOffsetKey right) => left.CompareTo(right) >= 0;

  public static bool operator ==(MatchWithDocIdAndOffsetKey left, MatchWithDocIdAndOffsetKey right) => left.CompareTo(right) == 0;

  public static bool operator !=(MatchWithDocIdAndOffsetKey left, MatchWithDocIdAndOffsetKey right) => left.CompareTo(right) != 0;

  public override bool Equals(object obj)
  {
    if (obj == null || GetType() != obj.GetType())
    {
      return false;
    }

    return this.CompareTo(obj) == 0;
  }

  public override int GetHashCode() => HashCode.Combine(UserType, UserId, RepoType, RepoId, DocId, Position);

  public MatchWithDocIdAndOffsetKey Dup() => new(UserType, UserId, RepoType, RepoId, DocId, Position);

  public void Copy(MatchWithDocIdAndOffsetKey other)
  {
    UserType = other.UserType;
    UserId = other.UserId;
    RepoType = other.RepoType;
    RepoId = other.RepoId;
    DocId = other.DocId;
    Position = other.Position;
  }

  public MatchWithDocIdAndOffsetKey CreateForNextRepo() => new MatchWithDocIdAndOffsetKey(UserType, UserId, RepoType, RepoId + 1, 0, 0);

  public MatchWithDocIdAndOffsetKey CreateForNextUser() => new MatchWithDocIdAndOffsetKey(UserType, UserId + 1, RepoType, 0, 0, 0);

  public MatchWithDocIdAndOffsetKey CreateForNextDoc() => new MatchWithDocIdAndOffsetKey(UserType, UserId, RepoType, RepoId, DocId + 1, 0);

  public MatchWithDocIdKey ToMatchWithDocIdKey() => new(UserType, UserId, RepoType, RepoId, DocId);
}
