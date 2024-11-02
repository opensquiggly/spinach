namespace Spinach.Blocks;

public class MatchWithRepoOffsetKey : IComparable<MatchWithRepoOffsetKey>, IComparable
{
  public MatchWithRepoOffsetKey()
  {
  }

  public MatchWithRepoOffsetKey(ushort userType, uint userId, ushort repoType, uint repoId, long offset)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
    Offset = offset;
  }

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public ushort RepoType { get; set; }

  public uint RepoId { get; set; }

  public long Offset { get; set; }

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
    if (Offset < other.Offset) return -1;
    if (Offset > other.Offset) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((MatchWithRepoOffsetKey) obj);
}
