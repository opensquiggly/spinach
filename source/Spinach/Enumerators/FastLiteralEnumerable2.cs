namespace Spinach.Enumerators;

using Misc;

public class FastLiteralEnumerable2 : IFastEnumerable<IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData>, TrigramMatchPositionKey, TextSearchMatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerable2(string literal, ITextSearchEnumeratorContext context)
  {
    Literal = literal;
    Context = context;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private string Literal { get; }

  private ITextSearchEnumeratorContext Context { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<TextSearchMatchData> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData> GetFastEnumerator() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new FastLiteralEnumerator2(Literal, Context);
}
