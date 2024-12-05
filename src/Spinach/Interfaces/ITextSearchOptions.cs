namespace Spinach.Interfaces;

using Misc;

public interface ITextSearchOptions
{
  int MaxDocSize { get; set; }

  DocMatchType DocMatchType { get; set; }

  bool MatchCase { get; set; }
}
