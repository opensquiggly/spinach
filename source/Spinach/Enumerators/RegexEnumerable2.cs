namespace Spinach.Enumerators;

using Misc;

public class RegexEnumerable2 : IEnumerable<MatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public RegexEnumerable2( string regex, ITextSearchEnumeratorContext context)
  {
    Regex = regex;
    Context = context;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private RE2 CompiledRegex { get; set; }

  private string Regex { get; }

  private ITextSearchEnumeratorContext Context { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

  public IEnumerator<MatchData> GetEnumerator()
  {
    NormalizedRegex normalized = RegexParser.Parse(Regex);
    LiteralQueryNode queryNode = LiteralQueryBuilder.BuildQuery(normalized);

    IFastEnumerable<IFastEnumerator<MatchWithRepoOffsetKey, MatchData>, MatchWithRepoOffsetKey, MatchData> queryEnumerable =
      LiteralQueryBuilder.BuildEnumerable2(queryNode, Context);
    CompiledRegex = RE2.CompileCaseInsensitive(Regex);

    foreach (MatchData match in queryEnumerable)
    {
      // if (!match.IsDocumentValid) continue;
      string contents = File.ReadAllText(match.Document.ExternalIdOrPath);

      if (CompiledRegex.Match(contents))
      {
        yield return match;
      }
    }
  }
}
