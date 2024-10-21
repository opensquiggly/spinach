namespace Spinach.Blocks;

public struct FileInfoKey : IComparable<FileInfoKey>, IComparable
{
  public FileInfoKey()
  {
  }

  public FileInfoKey(ushort userType, uint userId, uint repoId, ulong fileId)
  {
    UserType = userType;
    UserId = userId;
    RepoId = repoId;
    FileId = fileId;
  }

  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public uint RepoId { get; set; }
  public ulong FileId { get; set; }

  public int CompareTo(FileInfoKey other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (FileId < other.FileId) return -1;
    if (FileId > other.FileId) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((FileInfoKey) obj);
}
