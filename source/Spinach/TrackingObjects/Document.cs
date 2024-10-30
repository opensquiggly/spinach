namespace Spinach.TrackingObjects;

public class Document : IDocument
{
  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort RepoType { get; set; }
  public uint RepoId { get; set; }
  public uint DocId { get; set; }
  public ulong StartingOffset { get; set; }
  public long NameAddress { get; set; }
  public string Name { get; set; }
  public long ExternalIdOrPathAddress { get; set; }
  public string ExternalIdOrPath { get; set; }
}
