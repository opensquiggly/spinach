namespace SpinachExplorer;

internal static partial class Program
{
  private static void ViewFilesAndFolders()
  {
    foreach (string filePath in Directory.GetFiles("/home/kdietz/dev/OpenSquiggly", "*.*", SearchOption.AllDirectories))
    {
      Console.WriteLine(filePath);
    }

    Pause();
  }
}
