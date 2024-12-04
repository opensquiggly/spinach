namespace Spinach.Interfaces;

public interface ITextSearchManager
{
  string LoadString(long address);
  bool IndexFilesForSliceOfTime(CancellationToken cancellationToken, int milliseconds = 5000);

  ITextSearchOptions Options { get; }
}
