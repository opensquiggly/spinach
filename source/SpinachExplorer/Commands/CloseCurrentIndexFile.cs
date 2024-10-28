namespace SpinachExplorer;

internal static partial class Program
{
  private static void CloseCurrentIndexFile()
  {
    if (TextSearchManager.IsOpen)
    {
      Console.WriteLine();
      Console.WriteLine($"The index file '{TextSearchManager.FileName}' is currently open.");

      if (PromptToConfirm("Are you sure want to close it? (y/n)"))
      {
        Console.WriteLine($"Closing current index file: {TextSearchManager.FileName}");
      }
    }

    TextSearchManager.Close();
  }
}
