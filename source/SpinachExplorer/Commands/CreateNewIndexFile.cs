namespace SpinachExplorer;

internal static partial class Program
{
  private static void CreateNewIndexFile()
  {
    Console.WriteLine();
    string filename = PromptForString("Enter index file name");

    if (File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' already exists.");

      if (PromptToConfirm("Do you want to delete and recreate it? (y/n)"))
      {
        File.Delete(filename);
      }
      else
      {
        return;
      }
    }

    TextSearchManager.CreateNewIndexFile(filename);

    Pause();
  }
}
