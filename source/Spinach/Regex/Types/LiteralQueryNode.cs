namespace Spinach.Regex.Types;

public class LiteralQueryNode
{
  public LiteralQueryNodeTypes NodeType;
  public List<LiteralQueryNode> Subs;
  public string Literal;
}
