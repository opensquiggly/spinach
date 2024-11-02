namespace Spinach.Interfaces;

public interface IUser
{
  bool IsValid { get; set; }
  ushort Type { get; set; }
  uint Id { get; set; }
  long NameAddress { get; set; }
  string Name { get; set; }
  long ExternalIdAddress { get; set; }
  string ExternalId { get; set; }
  uint LastRepoId { get; set; }
}
