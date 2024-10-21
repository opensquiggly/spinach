namespace SpinachExplorer;

internal static partial class Program
{
  private static void IndexLocalFiles()
  {
    Console.WriteLine("Indexing files ...");

    Console.WriteLine("Current Repositories");
    Console.WriteLine("--------------------");
    foreach (RepoInfoBlock repoInfoBlock in TextSearchIndex.GetRepositories())
    {
      string name = TextSearchIndex.LoadImmutableString(repoInfoBlock.NameAddress);
      string rootFolder = TextSearchIndex.LoadImmutableString(repoInfoBlock.RootFolderAddress);
      string externalId = TextSearchIndex.LoadImmutableString(repoInfoBlock.ExternalIdAddress);
      if (externalId == string.Empty)
      {
        externalId = "<<none>>";
      }
      Console.WriteLine($"InternalId: {repoInfoBlock.InternalId}, ExternalId: '{externalId}', Name: '{name}', Root Folder: '{rootFolder}'");
    }

    Console.Write("> Enter InternalId of Repository to Index: ");
    string response = Console.ReadLine();
    if (long.TryParse(response, out long internalId))
    {
      DiskBTreeCursor<long, RepoInfoBlock> cursor = TextSearchIndex.GetRepositoriesCursor();
      if (cursor.MoveUntilGreaterThanOrEqual(internalId))
      {
        RepoInfoBlock foundBlock = cursor.CurrentNode.GetDataAt(cursor.CurrentIndex);
        if (foundBlock.InternalId == internalId)
        {
          string rootFolder = TextSearchIndex.LoadImmutableString(foundBlock.RootFolderAddress);
          Console.WriteLine($"Ready to index repository stored at '{rootFolder}'");
          Console.Write("Continue? (y/n) : ");
          string confirm = Console.ReadLine();
          if (confirm != null && confirm.ToLower() == "y")
          {
            TextSearchIndex.IndexLocalFiles(0, 1, (uint) internalId, rootFolder);
            TextSearchIndex.Flush();
          }
        }
      }
      else
      {
        Console.WriteLine($"Repository with InternalId={internalId} was not found");
      }
    }
    else
    {
      Console.WriteLine("Invalid id");
    }

    Pause();
  }
}
