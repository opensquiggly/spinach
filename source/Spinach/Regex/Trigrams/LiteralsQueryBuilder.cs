// ----------------------------------------------------------------------------------
// Full Original Go Source Code
// https://github.com/aaw/regrams/blob/master/regrams.go
// ----------------------------------------------------------------------------------

namespace Spinach.Regex.Trigrams;

using System.Runtime.InteropServices.JavaScript;

public class LiteralsQueryBuilder
{
  private int _maxCharClassSize;
  private readonly int _maxTrigramsSetSize;
  private int _maxNfaNodes;
  private readonly int _infinity;

  public LiteralsQueryBuilder() : this(10, 100, 1000)
  {
  }

  public LiteralsQueryBuilder(int maxCharClassSize, int maxTrigramsSetSize, int maxNFANodes)
  {
    _maxCharClassSize = maxCharClassSize;
    _maxTrigramsSetSize = maxTrigramsSetSize;
    _maxNfaNodes = maxNFANodes;
    _infinity = maxTrigramsSetSize * maxNFANodes;
  }

  /// <summary>
  /// A textbook regular expression. If Op is literal, this represents the
  /// character class [LitBegin-LitEnd]. If Op is kleeneStar, concatenate, or
  /// alternate, Sub is populated with subexpressions.
  /// </summary>
  public class NormalizedRegex
  {
    public NormalizedOpTypes Op;
    public List<NormalizedRegex> Subs;
    public int LitBegin;
    public int LitEnd;
  }

  public enum NormalizedOpTypes
  {
    KleeneStar = 0,
    Concatenate,
    Alternate,
    Literal,
    EmptyString,
    NoMatch
  }

  public class NFA
  {
    public NFANode Start;
    public NFANode Accept;
  }

  /// <summary>
  /// An nFANode has zero or more epsilon-transitions but only at most one
  /// character class transition ([LitBegin-LitEnd] -> LitOut). If the node has no
  /// character class transition, LitOut is nil. EpsilonClosure is populated by
  /// calling populateEpsilonClosure and Trigrams is populated by calling
  /// populateTrigrams. WhenSeen is the last epoch this node was visited and
  /// Capacity is used by findCut (and populated in that method by calling
  /// populateCapacities). ResidualEdges is used only during min cut isolation.
  /// </summary>
  public class NFANode
  {
    public NFANode LitOut;
    public int LitBegin;
    public int LitEnd;
    public List<NFANode> Epsilons = new List<NFANode>();
    public List<NFANode> EpsilonClosures = new List<NFANode>();
    public IEnumerable<string> Trigrams;
    public IEnumerable<string> Literals;
    public int WhenSeen;
    public int Capacity;
    public List<NFANode> ResidualEdges = new List<NFANode>();
  }

  private int epoch = 0;

  private NormalizedRegex ParseRegexString(string expr)
  {
    Regexp re = Parser.Parse(expr, RE2.PERL | RE2.PERL_X);
    Regexp sre = Simply.Simplify(re);
    return BuildNormalizedRegex(sre);
  }

