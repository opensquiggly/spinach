namespace Spinach.Utils;

public class FileUtils
{
  public static void PrintFile(string fileName, int hilightOffset, int hilightLength)
  {
    using (var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
    {
      using (var streamReader = new StreamReader(fileStream))
      {
        int lineNo = 1;
        int currentOffset = 0;
        while (!streamReader.EndOfStream)
        {
          string line = streamReader.ReadLine();
          if (line == null)
          {
            break;
          }

          if (hilightOffset >= currentOffset && hilightOffset < currentOffset + line.Length)
          {
            string gutter = $"{lineNo,4}: ";
            Console.WriteLine($"{gutter}{line}");
            int hilightStart = hilightOffset - currentOffset;
            string highlightIndicator = new string(' ', hilightStart + gutter.Length - 1) + new string('^', hilightLength);
            Console.WriteLine($"{highlightIndicator}");
            break;
          }

          lineNo++;
          currentOffset += line.Length + Environment.NewLine.Length; // Adding +1 for the newline character
        }
      }
    }
  }

  public static void PrintResults(string fileName, int hilightOffset, int hilightLength)
  {
    string[] lines = File.ReadAllLines(fileName);
    int currentOffset = 0;

    for (int i = 0; i < lines.Length; i++)
    {
      string line = lines[i];

      // Check if the highlight starts in this line
      if (hilightOffset >= currentOffset)
      {
        Console.WriteLine($"{i + 1}: {line}");

        // Calculate the starting index of the highlight in this line
        int highlightStart = hilightOffset - currentOffset;
        // Create a string of spaces and carets for highlighting
        string highlightIndicator = new string(' ', highlightStart) + new string('^', hilightLength);
        Console.WriteLine($"   {highlightIndicator}");
        break;
      }

      currentOffset += line.Length + Environment.NewLine.Length; // Adding +1 for the newline character
    }
  }
}
