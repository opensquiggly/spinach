namespace SpinachExplorer;

internal static partial class Program
{
  private static void AddRepository()
  {
    Console.WriteLine();

    Console.Write("Enter user type of repository owner: ");
    string userTypeResponse = Console.ReadLine();
    ushort.TryParse(userTypeResponse, out ushort userType);

    Console.Write("Enter user id of repository owner: ");
    string userIdResponse = Console.ReadLine();
    uint.TryParse(userIdResponse, out uint userId);

    Console.Write("Enter repository type (0 = User Topic Pages, 1 = Git Repo): ");
    string repoTypeResponse = Console.ReadLine();
    ushort.TryParse(repoTypeResponse, out ushort repoType);

    Console.Write("Enter repository name: ");
    string repoName = Console.ReadLine();

    Console.Write("Enter repository root folder: ");
    string rootFolder = Console.ReadLine();

    Console.Write("Enter external id: ");
    string externalId = Console.ReadLine();

    Console.WriteLine();
    Console.WriteLine($"Repository Name: {repoName}");
    Console.WriteLine($"Root Folder: {rootFolder}");
    Console.Write("Add repository? (y/n) : ");
    string confirm = Console.ReadLine();

    if (confirm.ToLower() == "y")
    {
      TextSearchManager.AddRepository(userType, userId, repoType, externalId, repoName, rootFolder);
      // TextSearchIndex.AddRepository(null, repoName, rootFolder);
      // TextSearchIndex.Flush();
    }

    Pause();
  }
}
