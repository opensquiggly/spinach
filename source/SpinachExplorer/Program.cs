using Eugene;
using Eugene.Blocks;
using Eugene.Collections;

internal static class Program
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  static Program()
  {
    DiskBlockManager = new DiskBlockManager();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Application Entry Point
  // /////////////////////////////////////////////////////////////////////////////////////////////

  internal static void Main(string[] args)
  {
    ClearScreen();
    Console.WriteLine("Hello, World!");

    bool finished = false;

    while (!finished)
    {
      ClearScreen();
      Console.WriteLine("\nWelcome to Spinach Explorer");
      Console.WriteLine();
      PrintCurrentIndexFileStatus();
      Console.WriteLine();
      Console.WriteLine("Main Menu");
      Console.WriteLine("---------");
      Console.WriteLine("1. Create and open new index file");
      Console.WriteLine("2. Open an existing index file");
      Console.WriteLine("3. Close current index file");
      Console.WriteLine("4. View files in a local folder");
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
            break;

          case "x":
            finished = true;
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
    }
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static DiskBlockManager DiskBlockManager { get; }

  private static string FileName { get; set; }

  private static bool IsOpen { get; set; } = false;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static void ClearScreen() => Console.Write("\u001b[2J\u001b[H");

  private static void Pause()
  {
    Console.WriteLine();
    Console.Write("Press <Enter> to return to Main Menu ... ");
    Console.ReadLine();
  }

  private static void ViewFilesAndFolders()
  {
    foreach (string filePath in Directory.GetFiles("/home/codespace/OpenSquiggly", "*.*", SearchOption.AllDirectories))
    {
      Console.WriteLine(filePath);
    }

    Pause();
  }

  private static void CreateNewIndexFile()
  {
    Console.WriteLine();
    Console.Write("Enter index file name: ");
    string filename = Console.ReadLine();

    if (File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' already exists.");
      Console.Write("Do you want to delete and recreate it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() == "y")
      {
        File.Delete(filename);
      }
      else
      {
        return;
      }
    }

    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);

    // HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();
    // headerBlock.Address1 = DataStructureInfoList.Address;

    // DiskBlockManager.WriteHeaderBlock(ref headerBlock);

    // Console.WriteLine($"Data Structure List Stored at Address: {headerBlock.Address1}");

    IsOpen = true;
    FileName = filename;
  }

  private static void PrintCurrentIndexFileStatus()
  {
    if (IsOpen)
    {
      Console.WriteLine($"The index file '{FileName}' is currently open");
    }
    else
    {
      Console.WriteLine("There is no current index file open");
    }
  }

  private static void OpenExistingIndexFile()
  {
    Console.WriteLine();
    Console.Write("Enter index file name: ");
    string filename = Console.ReadLine();

    if (!File.Exists(filename))
    {
      Console.WriteLine($"File name '{filename}' does not exist.");
      Console.WriteLine("Use option 1 from the Main Menu to create a new file");
      return;
    }

    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);

    // HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();

    // DiskLinkedListFactory<DataStructureInfoBlock> factory = DiskBlockManager.LinkedListManager.CreateFactory<DataStructureInfoBlock>(DataStructureBlockTypeIndex);
    // DataStructureInfoList = factory.LoadExisting(headerBlock.Address1);

    // Console.WriteLine($"Data Structure List Loaded from Address: {headerBlock.Address1}");
    // Console.WriteLine($"There are {DataStructureInfoList.Count} data structures");

    IsOpen = true;
    FileName = filename;

    Console.WriteLine($"File name {filename} is now open for exploration.");
  }


  private static void CloseCurrentIndexFile()
  {
    if (IsOpen)
    {
      Console.WriteLine();
      Console.WriteLine($"The index file '{FileName}' is currently open.");
      Console.Write("Are you sure want to close it? (y/n) : ");
      string response = Console.ReadLine();
      if (response.ToLower() != "y")
      {
        return;
      }
      Console.WriteLine($"Closing current index file: {FileName}");
    }

    IsOpen = false;
  }
}

