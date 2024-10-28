namespace Spinach.TrackingObjects;

public class Repository : IRepository
{
  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort Type { get; set; }
  public uint Id { get; set; }
  public long NameAddress { get; set; }
  public string Name { get; set; }
  public long ExternalIdAddress { get; set; }
  public string ExternalId { get; set; }
  public long RootFolderPathAddress { get; set; }
  public string RootFolderPath { get; set; }
  public uint LastDocId { get; set; }
}
