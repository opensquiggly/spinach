namespace SpinachExplorer;

internal static partial class Program
{
  private static void PrintFilesForTrigram()
  {
    Console.WriteLine();
    Console.Write("Enter trigram: ");
    string trigram = Console.ReadLine();

    if (trigram is not { Length: 3 }) return;

    int key = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);

    bool trigramFound = TextSearchManager.TrigramTree.TryFind(key, out long trigramMatchesAddress, out _, out _);
    if (!trigramFound)
    {
      Console.WriteLine("No matches for the trigram.");
      Console.WriteLine();
      Pause();

      return;
    }

    DiskBTree<TrigramMatchKey, long> trigramMatches = TextSearchManager.TrigramMatchesFactory.LoadExisting(trigramMatchesAddress);
    var cursor = new DiskBTreeCursor<TrigramMatchKey, long>(trigramMatches);

    while (cursor.MoveNext())
    {
      Console.WriteLine($"User Type = {cursor.CurrentKey.UserType}, User Id = {cursor.CurrentKey.UserId}, RepoType = {cursor.CurrentKey.RepoType}, RepoId = {cursor.CurrentKey.RepoId}");
      DiskSortedVarIntList postingsList = TextSearchManager.DiskBlockManager.SortedVarIntListFactory.LoadExisting(cursor.CurrentData);
      var postingsListCursor = new DiskSortedVarIntListCursor(postingsList);
      while (postingsListCursor.MoveNext())
      {
        Console.WriteLine($"  Offset = {postingsListCursor.CurrentKey}");
      }
    }

    Console.WriteLine();
    Pause();
  }
}
