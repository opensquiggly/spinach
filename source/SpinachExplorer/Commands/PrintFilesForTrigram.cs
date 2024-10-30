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
    var docOffsetCursor = new DiskBTreeCursor<DocOffsetCompoundKeyBlock, uint>(TextSearchManager.DocTreeByOffset);
    int matches = 0;

    while (cursor.MoveNext())
    {
      DiskSortedVarIntList postingsList = TextSearchManager.DiskBlockManager.SortedVarIntListFactory.LoadExisting(cursor.CurrentData);
      var postingsListCursor = new DiskSortedVarIntListCursor(postingsList);
      while (postingsListCursor.MoveNext())
      {
        var docOffsetCompoundKey = new DocOffsetCompoundKeyBlock()
        {
          UserType = cursor.CurrentKey.UserType,
          UserId = cursor.CurrentKey.UserId,
          RepoType = cursor.CurrentKey.RepoType,
          RepoId = cursor.CurrentKey.RepoId,
          StartingOffset = postingsListCursor.CurrentKey
        };

        docOffsetCursor.MoveUntilGreaterThanOrEqual(docOffsetCompoundKey);
        if (docOffsetCursor.IsPastEnd || docOffsetCursor.CurrentKey.CompareTo(docOffsetCompoundKey) > 0)
        {
          docOffsetCursor.MovePrevious();
        }

        var docIdCompoundKey = new DocIdCompoundKeyBlock()
        {
          UserType = cursor.CurrentKey.UserType,
          UserId = cursor.CurrentKey.UserId,
          RepoType = cursor.CurrentKey.RepoType,
          RepoId = cursor.CurrentKey.RepoId,
          Id = docOffsetCursor.CurrentData
        };

        bool foundDoc =
          TextSearchManager.DocTree.TryFind(docIdCompoundKey, out DocInfoBlock docInfoBlock, out _, out _);

        if (!foundDoc)
        {
          Console.WriteLine("Could not find document");
        }
        else
        {
          string externalIdOrPath = TextSearchManager.LoadString(docInfoBlock.ExternalIdOrPathAddress);
          Console.WriteLine($"  @{postingsListCursor.CurrentKey - docInfoBlock.StartingOffset} : {externalIdOrPath}");
          matches++;
        }
      }
    }

    Console.WriteLine($"Total matches: {matches}");
    Console.WriteLine();
    Pause();
  }
}
