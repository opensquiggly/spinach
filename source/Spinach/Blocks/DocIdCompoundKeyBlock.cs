namespace Spinach.Blocks;

public struct DocIdCompoundKeyBlock : IComparable<DocIdCompoundKeyBlock>, IComparable
{
  public DocIdCompoundKeyBlock()
  {
  }

  public DocIdCompoundKeyBlock(ushort userType, uint userId, ushort repoType, uint repoId, uint id)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
    Id = id;
  }

  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort RepoType { get; set; }
  public uint RepoId { get; set; }
  public uint Id { get; set; }

  public int CompareTo(DocIdCompoundKeyBlock other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoType < other.RepoType) return -1;
    if (RepoType > other.RepoType) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (Id < other.Id) return -1;
    if (Id > other.Id) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((DocIdCompoundKeyBlock) obj);
}
