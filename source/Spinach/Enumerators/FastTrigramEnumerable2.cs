namespace Spinach.Enumerators;

using Misc;

public class FastTrigramEnumerable2 : IFastEnumerable<IFastEnumerator<MatchWithRepoOffsetKey, MatchData>, MatchWithRepoOffsetKey, MatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerable2(string trigram, ITextSearchEnumeratorContext context, int adjustedOffset = 0)
  {
    Trigram = trigram;
    AdjustedOffset = adjustedOffset;
    Context = context;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private string Trigram { get; }

  private int AdjustedOffset { get; }

  private ITextSearchEnumeratorContext Context { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<MatchData> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<MatchWithRepoOffsetKey, MatchData> GetFastEnumerator()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramEnumerator2(Trigram, Context, AdjustedOffset);
  }
}
