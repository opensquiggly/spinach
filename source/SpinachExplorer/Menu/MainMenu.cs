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
      Console.WriteLine(" 4. View files in a local folder");
      Console.WriteLine(" 5. Index local files");
      Console.WriteLine(" 6. Lookup file id");
      Console.WriteLine(" 7. Test trigram extractor");
      Console.WriteLine(" 8. Index files");
      Console.WriteLine(" 9. Print Files for Trigram");
      Console.WriteLine("10. Print Files for Intersection of Two Trigrams");
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
            ViewFilesAndFolders();
            break;

          case "5":
            IndexLocalFiles();
            break;

          case "6":
            LookupFileId();
            break;

          case "7":
            TestTrigramExtractor();
            break;

          case "8":
            IndexFiles();
            break;

          case "9":
            PrintFilesForTrigram();
            break;

          case "10":
            PrintFilesForTrigramIntersection();
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
