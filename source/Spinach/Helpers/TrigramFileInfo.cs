namespace Spinach.Helpers;

public readonly struct TrigramFileInfo : IComparable<TrigramFileInfo>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Costructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TrigramFileInfo()
  {
  }

  public TrigramFileInfo(ulong fileId, long position)
  {
    FileId = (long)fileId;
    Position = position;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long FileId { get; init; }

  public long Position { get; init; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public int CompareTo(TrigramFileInfo other)
  {
    if (this.FileId < other.FileId) return -1;
    if (this.FileId > other.FileId) return 1;

    return 0;
  }

  public bool Equals(TrigramFileInfo other) => this.FileId == other.FileId;
}
