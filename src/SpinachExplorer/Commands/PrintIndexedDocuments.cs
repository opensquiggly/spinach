namespace SpinachExplorer;

using Spinach.Interfaces;

internal static partial class Program
{
  private static void PrintIndexedDocuments()
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

    foreach (IDocument doc in TextSearchManager.GetDocuments(userType, userId, repoType, repoId))
    {
      Console.Write($"User Type: {doc.UserType} ");
      Console.Write($"User Id: {doc.UserId} ");
      Console.Write($"Repo Type: {doc.RepoType} ");
      Console.Write($"Repo Id: {doc.RepoId} ");
      Console.Write($"Doc Id: {doc.DocId} ");
      Console.Write($"Status: {doc.Status} ");
      Console.Write($"IsIndexed: {doc.IsIndexed}");
      Console.Write($"Starting Offset: {doc.StartingOffset} ");
      Console.Write($"Length: {doc.OriginalLength} ");
      Console.WriteLine();
      Console.WriteLine($"{doc.ExternalIdOrPath}");
      Console.WriteLine();
    }

    Pause();
  }
}
