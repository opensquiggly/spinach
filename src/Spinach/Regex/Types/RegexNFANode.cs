namespace Spinach.Regex.Types;

public class RegexNFANode
{
  public RegexNFANode LitOut;
  public int LitBegin;
  public int LitEnd;
  public List<RegexNFANode> Epsilons = new List<RegexNFANode>();
  public List<RegexNFANode> EpsilonClosures = new List<RegexNFANode>();
  public IEnumerable<string> Trigrams;
  public IEnumerable<string> Literals;
  public int WhenSeen;
  public int Capacity;
  public List<RegexNFANode> ResidualEdges = new List<RegexNFANode>();
}
