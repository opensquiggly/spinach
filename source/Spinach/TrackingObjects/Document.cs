namespace Spinach.TrackingObjects;

public class Document : IDocument
{
  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort RepoType { get; set; }
  public uint RepoId { get; set; }
  public uint DocId { get; set; }
}
