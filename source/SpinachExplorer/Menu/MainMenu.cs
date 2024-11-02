namespace SpinachExplorer;

internal static partial class Program
{
  internal static ushort PromptForUInt16Value(string prompt)
  {
    bool valid;
    ushort result;

    do
    {
      Console.Write($"{prompt} : ");
      string response = Console.ReadLine();
      valid = ushort.TryParse(response, out result);

      if (!valid)
      {
        Console.WriteLine($"{response} is not a valid 16-bit value. Try again.");
        Console.WriteLine();
      }
    } while (!valid);

    return result;
  }

  internal static uint PromptForUInt32Value(string prompt)
  {
    bool valid;
    uint result;

    do
    {
      Console.Write($"{prompt} : ");
      string response = Console.ReadLine();
      valid = uint.TryParse(response, out result);

      if (!valid)
      {
        Console.WriteLine($"{response} is not a valid 16-bit value. Try again.");
        Console.WriteLine();
      }
    } while (!valid);

    return result;
  }

  internal static string PromptForString(string prompt)
  {
    Console.Write($"{prompt} : ");
    string response = Console.ReadLine();

    return response;
  }

  internal static bool PromptToConfirm(string prompt)
  {
    Console.Write($"{prompt} : ");
    string response = Console.ReadLine();

    return response is "y" or "Y";
  }

  internal static void MainMenu()
  {
    ClearScreen();

    bool finished = false;

    while (!finished)
    {
      ClearScreen();
      Console.WriteLine("\nWelcome to Spinach Explorer");
      Console.WriteLine();

      if (TextSearchManager.IsOpen)
      {
        Console.WriteLine($"The index file '{TextSearchManager.FileName}' is currently open");
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
      Console.WriteLine(" 9. Add repository file names to the index");
      Console.WriteLine("10. Print indexed documents (file names)");
      Console.WriteLine("11. Lookup file id");
      Console.WriteLine("12. Test trigram extractor");
      Console.WriteLine("13. Index files");
      Console.WriteLine("14. Print Files for Trigram");
      Console.WriteLine("15. Print Files for Literal");
      Console.WriteLine("16. Print Files for Intersection of Two Trigrams");
      Console.WriteLine("17. Print Files for Intersection of Two Literals");
      Console.WriteLine("18. Analyze Regex");
      Console.WriteLine("19. Find Files");
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
            PrintIndexedDocuments();
            break;

          case "11":
            LookupFileId();
            break;

          case "12":
            TestTrigramExtractor();
            break;

          case "13":
            IndexFiles();
            break;

          case "14":
            PrintFilesForTrigram();
            break;

          case "15":
            PrintLiteralsForTrigram();
            break;

          case "16":
            PrintFilesForTrigramIntersection();
            break;

          case "17":
            PrintFilesForLiteralIntersection();
            break;

          case "18":
            AnalyzeRegex();
            break;

          case "19":
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
