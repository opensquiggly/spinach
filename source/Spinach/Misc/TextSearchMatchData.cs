namespace Spinach.Misc;

public class TextSearchMatchData
{
  public TextSearchMatchData()
  {
  }

  public bool IsUserValid { get; set; }
  public bool IsRepositoryValid { get; set; }
  public bool IsDocumentValid { get; set; }

  public IUser User { get; set; }
  public IRepository Repository { get; set; }
  public IDocument Document { get; set; }
  public ulong MatchPosition { get; set; }
}
