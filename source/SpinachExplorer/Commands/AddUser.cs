namespace SpinachExplorer;

internal static partial class Program
{
  private static void AddUser()
  {
    Console.WriteLine();

    Console.Write("Enter user type (0 = Regular User, 1 = Organization) : ");
    string userTypeResponse = Console.ReadLine();
    ushort.TryParse(userTypeResponse, out ushort userType);

    Console.Write("Enter user name: ");
    string userName = Console.ReadLine();

    Console.Write("Enter user external id: ");
    string userExternalId = Console.ReadLine();

    Console.WriteLine();
    Console.WriteLine($"User Type: {userType}");
    Console.WriteLine($"User Name: {userName}");
    Console.WriteLine($"External Id: {userExternalId}");
    Console.Write("Add user? (y/n) : ");
    string confirm = Console.ReadLine();

    if (confirm.ToLower() == "y")
    {
      TextSearchManager.AddUser(userType, userName, userExternalId);
      TextSearchManager.Flush();
    }

    Pause();
  }
}
