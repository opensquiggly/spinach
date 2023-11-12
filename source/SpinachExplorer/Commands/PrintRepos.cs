namespace SpinachExplorer;

using Spinach.Blocks;

internal static partial class Program
{
  private static void PrintRepos()
  {
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

    Pause();
  }
}
