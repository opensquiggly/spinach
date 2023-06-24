namespace SpinachExplorer;

public struct TrigramKey : IComparable<TrigramKey>, IComparable
{
  public int Key { get; set; }
  public long FileId { get; set; }
  public long Position { get; set; }

  public int CompareTo(TrigramKey other)
  {
    if (Key < other.Key)
    {
      return -1;
    }

    if (Key > other.Key)
    {
      return 1;
    }

    if (FileId < other.FileId)
    {
      return -1;
    }

    if (FileId > other.FileId)
    {
      return 1;
    }

    if (Position < other.Position)
    {
      return -1;
    }

    if (Position > other.Position)
    {
      return 1;
    }

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((TrigramKey)obj);
}
