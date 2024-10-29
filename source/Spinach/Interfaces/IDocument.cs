namespace Spinach.Interfaces;

public interface IDocument
{
  ushort UserType { get; set; }
  uint UserId { get; set; }
  ushort RepoType { get; set; }
  uint RepoId { get; set; }
  uint DocId { get; set; }
}
