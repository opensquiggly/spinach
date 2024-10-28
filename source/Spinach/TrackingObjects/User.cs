namespace Spinach.TrackingObjects;

public class User : IUser
{
  public ushort Type { get; set; }
  public uint Id { get; set; }
  public long NameAddress { get; set; }
  public string Name { get; set; }
  public long ExternalIdAddress { get; set; }
  public string ExternalId { get; set; }
  public uint LastRepoId { get; set; }
}
