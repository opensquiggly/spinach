namespace Spinach.Keys;

public class MatchWithDocIdKey : IComparable<MatchWithDocIdKey>, IComparable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public MatchWithDocIdKey()
  {
  }

  public MatchWithDocIdKey(ushort userType, uint userId, ushort repoType, uint repoId, uint docId)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
    DocId = docId;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public ushort RepoType { get; set; }

  public uint RepoId { get; set; }

  public uint DocId { get; set; }

  public int CompareTo(MatchWithDocIdKey other)
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

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((MatchWithDocIdKey)obj);

  public override bool Equals(object obj)
  {
    if (obj == null || GetType() != obj.GetType())
    {
      return false;
    }

    return this.CompareTo(obj) == 0;
  }

  public override int GetHashCode() => HashCode.Combine(UserType, UserId, RepoType, RepoId, DocId);

  public static bool operator ==(MatchWithDocIdKey left, MatchWithDocIdKey right) => left != null && left.CompareTo(right) == 0;

  public static bool operator !=(MatchWithDocIdKey left, MatchWithDocIdKey right) => left != null && left.CompareTo(right) != 0;

  public static bool operator <(MatchWithDocIdKey left, MatchWithDocIdKey right) => left != null && left.CompareTo(right) < 0;

  public static bool operator >(MatchWithDocIdKey left, MatchWithDocIdKey right) => left != null && left.CompareTo(right) > 0;

  public static bool operator <=(MatchWithDocIdKey left, MatchWithDocIdKey right) => left != null && left.CompareTo(right) <= 0;

  public static bool operator >=(MatchWithDocIdKey left, MatchWithDocIdKey right) => left != null && left.CompareTo(right) >= 0;

  public MatchWithDocIdKey Dup() => new(UserType, UserId, RepoType, RepoId, DocId);

  public void Copy(MatchWithDocIdKey other)
  {
    UserType = other.UserType;
    UserId = other.UserId;
    RepoType = other.RepoType;
    RepoId = other.RepoId;
    DocId = other.DocId;
  }

  public MatchWithDocIdKey CreateForNextRepo() => new MatchWithDocIdKey(UserType, UserId, RepoType, RepoId + 1, 0);

  public MatchWithDocIdKey CreateForNextUser() => new MatchWithDocIdKey(UserType, UserId + 1, 0, 0, 0);

  public MatchWithDocIdKey CreateForNextDoc() => new MatchWithDocIdKey(UserType, UserId, RepoType, RepoId, DocId + 1);
}
