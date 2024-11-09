namespace Spinach.Interfaces;

public interface IDocument
{
  bool IsValid { get; set; }
  ushort UserType { get; set; }
  uint UserId { get; set; }
  ushort RepoType { get; set; }
  uint RepoId { get; set; }
  uint DocId { get; set; }
  long Length { get; set; }
  ulong StartingOffset { get; set; }
  long NameAddress { get; set; }
  string Name { get; set; }
  long ExternalIdOrPathAddress { get; set; }
  string ExternalIdOrPath { get; set; }
}
