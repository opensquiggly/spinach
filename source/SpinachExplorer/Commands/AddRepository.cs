namespace SpinachExplorer;

internal static partial class Program
{
  private static void AddRepository()
  {
    Console.WriteLine();

    ushort userType = PromptForUInt16Value("Enter user type of repository owner");
    uint userId = PromptForUInt32Value("Enter user id of repository owner");
    ushort repoType = PromptForUInt16Value("Enter repository type (0 = User Topic Pages, 1 = Git Repo)");
    string repoName = PromptForString("Enter repository name");
    string rootFolder = PromptForString("Enter repository root folder");
    string externalId = PromptForString("Enter external id");

    Console.WriteLine();
    Console.WriteLine($"Repository Name: {repoName}");
    Console.WriteLine($"Root Folder: {rootFolder}");

    if (PromptToConfirm("Add repository? (y/n)"))
    {
      TextSearchManager.AddRepository(userType, userId, repoType, externalId, repoName, rootFolder);
    }

    Pause();
  }
}
