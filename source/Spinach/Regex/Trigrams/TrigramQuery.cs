namespace Spinach.Regex.Trigrams;

using System.Collections.Generic;
using System.Text;

public class TrigramQuery : List<List<string>>
{
  private static string BuildClause(IEnumerable<string> segment)
  {
    var result = new StringBuilder();
    bool first = true;
    result.Append("(");

    foreach (string term in segment)
    {
      if (!first)
      {
        result.Append(" OR ");
      }
      else
      {
        first = false;
      }

      result.Append("\"");
      foreach (char ch in term)
      {
        switch (ch)
        {
          case '\"':
            result.Append('\\');
            result.Append('\"');
            break;

          case '\t':
            result.Append('\\');
            result.Append('t');
            break;

          case '\n':
            result.Append('\\');
            result.Append('n');
            break;

          case '\r':
            result.Append('\\');
            result.Append('r');
            break;

          case (char)0xc:
            break;

          default:
            result.Append(ch);
            break;
        }
      }
      result.Append("\"");
    }

    result.Append(")");
    return result.ToString();
  }

  public string ToQueryString()
  {
    var sb = new StringBuilder();
    bool first = true;
    foreach (List<string> segment in this)
    {
      if (!first)
      {
        sb.Append(" AND ");
      }
      else
      {
        first = false;
      }
      sb.Append(BuildClause(segment));
    }
    return sb.ToString();
  }
}
