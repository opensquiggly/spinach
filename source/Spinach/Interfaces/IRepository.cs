namespace Spinach.Interfaces;

public interface IRepository
{
  ushort UserType { get; set; }
  uint UserId { get; set; }
  ushort Type { get; set; }
  uint Id { get; set; }
  long NameAddress { get; set; }
  string Name { get; set; }
  long ExternalIdAddress { get; set; }
  string ExternalId { get; set; }
  long RootFolderPathAddress { get; set; }
  string RootFolderPath { get; set; }
  uint LastDocId { get; set; }
}
