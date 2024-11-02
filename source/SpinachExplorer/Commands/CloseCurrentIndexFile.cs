namespace SpinachExplorer;

internal static partial class Program
{
  private static void CloseCurrentIndexFile()
  {
    Console.WriteLine();

    if (!TextSearchManager.IsOpen)
    {
      Console.WriteLine("There is no index file that is currently open.");
      Pause();
      return;
    }

    Console.WriteLine($"The index file '{TextSearchManager.FileName}' is currently open.");
    if (!PromptToConfirm("Are you sure want to close it? (y/n)")) return;
    Console.WriteLine($"Closing current index file: {TextSearchManager.FileName}");

    TextSearchManager.Close();
  }
}
