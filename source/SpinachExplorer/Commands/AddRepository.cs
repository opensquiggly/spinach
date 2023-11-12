namespace SpinachExplorer;

internal static partial class Program
{
  private static void AddRepository()
  {
    Console.WriteLine();

    Console.Write("Enter repository name: ");
    string repoName = Console.ReadLine();

    Console.Write("Enter repository root folder: ");
    string rootFolder = Console.ReadLine();

    Console.WriteLine();
    Console.WriteLine($"Repository: {repoName}");
    Console.WriteLine($"Root Folder: {rootFolder}");
    Console.Write("Add repository? (y/n) : ");
    string confirm = Console.ReadLine();

    if (confirm.ToLower() == "y")
    {
      TextSearchIndex.AddRepository(null, repoName, rootFolder);
      TextSearchIndex.Flush();
    }

    Pause();
  }
}
