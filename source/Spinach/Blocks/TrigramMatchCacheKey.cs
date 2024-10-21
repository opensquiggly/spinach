namespace Spinach.Blocks;

public struct TrigramMatchCacheKey : IComparable<TrigramMatchCacheKey>, IComparable
{
  public int TrigramKey { get; set; }
  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public uint RepoId { get; set; }

  public int CompareTo(TrigramMatchCacheKey other)
  {
    if (TrigramKey < other.TrigramKey) return -1;
    if (TrigramKey > other.TrigramKey) return 1;
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((TrigramMatchCacheKey) obj);
}
