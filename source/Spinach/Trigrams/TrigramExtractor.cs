namespace Spinach.Trigrams;

public class TrigramExtractor : IEnumerable<TrigramInfo>
{
  public TrigramExtractor(string input)
  {
    Input = input;
  }

  public string Input { get; }

  public IEnumerator<TrigramInfo> GetEnumerator()
  {
    // Assumes all characters in the string are 7-bit ASCII
    TrigramInfo trigramInfo = default;
    char[] buffer = new char[3];
    int trigramStartIndex = 0;

    // var fileContent = File.ReadAllText(FileName);

    if (Input.Length >= 3)
    {
      buffer[0] = char.ToLower(Input[0]);
      buffer[1] = char.ToLower(Input[1]);

      for (int i = 2; i < Input.Length; i++)
      {
        buffer[(trigramStartIndex + 2) % 3] = char.ToLower(Input[i]);
        trigramInfo.Key =
          buffer[trigramStartIndex] * 128 * 128 +
          buffer[(trigramStartIndex + 1) % 3] * 128 +
          buffer[(trigramStartIndex + 2) % 3]
        ;

        trigramInfo.Position = i - 2;
        trigramStartIndex = (trigramStartIndex + 1) % 3;

        yield return trigramInfo;
      }
    }
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
