namespace Spinach.Misc;

public class TextSearchOptions : ITextSearchOptions
{
  static TextSearchOptions()
  {
    Default = new TextSearchOptions()
    {
      MaxDocSize = 1000000,
      DocMatchType = DocMatchType.FirstMatchOnly
    };
  }

  public int MaxDocSize { get; set; }

  public DocMatchType DocMatchType { get; set; }

  public static ITextSearchOptions Default { get; }
}
