namespace Spinach.TrackingObjects;

public class User : IUser
{
  static User()
  {
    InvalidUser = new User()
    {
      IsValid = false,
      Type = 0,
      Id = 0,
      NameAddress = 0,
      Name = "Invalid User",
      ExternalIdAddress = 0,
      ExternalId = "invalid-external-id",
      LastRepoId = 0
    };
  }

  public bool IsValid { get; set; }
  public ushort Type { get; set; }
  public uint Id { get; set; }
  public long NameAddress { get; set; }
  public string Name { get; set; }
  public long ExternalIdAddress { get; set; }
  public string ExternalId { get; set; }
  public uint LastRepoId { get; set; }

  public static IUser InvalidUser { get; }
}
