namespace Spinach.TrackingObjects;

public class Repository : IRepository
{
  static Repository()
  {
    InvalidRepository = new Repository()
    {
      IsValid = false,
      UserType = 0,
      UserId = 0,
      Type = 0,
      Id = 0,
      NameAddress = 0,
      Name = "Invalid Repository",
      ExternalIdAddress = 0,
      ExternalId = "invalid-external-id",
      RootFolderPathAddress = 0,
      RootFolderPath = "invalid-root-folder-path",
      LastDocId = 0,
      LastDocLength = 0,
      LastDocStartingOffset = 0
    };
  }

  public bool IsValid { get; set; }
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
  public long LastDocLength { get; set; }
  public ulong LastDocStartingOffset { get; set; }

  public static IRepository InvalidRepository { get; }
}