  private NormalizedRegex BuildNormalizedRegex(Regexp re)
  {
    switch (re.op)
    {
      case Regexp.Op.NO_MATCH:
        return new NormalizedRegex { Op = NormalizedOpTypes.NoMatch };

      case Regexp.Op.EMPTY_MATCH:
      case Regexp.Op.BEGIN_LINE:
      case Regexp.Op.END_LINE:
      case Regexp.Op.BEGIN_TEXT:
      case Regexp.Op.END_TEXT:
      case Regexp.Op.WORD_BOUNDARY:
      case Regexp.Op.NO_WORD_BOUNDARY:
        return new NormalizedRegex { Op = NormalizedOpTypes.EmptyString };

      case Regexp.Op.LITERAL:
        var list = new List<NormalizedRegex>();
        foreach (int r in re.runes)
        {
          if ((re.flags & RE2.FOLD_CASE) != 0)
          {
            var folds = new List<NormalizedRegex>
            {
              new NormalizedRegex {Op = NormalizedOpTypes.Literal, LitBegin = r, LitEnd = r}
            };
            for (int f = Unicode.simpleFold(r); f != r; f = Unicode.simpleFold(f))
            {
              folds.Add(
                new NormalizedRegex
                {
                  Op = NormalizedOpTypes.Literal,
                  LitBegin = f,
                  LitEnd = f
                }
              );
            }

            list.Add(new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = folds });
          }
          else
          {
            list.Add(new NormalizedRegex { Op = NormalizedOpTypes.Literal, LitBegin = r, LitEnd = r });
          }
        }

        return new NormalizedRegex { Op = NormalizedOpTypes.Concatenate, Subs = list };

      case Regexp.Op.ANY_CHAR_NOT_NL:
        {
          var beforeNL = new NormalizedRegex { Op = NormalizedOpTypes.Literal, LitBegin = 0, LitEnd = '\n' };
          var afterNL = new NormalizedRegex
          { Op = NormalizedOpTypes.Literal, LitBegin = '\n', LitEnd = Unicode.MAX_RUNE };
          var subs = new List<NormalizedRegex>()
          {
            beforeNL,
            afterNL
          };
          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs };
        }

      case Regexp.Op.ANY_CHAR:
        return new NormalizedRegex { Op = NormalizedOpTypes.Literal, LitBegin = 0, LitEnd = Unicode.MAX_RUNE };

      case Regexp.Op.CAPTURE:
        return BuildNormalizedRegex(re.subs[0]);

      case Regexp.Op.CONCAT:
        {
          var subs = new List<NormalizedRegex>();
          subs.AddRange(re.subs.Select(BuildNormalizedRegex));
          return new NormalizedRegex { Op = NormalizedOpTypes.Concatenate, Subs = subs };
        }

