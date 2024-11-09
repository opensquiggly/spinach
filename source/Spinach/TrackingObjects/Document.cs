namespace Spinach.TrackingObjects;

public class Document : IDocument
{
  static Document()
  {
    InvalidDocument = new Document()
    {
      IsValid = false,
      UserType = 0,
      UserId = 0,
      RepoType = 0,
      RepoId = 0,
      DocId = 0,
      Length = 0,
      StartingOffset = 0,
      NameAddress = 0,
      Name = "Invalid Document",
      ExternalIdOrPathAddress = 0,
      ExternalIdOrPath = "invalid-external-id-or-path"
    };
  }

  private string _content = null;

  public bool IsValid { get; set; }
  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort RepoType { get; set; }
  public uint RepoId { get; set; }
  public uint DocId { get; set; }
  public long Length { get; set; }
  public ulong StartingOffset { get; set; }
  public long NameAddress { get; set; }
  public string Name { get; set; }
  public long ExternalIdOrPathAddress { get; set; }
  public string ExternalIdOrPath { get; set; }

  public string Content
  {
    get
    {
      if (!IsValid) return "Invalid Document - Content not available";
      if (_content == null)
      {
        _content = File.ReadAllText(ExternalIdOrPath);
      }

      return _content;
    }
  }

  public static IDocument InvalidDocument { get; }
}
