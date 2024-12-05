namespace Spinach.Interfaces;

public interface IDocument
{
  bool IsValid { get; set; }
  ushort UserType { get; set; }
  uint UserId { get; set; }
  ushort RepoType { get; set; }
  uint RepoId { get; set; }
  uint DocId { get; set; }
  DocStatus Status { get; set; }
  bool IsIndexed { get; set; }
  long OriginalLength { get; set; }
  long CurrentLength { get; set; }
  ulong StartingOffset { get; set; }
  long NameAddress { get; set; }
  string Name { get; set; }
  long ExternalIdOrPathAddress { get; set; }
  string ExternalIdOrPath { get; set; }
  string FullPath { get; set; }
  string Content { get; }
}
