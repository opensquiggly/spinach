namespace SpinachExplorer;

internal static partial class Program
{
  private static void AddUser()
  {
    Console.WriteLine();

    ushort userType = PromptForUInt16Value("Enter user type (0 = Regular User, 1 = Organization)");
    string userName = PromptForString("Enter user name");
    string userExternalId = PromptForString("Enter user external id");

    Console.WriteLine();
    Console.WriteLine($"User Type: {userType}");
    Console.WriteLine($"User Name: {userName}");
    Console.WriteLine($"External Id: {userExternalId}");

    if (PromptToConfirm("Add user? (y/n)"))
    {
      TextSearchManager.AddUser(userType, userName, userExternalId);
      TextSearchManager.Flush();
    }

    Pause();
  }
}
