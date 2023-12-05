namespace SpinachExplorer;

internal static partial class Program
{
  private static void IndexFiles()
  {
    Console.WriteLine();

    Console.Write("Ready to index files. Continue? (y/n) : ");
    string confirm = Console.ReadLine();

    if (confirm.ToLower() != "y")
    {
      return;
    }

    TextSearchIndex.IndexFiles();
    TextSearchIndex.Flush();

    Pause();
  }
}
