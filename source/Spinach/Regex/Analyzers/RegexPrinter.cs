namespace Spinach.Regex.Analyzers;

public static class RegexPrinter
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static string GetStringLiteral(NormalizedRegex node)
  {
    // Check if the node is a Concatenate operation
    if (node.Op != NormalizedOpTypes.Concatenate)
    {
      return null;
    }

    StringBuilder sb = new StringBuilder();

    // Check if all subexpressions are single-character literals with no subexpressions
    foreach (NormalizedRegex sub in node.Subs)
    {
      if (sub.Op != NormalizedOpTypes.Literal || sub.LitBegin != sub.LitEnd || sub.Subs != null)
      {
        return null;
      }

      // Append the character to the string
      sb.Append((char)sub.LitBegin);
    }

    // If we made it through the loop, the node represents a string literal
    return sb.ToString();
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public static void Print(NormalizedRegex regex, int depth = 0)
  {
    string indent = new string(' ', depth * 9);
   
    string literal1 = GetStringLiteral(regex);
    if (literal1 != null)
    {
      Console.WriteLine($"{indent}| String Literal: '{literal1}'");
      return;
    }

    if (regex.Op == NormalizedOpTypes.Literal)
    {
      Console.WriteLine($"{indent}| Op: {regex.Op}, LitBegin = {regex.LitBegin}, LitEnd = {regex.LitEnd}");
    }
    else
    {      
      Console.WriteLine($"{indent}| Op: {regex.Op}");
    }


    if (regex.Subs != null)
    {
      int index = 0;
      foreach (var sub in regex.Subs)
      {
        string literal = GetStringLiteral(sub);
        if (literal != null)
        {
          Console.WriteLine($"{indent}| Sub[{index}] = '{literal}'");
        }
        else
        {
          Console.WriteLine($"{indent}| Sub[{index}] + ");
          Print(sub, depth + 1);
        }
        index++;
      }
    }
  }
}
