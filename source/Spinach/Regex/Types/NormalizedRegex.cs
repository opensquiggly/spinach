namespace Spinach.Regex.Types;

public class NormalizedRegex
{
  public NormalizedOpTypes Op;
  public List<NormalizedRegex> Subs;
  public int LitBegin;
  public int LitEnd;
}
