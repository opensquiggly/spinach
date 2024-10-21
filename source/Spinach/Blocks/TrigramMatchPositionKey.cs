namespace Spinach.Blocks;

public class TrigramMatchPositionKey : IComparable<TrigramMatchPositionKey>, IComparable
{
  public TrigramMatchPositionKey()
  {
  }

  public TrigramMatchPositionKey(ushort userType, uint userId, uint repoId, ulong offset)
  {
    UserType = userType;
    UserId = userId;
    RepoId = repoId;
    Offset = offset;
  }

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public uint RepoId { get; set; }

  public ulong Offset { get; set; }

  public int CompareTo(TrigramMatchPositionKey other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (Offset < other.Offset) return -1;
    if (Offset > other.Offset) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((TrigramMatchPositionKey) obj);
}
