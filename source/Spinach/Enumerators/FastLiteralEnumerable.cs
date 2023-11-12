namespace Spinach.Enumerators;

public class FastLiteralEnumerable : IFastEnumerable<IFastEnumerator<ulong, long>, ulong, long>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerable(TextSearchIndex textSearchIndex, string literal)
  {
    TextSearchIndex = textSearchIndex;
    Literal = literal;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private string Literal { get; }

  private TextSearchIndex TextSearchIndex { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<ulong> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<ulong, long> GetFastEnumerator() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new FastLiteralEnumerator(TextSearchIndex, Literal);
}
