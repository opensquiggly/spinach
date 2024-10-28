namespace SpinachExplorer;

using Spinach.Blocks;
using Spinach.Interfaces;

internal static partial class Program
{
  private static void PrintRepos()
  {
    Console.WriteLine("Current Repositories");
    Console.WriteLine("--------------------");
    foreach (IRepository repo in TextSearchManager.GetRepositories())
    {
      Console.WriteLine($"User Type: {repo.UserType}, User Id: {repo.UserId}, Type: {repo.Type}, Id: {repo.Id}, Name: {repo.Name}, External Id: {repo.ExternalId}, Last Doc Id: {repo.LastDocId}, Root Folder Path: {repo.RootFolderPath}");
    }

    Pause();
  }
}
