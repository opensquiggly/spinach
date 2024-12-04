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
      Console.Write($"User Type: {repo.UserType} ");
      Console.Write($"User Id: {repo.UserId} ");
      Console.Write($"Type: {repo.Type} ");
      Console.Write($"Id: {repo.Id} ");
      Console.Write($"Name: {repo.Name} ");
      Console.Write($"External Id: {repo.ExternalId} ");
      Console.Write($"Last Doc Id: {repo.LastDocId} ");
      Console.Write($"Last Doc Length: {repo.LastDocLength} ");
      Console.Write($"Last Doc Starting Offset: {repo.LastDocStartingOffset} ");
      Console.Write($"Root Folder Path: {repo.RootFolderPath}");
      Console.WriteLine();
    }

    Pause();
  }
}
