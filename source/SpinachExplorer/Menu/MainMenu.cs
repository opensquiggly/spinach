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
      Console.WriteLine(" 4. Add repository");
      Console.WriteLine(" 5. Print repositories");
      Console.WriteLine(" 6. View files in a local folder");
      Console.WriteLine(" 7. Index local files");
      Console.WriteLine(" 8. Lookup file id");
      Console.WriteLine(" 9. Test trigram extractor");
      Console.WriteLine("10. Index files");
      Console.WriteLine("11. Print Files for Trigram");
      Console.WriteLine("12. Print Files for Literal");
      Console.WriteLine("13. Print Files for Intersection of Two Trigrams");
      Console.WriteLine("14. Print Files for Intersection of Two Literals");
      Console.WriteLine("15. Analyze Regex");
      Console.WriteLine("16. Find Files");
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
            AddRepository();
            break;

          case "5":
            PrintRepos();
            break;

          case "6":
            ViewFilesAndFolders();
            break;

          case "7":
            IndexLocalFiles();
            break;

          case "8":
            LookupFileId();
            break;

          case "9":
            TestTrigramExtractor();
            break;

          case "10":
            IndexFiles();
            break;

          case "11":
            PrintFilesForTrigram();
            break;

          case "12":
            PrintLiteralsForTrigram();
            break;

          case "13":
            PrintFilesForTrigramIntersection();
            break;

          case "14":
            PrintFilesForLiteralIntersection();
            break;

          case "15":
            AnalyzeRegex();
            break;

          case "16":
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
