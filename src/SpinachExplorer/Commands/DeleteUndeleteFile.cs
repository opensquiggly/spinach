namespace SpinachExplorer;

using Spinach.Interfaces;
using Spinach.Misc;

internal static partial class Program
{
  private static void DeleteUndeleteFile()
  {
    Console.WriteLine();
    Console.WriteLine("Current Repositories");
    Console.WriteLine("--------------------");

    foreach (IRepository repo in TextSearchManager.GetRepositories())
    {
      Console.Write($"User Type: {repo.UserType} ");
      Console.Write($"User Id: {repo.UserId} ");
      Console.Write($"Repo Type: {repo.Type} ");
      Console.Write($"Repo Id: {repo.Id} ");
      Console.Write($"Name: {repo.Name} ");
      Console.WriteLine($"Path: {repo.RootFolderPath}");
    }

    Console.WriteLine();

    ushort userType = PromptForUInt16Value("Enter User Type of Repository to Index");
    uint userId = PromptForUInt32Value("Enter User Id Of Repository to Index");
    ushort repoType = PromptForUInt16Value("Enter Repo Type of Repository to Index");
    uint repoId = PromptForUInt32Value("Enter Repo Id of Repository to Index");
    string name = PromptForString("Enter the name of the file to delete/undelete");
    ushort action = PromptForUInt16Value("Action: 1 for delete, 2 for undelete");
    DocStatus docStatus = action switch
    {
      1 => DocStatus.Deleted,
      2 => DocStatus.Normal,
      _ => DocStatus.Normal
    };

    bool success = TextSearchManager.SetDocStatus(userType, userId, repoType, repoId, name, docStatus);

    if (success)
    {
      Console.WriteLine("Success");
    }
    else
    {
      Console.WriteLine("File not found");
    }

    Pause();
  }
}
