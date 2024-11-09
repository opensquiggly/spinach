namespace Spinach.Interfaces;

public interface ITextSearchManager
{
  string LoadString(long address);

  ITextSearchOptions Options { get; }
}
