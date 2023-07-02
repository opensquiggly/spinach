namespace Spinach.Helpers;

public struct TrigramFileInfo : IComparable<TrigramFileInfo>
{
  public TrigramFileInfo()
  {
  }

  public TrigramFileInfo(long fileId, long position)
  {
    FileId = fileId;
    Position = position;
  }

  public long FileId { get; set; }
  public long Position { get; set; }

  public int CompareTo(TrigramFileInfo other)
  {
    if (this.FileId < other.FileId) return -1;
    if (this.FileId > other.FileId) return 1;
    // if (this.Position < other.Position) return -1;
    // if (this.Position > other.Position) return 1;

    return 0;
  }

  public bool Equals(TrigramFileInfo other) => this.FileId == other.FileId;
}
