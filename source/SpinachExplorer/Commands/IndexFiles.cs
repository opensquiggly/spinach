namespace SpinachExplorer;

internal static partial class Program
{
  private static void IndexFiles()
  {
    Console.WriteLine();

    Console.Write("Enter first internal file id: ");
    string firstResponse = Console.ReadLine();

    Console.Write("Enter last internal file id: ");
    string lastResponse = Console.ReadLine();

    if (int.TryParse(firstResponse, out int firstFileId) && int.TryParse(lastResponse, out int lastFileId))
    {
      TextSearchIndex.IndexFiles(firstFileId, lastFileId);
      TextSearchIndex.Flush();
    }

    Pause();
  }
}
