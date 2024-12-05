namespace Spinach.Helpers;

public static class TrigramHelper
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public static int GetTrigramKey(char ch1, char ch2, char ch3)
  {
    return
      char.ToLower(ch1) * 128 * 128 +
      char.ToLower(ch2) * 128 +
      char.ToLower(ch3);
  }

  public static int GetTrigramKey(string trigram)
  {
    if (trigram.Length != 3)
    {
      throw new ArgumentOutOfRangeException(
        "TrigramHelper.GetTrigramKey: Trigram string should be exactly 3 characters in length");
    }

    return GetTrigramKey(trigram[0], trigram[1], trigram[2]);
  }

  public static int GetLeadingTrigramKey(string literal)
  {
    if (literal.Length < 3)
    {
      throw new ArgumentOutOfRangeException(
        "TrigramHeplper.GetLeadingTrigramKey: Literal should be at least 3 characters long");
    }

    return GetTrigramKey(literal.Substring(0, 3));
  }

  public static int GetTrailingTrigramKey(string literal)
  {
    if (literal.Length < 3)
    {
      throw new ArgumentOutOfRangeException(
        "TrigramHeplper.GetTrailingTrigramKey: Literal should be at least 3 characters long");
    }

    return GetTrigramKey(literal.Substring(literal.Length - 3, 3));
  }
}
