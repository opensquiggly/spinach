namespace SpinachExplorer;

using Spinach.Interfaces;

internal static partial class Program
{
  private static void LookupFileByName()
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
    string name = PromptForString("Enter the name of the file to look up");

    ulong docId;
    var docInfoBlock = new DocInfoBlock();
    bool found = TextSearchManager.TryFindDocument(userType, userId, repoType, repoId, name, out docId, out docInfoBlock, out _, out _);

    if (found)
    {
      Console.WriteLine($"Doc Id = {docId}");
    }
    else
    {
      Console.WriteLine($"File '{name}' not found in repository.");
    }

    Pause();
  }
}
