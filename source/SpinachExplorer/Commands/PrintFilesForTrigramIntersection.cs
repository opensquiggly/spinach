namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForTrigramIntersection()
  {
    Console.WriteLine();

    Console.Write("Enter trigram 1: ");
    string trigram1 = Console.ReadLine();
    
    Console.Write("Enter trigram 2: ");
    string trigram2 = Console.ReadLine();

    int key1 = char.ToLower(trigram1[0]) * 128 * 128 + char.ToLower(trigram1[1]) * 128 + char.ToLower(trigram1[2]);
    int key2 = char.ToLower(trigram2[0]) * 128 * 128 + char.ToLower(trigram2[1]) * 128 + char.ToLower(trigram2[2]);

    DiskBTree<long, long> tree1 = TextSearchIndex.LoadTrigramFileIdTree(key1);
    DiskBTree<long, long> tree2 = TextSearchIndex.LoadTrigramFileIdTree(key2);

    var intersection = tree1.FastIntersect<long>(tree2);
    
    try
    {
      foreach (long fileId in intersection)
      {
        long nameAddress = TextSearchIndex.InternalFileIdTree.Find(fileId);
        var nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        Console.WriteLine($"Match on FileId = {fileId} : {nameString.GetValue()}");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }

    Pause();
  }
}
