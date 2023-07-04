namespace Spinach.Regex.Analyzers;

public static class LiteralQueryBuilder
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

  public static LiteralQueryNode BuildQuery(NormalizedRegex regex)
  {
    string literal = GetStringLiteral(regex);

    if (literal != null)
    {
      if (literal.Length >= 3)
      {
        return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.Literal, Literal = literal };
      }
      
      // Any literal less than 3 characters means we have to query all documents because
      // the smalling n-grams we index are trigrams
      return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.AllDocuments };
    }

    var subQueryNodes = new List<LiteralQueryNode>();

    switch (regex.Op)
    {
      case NormalizedOpTypes.Alternate:
        foreach (var sub in regex.Subs)
        {
          var queryNode = BuildQuery(sub);
          if (queryNode.NodeType == LiteralQueryNodeTypes.AllDocuments)
          {
            // If any subexpressions yield an "AllDocuments" result, then we just immediately
            // return an "AllDocuments", because Or-ing together a bunch of stuff combined
            // with an "AllDocuments" node means we still have to query "AllDocuments"
            return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.AllDocuments };
          }

          subQueryNodes.Add(queryNode);
        }

        if (subQueryNodes.Count == 0)
        {
          return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.AllDocuments };
        }
        if (subQueryNodes.Count == 1)
        {
          return subQueryNodes[0];
        }
        return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.Union, Subs = subQueryNodes };
      
      case NormalizedOpTypes.Concatenate:
        foreach (var sub in regex.Subs)
        {
          var queryNode = BuildQuery(sub);
          if (queryNode.NodeType == LiteralQueryNodeTypes.AllDocuments)
          {
            // If any subexpressions yield an "AllDocuments" result, then we just skip it
            // because Anding-ing together a bunch of stuff combined "AllDocuments" node means we
            // the "AllDocuments" node will have no effect on the result
            continue;
          }

          subQueryNodes.Add(queryNode);
        }
        
        if (subQueryNodes.Count == 0)
        {
          return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.AllDocuments };
        }
        if (subQueryNodes.Count == 1)
        {
          return subQueryNodes[0];
        }
        return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.Intersect, Subs = subQueryNodes };
      
      default:
        return new LiteralQueryNode() { NodeType = LiteralQueryNodeTypes.AllDocuments };
    }
  }

  public static void Print(LiteralQueryNode queryNode, int depth = 0)
  {
    string indent = new string(' ', depth * 2);

    if (queryNode.NodeType == LiteralQueryNodeTypes.Literal)
    {
      Console.WriteLine($"{indent}NodeType: {queryNode.NodeType} = '{queryNode.Literal}'");
    }
    else
    {
      Console.WriteLine($"{indent}NodeType: {queryNode.NodeType}");
    }
   
    if (queryNode.Subs != null)
    {
      int index = 0;
      foreach (var sub in queryNode.Subs)
      {
        Print(sub, depth + 1);
        index++;
      }
    }
  }
}
