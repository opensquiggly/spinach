namespace Spinach.Blocks;

public struct TrigramMatchKey : IComparable<TrigramMatchKey>, IComparable
{
  public TrigramMatchKey()
  {
  }

  public TrigramMatchKey(ushort userType, uint userId, uint repoId)
  {
    UserType = userType;
    UserId = userId;
    RepoId = repoId;
  }

  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public uint RepoId { get; set; }

  public int CompareTo(TrigramMatchKey other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((TrigramMatchKey) obj);
}