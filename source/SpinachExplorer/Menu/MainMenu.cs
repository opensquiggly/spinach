namespace SpinachExplorer;

internal static partial class Program
{
  internal static void MainMenu()
  {
    ClearScreen();

    bool finished = false;

    while (!finished)
    {
      ClearScreen();
      Console.WriteLine("\nWelcome to Spinach Explorer");
      Console.WriteLine();

      if (TextSearchIndex.IsOpen)
      {
        Console.WriteLine($"The index file '{TextSearchIndex.FileName}' is currently open");
      }
      else
      {
        Console.WriteLine("There is no current index file open");
      }

      Console.WriteLine();
      Console.WriteLine("Main Menu");
      Console.WriteLine("---------");
      Console.WriteLine(" 1. Create and open new index file");
      Console.WriteLine(" 2. Open an existing index file");
      Console.WriteLine(" 3. Close current index file");
      Console.WriteLine(" 4. Add user");
      Console.WriteLine(" 5. Print users");
      Console.WriteLine(" 6. Add repository");
      Console.WriteLine(" 7. Print repositories");
      Console.WriteLine(" 8. View files in a local folder");
      Console.WriteLine(" 9. Index local files");
      Console.WriteLine("10. Lookup file id");
      Console.WriteLine("11. Test trigram extractor");
      Console.WriteLine("12. Index files");
      Console.WriteLine("13. Print Files for Trigram");
      Console.WriteLine("14. Print Files for Literal");
      Console.WriteLine("15. Print Files for Intersection of Two Trigrams");
      Console.WriteLine("16. Print Files for Intersection of Two Literals");
      Console.WriteLine("17. Analyze Regex");
      Console.WriteLine("18. Find Files");
      Console.WriteLine("X. Exit program");
      Console.WriteLine();
      Console.Write("Enter selection: ");

      string response = Console.ReadLine();

      try
      {
        switch (response.ToLower())
        {
          case "1":
            CreateNewIndexFile();
            break;

          case "2":
            OpenExistingIndexFile();
            break;

          case "3":
            CloseCurrentIndexFile();
            break;

          case "4":
            AddUser();
            break;

          case "5":
            PrintUsers();
            break;

          case "6":
            AddRepository();
            break;

          case "7":
            PrintRepos();
            break;

          case "8":
            ViewFilesAndFolders();
            break;

          case "9":
            IndexLocalFiles();
            break;

          case "10":
            LookupFileId();
            break;

          case "11":
            TestTrigramExtractor();
            break;

          case "12":
            IndexFiles();
            break;

          case "13":
            PrintFilesForTrigram();
            break;

          case "14":
            PrintLiteralsForTrigram();
            break;

          case "15":
            PrintFilesForTrigramIntersection();
            break;

          case "16":
            PrintFilesForLiteralIntersection();
            break;

          case "17":
            AnalyzeRegex();
            break;

          case "18":
            FindFiles();
            break;

          case "x":
            finished = true;
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
        Pause();
      }
    }
  }
}
