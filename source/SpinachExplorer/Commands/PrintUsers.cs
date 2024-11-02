namespace SpinachExplorer;

using Spinach.Blocks;
using Spinach.Interfaces;

internal static partial class Program
{
  private static void PrintUsers()
  {
    Console.WriteLine("Current Users");
    Console.WriteLine("-------------");

    foreach (IUser user in TextSearchManager.GetUsers())
    {
      Console.WriteLine($"User Id: {user.Id}, User Type: {user.Type}, Name: {user.Name}, External Id: {user.ExternalId}, Last Repo Id: {user.LastRepoId}");
    }

    Pause();
  }
}
