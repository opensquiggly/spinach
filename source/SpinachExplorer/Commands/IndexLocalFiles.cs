namespace SpinachExplorer;

internal static partial class Program
{
  private static void IndexLocalFiles()
  {
    Console.WriteLine("Indexing files ...");
    TextSearchIndex.IndexLocalFiles("/home/kdietz/spinach-repos/OpenSquiggly");
    Pause();
  }
}
