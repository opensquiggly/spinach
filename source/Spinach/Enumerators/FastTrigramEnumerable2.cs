namespace Spinach.Enumerators;

using Misc;

public class FastTrigramEnumerable2 : IFastEnumerable<IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData>, TrigramMatchPositionKey, TextSearchMatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerable2(string trigram, ITextSearchEnumeratorContext context)
  {
    Trigram = trigram;
    Context = context;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private string Trigram { get; }

  private ITextSearchEnumeratorContext Context { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<TextSearchMatchData> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData> GetFastEnumerator()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramEnumerator2(Trigram, Context);
  }
}
