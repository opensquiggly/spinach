namespace SpinachExplorer;

internal static partial class Program
{
  private static void CreateNewIndexFile()
  {
    Console.WriteLine();
    Console.Write("Enter index file name: ");
    string filename = Console.ReadLine();

    if (File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' already exists.");
      Console.Write("Do you want to delete and recreate it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() == "y")
      {
        File.Delete(filename);
      }
      else
      {
        return;
      }
    }

    // TextSearchIndex.CreateNewIndexFile(filename);
    TextSearchManager.CreateNewIndexFile(filename);

    Pause();
  }
}
