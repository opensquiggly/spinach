namespace Spinach.Blocks;

public class MatchWithDocIdKey : IComparable<MatchWithDocIdKey>, IComparable
{
  public MatchWithDocIdKey()
  {
  }

  public MatchWithDocIdKey(ushort userType, uint userId, ushort repoType, uint repoId, uint docId, ulong position)
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
    if (Position < other.Position) return -1;
    if (Position > other.Position) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((MatchWithDocIdKey) obj);
}
