namespace SpinachExplorer;

internal static partial class Program
{
  private static void OpenExistingIndexFile()
  {
    Console.WriteLine();
    string filename = PromptForString("Enter index file name");

    if (!File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' does not exist.");
      Console.WriteLine("Use option 1 from the Main Menu to create a new file");
      Pause();
      return;
    }

    TextSearchManager.OpenExistingIndexFile(filename);

    Console.WriteLine($"Index file {filename} is now open for exploration.");
    Pause();
  }
}
