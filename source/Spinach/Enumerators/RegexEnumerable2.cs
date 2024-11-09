namespace Spinach.Enumerators;

using Misc;

public class RegexEnumerable2 : IEnumerable<MatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public RegexEnumerable2(string regex, ITextSearchEnumeratorContext context)
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
    IFastEnumerator<MatchWithRepoOffsetKey, MatchData> queryEnumerator = queryEnumerable.GetFastEnumerator();
    CompiledRegex = RE2.CompileCaseInsensitive(Regex);

    while (queryEnumerator.MoveNext())
    {
      MatchData match = queryEnumerator.CurrentData;

      if (match.Document.Length > 100000)
      {
        var skipToKey = new MatchWithRepoOffsetKey()
        {
          UserType = queryEnumerator.CurrentKey.UserType,
          UserId = queryEnumerator.CurrentKey.UserId,
          RepoType = queryEnumerator.CurrentKey.RepoType,
          RepoId = queryEnumerator.CurrentKey.RepoId,
          Offset = (long)(queryEnumerator.CurrentData.Document.StartingOffset + (ulong)queryEnumerator.CurrentData.Document.Length)
        };

        if (!queryEnumerator.MoveUntilGreaterThanOrEqual(skipToKey)) break; ;
      }

      if (!match.Document.IsValid) continue;

      if (CompiledRegex.Match(match.Document.Content))
      {
        yield return match;
      }
    }
  }
}
