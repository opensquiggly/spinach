namespace Spinach.Misc;

public class MatchData
{
  public MatchData()
  {
  }

  public bool IsUserValid { get; set; }
  public bool IsRepositoryValid { get; set; }
  public bool IsDocumentValid { get; set; }

  public IUser User { get; set; }
  public IRepository Repository { get; set; }
  public IDocument Document { get; set; }
  public long MatchPosition { get; set; }
}
