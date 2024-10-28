namespace SpinachExplorer;

internal static partial class Program
{
  private static void IndexLocalFiles()
  {
    Console.WriteLine("Indexing files ...");

    Console.WriteLine("Current Repositories");
    Console.WriteLine("--------------------");
    foreach (var repo in TextSearchManager.GetRepositories())
    {
      Console.Write($"User Type: {repo.UserType} ");
      Console.Write($"User Id: {repo.UserId} ");
      Console.Write($"Repo Type: {repo.Type} ");
      Console.Write($"Repo Id: {repo.Id} ");
      Console.Write($"Name: {repo.Name} ");
      Console.WriteLine($"Path: {repo.RootFolderPath}");
    }

    Console.WriteLine();
    Console.Write("> Enter User Type of Repository to Index: ");
    string userTypeText = Console.ReadLine();
    Console.Write("> Enter User Id Of Repository to Index: ");
    string userIdText = Console.ReadLine();
    Console.Write("> Enter Repo Type of Repository to Index: ");
    string repoTypeText = Console.ReadLine();
    Console.Write("> Enter Repo Id of Repository to Index: ");
    string repoIdText = Console.ReadLine();

    UInt16.TryParse(userTypeText, out ushort userType);
    UInt32.TryParse(userIdText, out uint userId);
    UInt16.TryParse(repoTypeText, out ushort repoType);
    UInt32.TryParse(repoIdText, out uint repoId);

    TextSearchManager.IndexLocalFiles(userType, userId, repoType, repoId);

    // if (long.TryParse(response, out long internalId))
    // {
    //   DiskBTreeCursor<long, RepoInfoBlock> cursor = TextSearchIndex.GetRepositoriesCursor();
    //   if (cursor.MoveUntilGreaterThanOrEqual(internalId))
    //   {
    //     RepoInfoBlock foundBlock = cursor.CurrentNode.GetDataAt(cursor.CurrentIndex);
    //     if (foundBlock.InternalId == internalId)
    //     {
    //       string rootFolder = TextSearchIndex.LoadImmutableString(foundBlock.RootFolderPathAddress);
    //       Console.WriteLine($"Ready to index repository stored at '{rootFolder}'");
    //       Console.Write("Continue? (y/n) : ");
    //       string confirm = Console.ReadLine();
    //       if (confirm != null && confirm.ToLower() == "y")
    //       {
    //         TextSearchIndex.IndexLocalFiles(0, 1, (uint) internalId, rootFolder);
    //         TextSearchIndex.Flush();
    //       }
    //     }
    //   }
    //   else
    //   {
    //     Console.WriteLine($"Repository with InternalId={internalId} was not found");
    //   }
    // }
    // else
    // {
    //   Console.WriteLine("Invalid id");
    // }

    Pause();
  }
}