      case Regexp.Op.ALTERNATE:
        {
          var subs = new List<NormalizedRegex>();
          subs.AddRange(re.subs.Select(BuildNormalizedRegex));
          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs };
        }

      case Regexp.Op.QUEST:
        {
          var subs = new List<NormalizedRegex>
          {
            BuildNormalizedRegex(re.subs[0]),
            new NormalizedRegex {Op = NormalizedOpTypes.EmptyString}
          };
          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs };
        }

      case Regexp.Op.STAR:
        {
          var subs = new List<NormalizedRegex>
          {
            BuildNormalizedRegex(re.subs[0]),
          };
          return new NormalizedRegex { Op = NormalizedOpTypes.KleeneStar, Subs = subs };
        }

      case Regexp.Op.REPEAT:
        {
          var args = new List<NormalizedRegex>();
          NormalizedRegex sub = BuildNormalizedRegex(re.subs[0]);
          for (int i = 0; i < re.min; i++)
          {
            args.Add(sub);
          }

          for (int i = re.min; i < re.max; i++)
          {
            var subs = new List<NormalizedRegex>
            {
              sub,
              new NormalizedRegex {Op = NormalizedOpTypes.EmptyString}
            };
            args.Add(new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs });
          }

          return new NormalizedRegex { Op = NormalizedOpTypes.Concatenate, Subs = args };
        }

      case Regexp.Op.PLUS:
        {
          NormalizedRegex parsed = BuildNormalizedRegex(re.subs[0]);
          return new NormalizedRegex
          {
            Op = NormalizedOpTypes.Concatenate,
            Subs = new List<NormalizedRegex>{
            parsed, new NormalizedRegex
            {
              Op = NormalizedOpTypes.KleeneStar,
              Subs = new List<NormalizedRegex>{ parsed }
            }}
          };
        }

      case Regexp.Op.CHAR_CLASS:
        {
          var args = new List<NormalizedRegex>();
          for (int i = 0; i < re.runes.Length - 1; i += 2)
          {
            args.Add(new NormalizedRegex
            {
              Op = NormalizedOpTypes.Literal,
              LitBegin = re.runes[i],
              LitEnd = re.runes[i + 1]
            });
          }

          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = args };
        }
    }

    throw new Exception($"Unknown regexp operation: {re.op}");
  }

  /// <summary>
  /// Thompson's construction of an NFA from a regular expression.
  /// </summary>
  /// <param name="nre">normalized regex</param>
  /// <returns>nfa</returns>
  private NFA BuildNFA(NormalizedRegex nre)
  {
    switch (nre.Op)
    {
      case NormalizedOpTypes.KleeneStar:
        {
          NFA sub = BuildNFA(nre.Subs[0]);
          var accept = new NFANode();
          var startEpsilons = new List<NFANode>()
          {
            sub.Start,
            accept
          };
          var start = new NFANode()
          {
            Epsilons = startEpsilons
          };
          sub.Accept.Epsilons.Add(sub.Start);
          sub.Accept.Epsilons.Add(accept);
          return new NFA()
          {
            Start = start,
            Accept = accept
          };
        }

      case NormalizedOpTypes.Concatenate:
        {
          NFA next = null, curr = null;
          NFANode accept = null;
          for (int i = nre.Subs.Count - 1; i >= 0; i--)
          {
            curr = BuildNFA(nre.Subs[i]);
            if (next != null)
            {
              curr.Accept.Epsilons.Add(next.Start);
            }
            else
            {
              accept = curr.Accept;
            }

            next = curr;
          }

          return new NFA() { Start = curr.Start, Accept = accept };
        }

      case NormalizedOpTypes.Alternate:
        {
          var subStarts = new List<NFANode>();
          var accept = new NFANode();
          foreach (NormalizedRegex sub in nre.Subs)
          {
            NFA nfa = BuildNFA(sub);
            nfa.Accept.Epsilons.Add(accept);
            subStarts.Add(nfa.Start);
          }

          var start = new NFANode() { Epsilons = subStarts };
          return new NFA() { Start = start, Accept = accept };
        }

      case NormalizedOpTypes.Literal:
        {
          var accept = new NFANode();
          var start = new NFANode()
          {
            LitBegin = nre.LitBegin,
            LitEnd = nre.LitEnd,
            LitOut = accept
          };
          return new NFA() { Start = start, Accept = accept };
        }

      case NormalizedOpTypes.EmptyString:
        {
          var accept = new NFANode();
          var startEpsilons = new List<NFANode>() { accept };
          var start = new NFANode() { Epsilons = startEpsilons };
          return new NFA() { Start = start, Accept = accept };
        }

      case NormalizedOpTypes.NoMatch:
        {
          return new NFA() { Start = new NFANode() };
        }
    }

    throw new Exception($"Unknown regexp operation: {nre.Op}");
  }

  /// <summary>
  /// Visit the node, return true if the node has already been visited this
  /// epoch and false otherwise.
  /// </summary>
  /// <param name="node">nfa node</param>
  /// <param name="epoch"></param>
  /// <returns></returns>
  private static bool Seen(NFANode node, int epoch)
  {
    if (node.WhenSeen == epoch)
      return true;

    node.WhenSeen = epoch;
    return false;
  }

  private static List<NFANode> UniqNodes(IEnumerable<NFANode> nodes)
  {
    var result = new List<NFANode>();
    result.AddRange(nodes.Distinct());
    return result;
  }

  /// <summary>
  /// Compute the epsilon closure of each node in the NFA, populate the
  /// EpsilonClosure field of each node with that value. The epsilon closure of
  /// a node is the set of all nodes that can be reached via zero or more epsilon
  /// transitions.
  /// </summary>
  /// <param name="nfa"></param>
  private void PopulateEpsilonClosure(NFA nfa) => PopulateEpsilonClosureHelper(nfa.Start, ++epoch);

  private static void PopulateEpsilonClosureHelper(NFANode node, int epoch)
  {
    if (Seen(node, epoch))
      return;

    var closure = new List<NFANode>();
    if (node.LitOut != null || node.Epsilons.Count == 0)
    {
      closure.Add(node);
    }

    foreach (NFANode e in node.Epsilons)
    {
      PopulateEpsilonClosureHelper(e, epoch);
      closure.AddRange(e.EpsilonClosures);
    }

    node.EpsilonClosures = UniqNodes(closure);
    if (node.LitOut != null)
    {
      PopulateEpsilonClosureHelper(node.LitOut, epoch);
    }
  }

  /// <summary>
  /// Build the trigram set for all nodes in the NFA. Nodes with trigram sets that
  /// are deemed too large during expansion or that can't be computed because the
  /// accept state is reachable in 2 or fewer steps are given an empty trigram
  /// set. There's a faster way to compute the trigram sets than what we do here:
  /// we're essentially running a separate sub-traversal to compute trigrams
  /// at each node (the call to "trigrams" in populateTrigramsHelper), when we
  /// could be computing the trigram set with three passes over the graph,
  /// accumulating intermediate suffixes to build up the trigrams at each step.
  /// </summary>
  /// <param name="nfa"></param>
  private void PopulateTrigrams(NFA nfa) => PopulateTrigramsHelper(nfa.Start, nfa.Accept, ++epoch);

  private void PopulateTrigramsHelper(NFANode node, NFANode accept, int epoch)
  {
    if (Seen(node, epoch))
      return;

    if (node.LitOut != null)
    {
      node.Trigrams = Trigrams(node, accept);
      PopulateTrigramsHelper(node.LitOut, accept, epoch);
    }

    foreach (NFANode eps in node.Epsilons)
    {
      PopulateTrigramsHelper(eps, accept, epoch);
    }
  }
  
  private void PopulateLiterals(NFA nfa) => PopulateLiteralsHelper(nfa.Start, nfa.Accept, ++epoch);
  
  private void PopulateLiteralsHelper(NFANode node, NFANode accept, int epoch)
  {
    if (Seen(node, epoch))
      return;

    if (node.LitOut != null)
    {
      node.Literals = Literals(node, accept);
      PopulateLiteralsHelper(node.LitOut, accept, epoch);
    }

    foreach (NFANode eps in node.Epsilons)
    {
      PopulateLiteralsHelper(eps, accept, epoch);
    }
  }

  /// <summary>
  /// Compute the trigram set for an individual node.
  /// </summary>
  /// <param name="root">nfa node</param>
  /// <param name="accept">nfa node</param>
  /// <returns></returns>
  private IEnumerable<string> Trigrams(NFANode root, NFANode accept) => NgramSearch(root, accept, 3).Distinct();
  
  private IEnumerable<string> Literals(NFANode root, NFANode accept) => LiteralsSearch(root, accept).Distinct();

  private List<string> NgramSearch(NFANode node, NFANode accept, int limit)
  {
    if (limit == 0)
    {
      return new List<string>() { "" };
    }

    var results = new List<string>();
    foreach (NFANode cnode in node.EpsilonClosures)
    {
      if (cnode == accept)
      {
        // Bail out, we can reach the accept state before we've
        // consumed enough characters for a full n-gram.   
        return new List<string>();
      }

      if (cnode.LitOut == null)
      {
        continue;
      }

      int begin = cnode.LitBegin;
      int end = cnode.LitEnd;
      if (end - begin + 1 > this._maxTrigramsSetSize)
      {
        // Bail out, the ngram set might be too large.
        return new List<string>();
      }

      List<string> subResults = NgramSearch(cnode.LitOut, accept, limit - 1);
      if (subResults.Count == 0)
      {
        // A subresult has bailed out. short-circuit here too.
        return new List<string>();
      }

      if (subResults.Count * (end - begin + 1) > this._maxTrigramsSetSize)
      {
        // Bail out, the ngram set is going to be too large.
        return new List<string>();
      }

      for (int i = begin; i <= end; i++)
      {
        var suffixes = new List<string>();
        suffixes.AddRange(subResults);
        CrossProduct(i, suffixes);
        results.AddRange(suffixes);
      }
    }

    return results;
  }
  
  private List<string> LiteralsSearch(NFANode node, NFANode accept)
  {
    // if (limit == 0)
    // {
    //   return new List<string>() { "" };
    // }

    var results = new List<string>();
    foreach (NFANode cnode in node.EpsilonClosures)
    {
      if (cnode == accept)
      {
        // Bail out, we can reach the accept state before we've
        // consumed enough characters for a full n-gram.   
        return new List<string>() { "" };
      }

      if (cnode.LitOut == null)
      {
        continue;
      }

      int begin = cnode.LitBegin;
      int end = cnode.LitEnd;
      // if (end - begin + 1 > this._maxTrigramsSetSize)
      // {
      //   // Bail out, the ngram set might be too large.
      //   return new List<string>();
      // }

      List<string> subResults = LiteralsSearch(cnode.LitOut, accept);
      // if (subResults.Count == 0)
      // {
      //   // A subresult has bailed out. short-circuit here too.
      //   return new List<string>();
      // }

      // if (subResults.Count * (end - begin + 1) > this._maxTrigramsSetSize)
      // {
      //   // Bail out, the ngram set is going to be too large.
      //   return new List<string>();
      // }

      // for (int i = begin; i <= end; i++)
      // {
      //   var suffixes = new List<string>();
      //   suffixes.AddRange(subResults);
      //   CrossProduct(i, suffixes);
      //   results.AddRange(suffixes);
      // }
      
      if (begin == end)
      {
        var suffixes = new List<string>();
        suffixes.AddRange(subResults);
        CrossProduct(begin, suffixes);
        results.AddRange(suffixes);
      }      
    }

    return results;
  }  

  /// <summary>
  /// Prefix each string in y with the string at codepoint x.
  /// </summary>
  /// <param name="x"></param>
  /// <param name="y"></param>
  private void CrossProduct(int x, IList<string> y)
  {
    string s = (new Rune(x)).ToString();
    for (int i = 0; i < y.Count; i++)
    {
      y[i] = s + y[i];
    }
  }

  /// <summary>
  /// Once the trigram set is populated on each node, all that's left is to
  /// generate the query. We find a minimum weight vertex cut in the NFA based on
  /// weights computed from the size of the trigram sets of each node, then
  /// recursively continue on both sides of the cut to identify disjunctions that
  /// we can AND together to make a complete query.
  /// </summary>
  /// <param name="nfa"></param>
  /// <returns></returns>
  private TrigramQuery MakeQueryHelper(NFA nfa)
  {
    (NFA s, NFA t, List<string> cut) = FindCut(nfa);
    if (cut.Count > 0)
    {
      TrigramQuery sq = MakeQueryHelper(s);
      TrigramQuery tq = MakeQueryHelper(t);
      var result = new TrigramQuery();
      var uniqCut = new List<string>();
      uniqCut.AddRange(cut.Distinct());
      sq.Add(uniqCut);
      result.AddRange(sq);
      result.AddRange(tq);
      return result;
    }

    return new TrigramQuery();
  }

  /// <summary>
  /// Find a path from nfa.Start to nfa.Accept through vertices of positive
  /// capacity. The path is returned in reverse order from accept to start.
  /// </summary>
  /// <param name="nfa"></param>
  /// <returns></returns>
  private List<NFANode> FindAugmentingPath(NFA nfa) => FindAugmentingPathHelper(nfa.Start, nfa.Accept, ++epoch);

  private static List<NFANode> FindAugmentingPathHelper(NFANode node, NFANode accept, int epoch)
  {
    if (Seen(node, epoch) || node.Capacity == 0)
      return null;

    if (node == accept)
    {
      return new List<NFANode>()
      {
        node
      };
    }

    if (node.LitOut != null)
    {
      List<NFANode> path = FindAugmentingPathHelper(node.LitOut, accept, epoch);
      if (path != null)
      {
        path.Add(node);
        return path;
      }
    }

    foreach (NFANode v in node.Epsilons)
    {
      List<NFANode> path = FindAugmentingPathHelper(v, accept, epoch);
      if (path != null)
      {
        path.Add(node);
        return path;
      }
    }

    return null;
  }

  /// <summary>
  /// Calculate capacities for all nodes in the NFA.
  /// </summary>
  /// <param name="nfa"></param>
  private void PopulateCapacities(NFA nfa) => PopulateCapacitiesHelper(nfa.Start, ++epoch);


  /// <summary>
  /// * Any node with LitOut = nil has capacity infinity.
  /// * Any node with LitOut != nil &amp;&amp; empty trigram set has capacity infinity.
  /// * Any node with LitOut != nil &amp;&amp; non-empty trigram set has capacity
  ///   len(trigram set).
  /// </summary>
  /// <param name="node"></param>
  /// <param name="epoch"></param>
  private void PopulateCapacitiesHelper(NFANode node, int epoch)
  {
    if (Seen(node, epoch))
      return;

    if (node.LitOut != null)
    {
      // int nt = node.Trigrams?.Count() ?? 0;
      int nt = node.Literals?.Count() ?? 0;
      node.Capacity = (nt > 0) ? nt : this._infinity;
      PopulateCapacitiesHelper(node.LitOut, epoch);
    }
    else
    {
      node.Capacity = this._infinity;
    }

    foreach (NFANode eps in node.Epsilons)
    {
      PopulateCapacitiesHelper(eps, epoch);
    }
  }


  /// <summary>
  /// Find a minimum-weight vertex cut in the NFA by repeatedly pushing flow
  /// through a path of positive capacity until no such path exists. This is
  /// essentially the (depth-first) Ford-Fulkerson algorithm. After no more flow
  /// can be pushed through, identify the cut and do a little surgery on the NFA
  /// so that it's actually two NFAs: one on each side of the cut. We'll pass
  /// both NFAs back along with the cut and continue extracting queries from each.
  /// </summary>
  /// <param name="nfa"></param>
  /// <returns></returns>
  private (NFA, NFA, List<string>) FindCut(NFA nfa)
  {
    PopulateCapacities(nfa);

    for (var path = new List<NFANode>(); path != null; path = FindAugmentingPath(nfa))
    {
      int minCap = this._infinity;
      foreach (NFANode node in path)
      {
        if (node.Capacity < minCap)
        {
          minCap = node.Capacity;
        }
      }

      // For every node on the augmenting path, decrement the
      // capacity by the min capacity on the path and install
      // back edges to simulate reverse edges in the residual
      // graph.        
      NFANode prev = null;
      foreach (NFANode node in path)
      {
        if (prev != null && !prev.ResidualEdges.Contains(node))
        {
          prev.ResidualEdges.Add(node);
        }

        node.Capacity -= minCap;
        prev = node;
      }
    }

    (List<NFANode> cut, int cutEpoch) = IsolateCut(nfa);
    var accept = new NFANode();
    var start = new NFANode();
    var orClause = new List<string>();

    foreach (NFANode node in cut)
    {
      bool frontier = false;
      for (int i = 0; i < node.Epsilons.Count; i++)
      {
        NFANode e = node.Epsilons[i];
        if (e.WhenSeen != cutEpoch)
        {
          frontier = true;
          start.Epsilons.Add(e);
          node.Epsilons[i] = accept;
        }
      }

      if (node.LitOut != null && node.LitOut.WhenSeen != cutEpoch)
      {
        frontier = true;
        start.Epsilons.Add(node.LitOut);
        node.LitOut = accept;
      }

      if (frontier && node.LitOut != null)
      {
        // orClause.AddRange(node.Trigrams);
        orClause.AddRange(node.Literals);
        // This is a hack, we're clearing the trigram set on a
        // node when it's used so that they aren't continually
        // reused when the graph is decomposed and cut again.            
        node.Trigrams = new List<string>();
        node.Literals = new List<string>();
      }
    }

    var leftNFA = new NFA() { Start = nfa.Start, Accept = accept };
    var rightNFA = new NFA() { Start = start, Accept = nfa.Accept };

    return (leftNFA, rightNFA, orClause);
  }

  /// <summary>
  /// Once capacities have been decremented by pushing flow through a graph, we
  /// can identify the cut by figuring out which nodes are reachable on the
  /// residual flow graph without crossing any zero-capacity nodes. We run a
  /// depth-first search here to identify all reachable zero-capacity nodes, then
  /// mark all vertices that are reachable without crossing zero-capacity nodes
  /// except via residual edges. The findCut function calling this function then
  /// figures out which nodes are in the cut from that information.
  /// </summary>
  /// <param name="nfa"></param>
  /// <returns></returns>
  private (List<NFANode>, int) IsolateCut(NFA nfa)
  {
    List<NFANode> cut = IsolateCutHelper(nfa.Start, ++epoch);
    int resisualEpoch = ++epoch;
    ResidualTraversal(nfa.Start, false, resisualEpoch);
    return (cut, resisualEpoch);
  }

  private List<NFANode> IsolateCutHelper(NFANode node, int epoch)
  {
    if (Seen(node, epoch))
      return null;

    var result = new List<NFANode>() { node };

    if (node.Capacity == 0)
      return result;

    if (node.LitOut != null)
    {
      List<NFANode> temp = IsolateCutHelper(node.LitOut, epoch);
      if (temp != null)
      {
        result.AddRange(temp);
      }
    }

    foreach (NFANode e in node.Epsilons)
    {
      List<NFANode> temp = IsolateCutHelper(e, epoch);
      if (temp != null)
      {
        result.AddRange(temp);
      }
    }

    return result;
  }

  /// <summary>
  /// Traverse all nodes in the graph, avoiding crossing capacity 0 vertices
  /// unless we're moving across edges in the residual graph.
  /// </summary>
  /// <param name="node"></param>
  /// <param name="upstream"></param>
  /// <param name="epoch"></param>
  private void ResidualTraversal(NFANode node, bool upstream, int epoch)
  {
    if (Seen(node, epoch))
      return;
    if (node.Capacity == 0 && !upstream)
      return;
    if (node.LitOut != null)
    {
      ResidualTraversal(node.LitOut, false, epoch);
    }

    foreach (NFANode e in node.Epsilons)
    {
      ResidualTraversal(e, false, epoch);
    }

    foreach (NFANode r in node.ResidualEdges)
    {
      ResidualTraversal(r, true, epoch);
    }
  }

  private static string BuildDocTypeQuery(bool docTypeIsMountPoint)
  {
    string resultQuery = $"_exists_:\"mountPointId\"";
    return docTypeIsMountPoint ? resultQuery : $"!({resultQuery})";
  }

  /// <summary>
  /// Make a regrams.Query from a string representation of a regexp.
  /// </summary>
  /// <param name="regex"></param>
  /// <returns></returns>
  public TrigramQuery MakeQuery(string regex)
  {
    NormalizedRegex nre = ParseRegexString(regex);
    NFA nfa = BuildNFA(nre);

    // var n = CountReachableNodes(nfa);
    // TODO: Check for NFA too big

    PopulateEpsilonClosure(nfa);
    PopulateTrigrams(nfa);
    PopulateLiterals(nfa);
    TrigramQuery q = MakeQueryHelper(nfa);
    // var s = simplify(q);

    return q;
  }

  public NFA CompileToNFA(string regex)
  {
    NormalizedRegex nre = ParseRegexString(regex);
    NFA nfa = BuildNFA(nre);
    PopulateEpsilonClosure(nfa);

    return nfa;
  }
  
  // public class NFA
  // {
  //   public NFANode Start;
  //   public NFANode Accept;
  // }
  //
  // /// <summary>
  // /// An nFANode has zero or more epsilon-transitions but only at most one
  // /// character class transition ([LitBegin-LitEnd] -> LitOut). If the node has no
  // /// character class transition, LitOut is nil. EpsilonClosure is populated by
  // /// calling populateEpsilonClosure and Trigrams is populated by calling
  // /// populateTrigrams. WhenSeen is the last epoch this node was visited and
  // /// Capacity is used by findCut (and populated in that method by calling
  // /// populateCapacities). ResidualEdges is used only during min cut isolation.
  // /// </summary>
  // public class NFANode
  // {
  //   public NFANode LitOut;
  //   public int LitBegin;
  //   public int LitEnd;
  //   public List<NFANode> Epsilons = new List<NFANode>();
  //   public List<NFANode> EpsilonClosures = new List<NFANode>();
  //   public IEnumerable<string> Trigrams;
  //   public int WhenSeen;
  //   public int Capacity;
  //   public List<NFANode> ResidualEdges = new List<NFANode>();
  // }

  public void PrintNFA(NFA nfa)
  {
    PrintNFANode("Start", nfa.Start, nfa.Accept);
    PrintNFANode("Accept", nfa.Accept, nfa.Accept);
  }

  private void PrintNFANode(string name, NFANode node, NFANode accept, int depth = 0)
  {
    string indent = new string(' ', depth * 2);

    Console.WriteLine($"{indent}***********************");
    
    if (node == accept)
    {
      Console.WriteLine($"{indent}NFA Node: Accept");
      return;
    }
    
    // if (depth > 10)
    // {
    //   Console.WriteLine("Depth too deep");
    //   return;
    // }

    
    Console.WriteLine($"{indent}NFA Node: {name}");

    Console.WriteLine($"{indent}LitBegin: {node.LitBegin}");
    Console.WriteLine($"{indent}LitEnd: {node.LitEnd}");
    Console.WriteLine($"{indent}WhenSeen: {node.WhenSeen}");
    Console.WriteLine($"{indent}Capacity: {node.Capacity}");

    if (node.LitBegin != 0 && node.LitEnd != 0 && node.LitOut != null && node.LitOut != node)
    {
      PrintNFANode(name + ".LitOut", node.LitOut, accept, depth + 1);
    }

    if (node.Epsilons is { Count: > 0 })
    {
      Console.WriteLine($"{indent}Epsilons");
      Console.WriteLine($"{indent}--------");
      for (int x = 0; x < node.Epsilons.Count; x++)
      {
        if (node.Epsilons[x] == node)
        {
          Console.WriteLine($"{indent}epsilon to self at x={x}");
        }
        else
        {
          PrintNFANode($"{name}.Epsilons[{x}]", node.Epsilons[x], accept, depth + 1);
        }
      }
    }
    else
    {
      Console.WriteLine($"{indent}Epsilons is null or empty");
    }

    if (node.EpsilonClosures is { Count: > 0 })
    {
      Console.WriteLine($"{indent}EpsilonClosures");
      Console.WriteLine($"{indent}---------------");
      for (int x = 0; x < node.EpsilonClosures.Count; x++)
      {
        // PrintNFANode($"{name}.EpsilonClosures[{x}]", node.EpsilonClosures[x], accept, depth + 1);
        if (node.EpsilonClosures[x] == node)
        {
          Console.WriteLine($"{indent}epsilon closure to self at x={x}");
        }
        else
        {
          PrintNFANode($"{name}.EpsilonClosures[{x}]", node.EpsilonClosures[x], accept, depth + 1);
        }        
      }
    }
    else
    {
      Console.WriteLine($"{indent}EpsilonClosures is null or empty");
    }

    if (node.ResidualEdges is { Count: > 0 })
    {
      Console.WriteLine($"{indent}ResidualEdges");
      Console.WriteLine($"{indent}-------------");
      for (int x = 0; x < node.ResidualEdges.Count; x++)
      {
        // PrintNFANode($"{name}.ResidualEdges[{x}]", node.ResidualEdges[x], accept, depth + 1);
        if (node.ResidualEdges[x] == node)
        {
          Console.WriteLine($"{indent}residual edge to self at x={x}");
        }
        else
        {
          PrintNFANode($"{name}.ResidualEdges[{x}]", node.ResidualEdges[x], accept, depth + 1);
        }          
      }
    }
    else
    {
      Console.WriteLine($"{indent}ResidualEdges is null or empty");
    }
  }
}
