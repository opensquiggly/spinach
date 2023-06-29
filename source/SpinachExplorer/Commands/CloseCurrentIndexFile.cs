namespace SpinachExplorer;

internal static partial class Program
{
  private static void CloseCurrentIndexFile()
  {
    if (TextSearchIndex.IsOpen)
    {
      Console.WriteLine();
      Console.WriteLine($"The index file '{TextSearchIndex.FileName}' is currently open.");
      Console.Write("Are you sure want to close it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() != "y")
      {
        return;
      }
      Console.WriteLine($"Closing current index file: {TextSearchIndex.FileName}");
    }

    TextSearchIndex.Close();
  }
}
