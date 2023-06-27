using Eugene;
using Eugene.Blocks;
using Eugene.Collections;
using Spinach.Trigrams;
using SpinachExplorer;
using SpinachExplorer.Caching;
using System.Threading.Channels;
using System.Xml;

internal static class Program
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  static Program()
  {
    DiskBlockManager = new DiskBlockManager();
    TrigramKeyType = DiskBlockManager.RegisterBlockType<TrigramKey>();

    // The FileIdTree is used to look up file names based on their internal id
    // Given a key of internal file id, it returns an address to a DiskImmutableString
    FileIdTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<long, long>(
        DiskBlockManager.LongBlockType,
        DiskBlockManager.LongBlockType
      );

    // The TrigramTree is used to look up a trigram and figure out which TrigramFileTree goes with it.
    // Given a key representing a trigram, it returns an address a a TrigramFileTree BTree
    TrigramTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<int, long>(
        DiskBlockManager.IntBlockType,
        DiskBlockManager.LongBlockType
      );

    // The TrigramFileTree is used to look up a file for a given trigram. Each individual trigram
    // corresponds to a BTree of file ids that contain that trigram. Given a key of an internal
    // file id, it returns an address to a DiskLinkedList<long> which is a list that contains the
    // position(s) where that trigram appears in the file.
    TrigramFileTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<long, long>(
        DiskBlockManager.LongBlockType,
        DiskBlockManager.LongBlockType
      );

    LinkedListOfLongFactory =
      DiskBlockManager.LinkedListManager.CreateFactory<long>(DiskBlockManager.LongBlockType);

    TrigramFileIdTreeCache = new LruCache<int, DiskBTree<long, long>>(2200000);
    PostingsListCache = new LruCache<Tuple<int, long>, DiskLinkedList<long>>(2200000);
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
      Console.WriteLine("5. Index local files");
      Console.WriteLine("6. Lookup file id");
      Console.WriteLine("7. Test trigram extractor");
      Console.WriteLine("8. Index files");
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

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static DiskBlockManager DiskBlockManager { get; }

  private static string FileName { get; set; }

  private static bool IsOpen { get; set; } = false;

  private static DiskBTree<long, long> InternalFileIdTree { get; set; }

  private static DiskBTree<int, long> TrigramTree { get; set; }

  private static short TrigramKeyType { get; set; }

  private static DiskBTreeFactory<long, long> FileIdTreeFactory { get; set; }

  private static DiskBTreeFactory<int, long> TrigramTreeFactory { get; set; }

  private static DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private static DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  private static LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; set; }

  private static LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

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
    foreach (string filePath in Directory.GetFiles("/home/kdietz/dev/OpenSquiggly", "*.*", SearchOption.AllDirectories))
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

    InternalFileIdTree = FileIdTreeFactory.AppendNew(100);
    TrigramTree = TrigramTreeFactory.AppendNew(100);

    HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();
    headerBlock.Address1 = InternalFileIdTree.Address;
    headerBlock.Address2 = TrigramTree.Address;

    DiskBlockManager.WriteHeaderBlock(ref headerBlock);

    Console.WriteLine($"InternalFileIdTree Stored at Address: {headerBlock.Address1}");
    Console.WriteLine($"TrigramTree Stored at Address: {headerBlock.Address2}");

    IsOpen = true;
    FileName = filename;

    Pause();
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
      Pause();
      return;
    }

    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);

    HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();

    InternalFileIdTree = FileIdTreeFactory.LoadExisting(headerBlock.Address1);
    TrigramTree = TrigramTreeFactory.LoadExisting(headerBlock.Address2);

    Console.WriteLine($"InternalFileIdTree Loaded from Address: {headerBlock.Address1}");
    Console.WriteLine($"TrigramTree Loaded from Address: {headerBlock.Address2}");

    IsOpen = true;
    FileName = filename;

    Console.WriteLine($"Index file {filename} is now open for exploration.");
    Pause();
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

  private static void IndexLocalFiles()
  {
    long currentFileId = 1;

    Console.WriteLine("Indexing files ...");

    foreach (string filePath in Directory.GetFiles("/home/kdietz/spinach-repos/OpenSquiggly", "*.*", SearchOption.AllDirectories))
    {
      Console.WriteLine($"{currentFileId} : {filePath}");
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(filePath);
      InternalFileIdTree.Insert(currentFileId, nameString.Address);
      currentFileId++;
    }

    Pause();
  }

  private static void LookupFileId()
  {
    Console.WriteLine();

    Console.Write("Enter first internal file id: ");
    string firstResponse = Console.ReadLine();

    Console.Write("Enter last internal file id: ");
    string lastResponse = Console.ReadLine();

    if (int.TryParse(firstResponse, out int firstFileId) && int.TryParse(lastResponse, out int lastFileId))
    {
      for (int fileId = firstFileId; fileId <= lastFileId; fileId++)
      {
        long nameAddress = InternalFileIdTree.Find(fileId);
        DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        Console.WriteLine($"{fileId}: {nameString.GetValue()}");
      }
    }
    else
    {
      Console.WriteLine("Invalid Response. Enter an integer value next time.");
    }

    Pause();
  }

  private static bool FileIsBinary(string filename)
  {
    byte[] buffer = new byte[8192];

    using (FileStream fileStream = File.OpenRead(filename))
    {
      int bytesRead = fileStream.Read(buffer, 0, buffer.Length);

      for (int i = 0; i < bytesRead; i++)
      {
        if (buffer[i] < 0x09)
        {
          return true; // File contains a null byte or other control character; assume binary
        }
      }
    }

    return false;
  }

  private static void TestTrigramExtractor()
  {
    // Console.WriteLine();
    // Console.Write("Enter a string: ");
    // string response = Console.ReadLine();

    Console.WriteLine();

    Console.Write("Enter first internal file id: ");
    string firstResponse = Console.ReadLine();

    Console.Write("Enter last internal file id: ");
    string lastResponse = Console.ReadLine();

    if (int.TryParse(firstResponse, out int firstFileId) && int.TryParse(lastResponse, out int lastFileId))
    {
      for (int fileId = firstFileId; fileId <= lastFileId; fileId++)
      {
        long nameAddress = InternalFileIdTree.Find(fileId);
        DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        string name = nameString.GetValue();

        if (name.Contains("/.git/"))
        {
          Console.Write(".");
          continue;
        }

        if (name.Contains("/obj/"))
        {
          Console.Write(".");
          continue;
        }

        if (name.Contains("/bin/"))
        {
          Console.Write(".");
          continue;
        }

        if (name.Contains("node_modules"))
        {
          Console.Write(".");
          continue;
        }

        if (name.EndsWith(".jpg") || name.EndsWith(".gif") || name.EndsWith(".png") || name.EndsWith(".dll"))
        {
          Console.Write(".");
          continue;
        }

        if (FileIsBinary(name))
        {
          Console.Write(".");
          continue;
        }

        string content = File.ReadAllText(name);

        var trigramExtractor = new TrigramExtractor(content);
        int count = 0;

        foreach (TrigramInfo trigramInfo in trigramExtractor)
        {
          char ch1 = (char)(trigramInfo.Key / 128L / 128L);
          char ch2 = (char)(trigramInfo.Key % (128L * 128L) / 128L);
          char ch3 = (char)(trigramInfo.Key % 128L);
          count++;
          Console.Write(".");

          // Console.WriteLine($"Key = {trigramInfo.Key}  Position = {trigramInfo.Position}");
        }

        Console.WriteLine($"{fileId} : {count} : {nameString.GetValue()}");
        // Console.WriteLine($"{fileId} : {count}");
      }
    }

    Pause();
  }

  private static DiskBTree<long, long> LoadOrAddTrigramFileIdTree(int trigramKey, long fileId, out bool created)
  {
    created = false;

    if (TrigramFileIdTreeCache.TryGetValue(trigramKey, out DiskBTree<long, long> btree))
    {
      return btree;
    }

    if (TrigramTree.TryFind(trigramKey, out long trigramFileIdTreeAddress))
    {
      DiskBTree<long, long> trigramFileIdTree =
        TrigramFileTreeFactory.LoadExisting(trigramFileIdTreeAddress);

      TrigramFileIdTreeCache.Add(trigramKey, trigramFileIdTree);
      return trigramFileIdTree;
    }

    DiskBTree<long, long> newTrigramFileIdTree = TrigramFileTreeFactory.AppendNew(100);
    DiskLinkedList<long> linkedList = LinkedListOfLongFactory.AppendNew();
    newTrigramFileIdTree.Insert(fileId, linkedList.Address);
    TrigramTree.Insert(trigramKey, newTrigramFileIdTree.Address);
    TrigramFileIdTreeCache.Add(trigramKey, newTrigramFileIdTree);
    created = true;

    return newTrigramFileIdTree;
  }

  private static DiskLinkedList<long> LoadOrAddPostingsList(int trigramKey, long fileId)
  {
    var key = new Tuple<int, long>(trigramKey, fileId);

    if (PostingsListCache.TryGetValue(key, out DiskLinkedList<long> postingsList))
    {
      return postingsList;
    }

    DiskBTree<long, long> trigramFileIdTree = LoadOrAddTrigramFileIdTree(trigramKey, fileId, out bool created);
    if (!created)
    {
      if (!trigramFileIdTree.TryFind(fileId, out long postingsListAddress))
      {
        DiskLinkedList<long> newPostingsList1 = LinkedListOfLongFactory.AppendNew();
        trigramFileIdTree.Insert(fileId, newPostingsList1.Address);
        PostingsListCache.Add(key, newPostingsList1);

        return newPostingsList1;
      }

      DiskLinkedList<long> diskPostingsList = LinkedListOfLongFactory.LoadExisting(postingsListAddress);
      PostingsListCache.Add(key, diskPostingsList);

      return diskPostingsList;
    }

    DiskLinkedList<long> newPostingsList2 = LinkedListOfLongFactory.AppendNew();
    trigramFileIdTree.Insert(fileId, newPostingsList2.Address);
    PostingsListCache.Add(key, newPostingsList2);

    return newPostingsList2;
  }

  private static void IndexFiles()
  {
    Console.WriteLine();

    Console.Write("Enter first internal file id: ");
    string firstResponse = Console.ReadLine();

    Console.Write("Enter last internal file id: ");
    string lastResponse = Console.ReadLine();

    if (int.TryParse(firstResponse, out int firstFileId) && int.TryParse(lastResponse, out int lastFileId))
    {
      for (int fileId = firstFileId; fileId <= lastFileId; fileId++)
      {
        long nameAddress = InternalFileIdTree.Find(fileId);
        DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        string name = nameString.GetValue();

        if (name.Contains("/.git/"))
        {
          continue;
        }

        if (name.Contains("/obj/"))
        {
          continue;
        }

        if (name.Contains("/bin/"))
        {
          continue;
        }

        if (name.Contains("node_modules"))
        {
          continue;
        }

        if (name.EndsWith(".jpg") || name.EndsWith(".gif") || name.EndsWith(".png") || name.EndsWith(".dll"))
        {
          continue;
        }

        if (FileIsBinary(name))
        {
          continue;
        }

        Console.Write($"Indexing {fileId} : {name} ...");

        string content = File.ReadAllText(name);

        var trigramExtractor = new TrigramExtractor(content);
        int count = 0;

        foreach (TrigramInfo trigramInfo in trigramExtractor)
        {
          char ch1 = (char)(trigramInfo.Key / 128L / 128L);
          char ch2 = (char)(trigramInfo.Key % (128L * 128L) / 128L);
          char ch3 = (char)(trigramInfo.Key % 128L);

          DiskLinkedList<long> postingsList = LoadOrAddPostingsList(trigramInfo.Key, fileId);
          postingsList.AddLast(trigramInfo.Position);

          // if (!TrigramTree.TryFind(trigramInfo.Key, out long trigramFileIdTreeAddress))
          // {
          //   DiskBTree<long, long> trigramFileIdTree = TrigramFileTreeFactory.AppendNew(100);
          //   var linkedList = LinkedListOfLongFactory.AppendNew();
          //   trigramFileIdTree.Insert(fileId, linkedList.Address);
          //   TrigramTree.Insert(trigramInfo.Key, trigramFileIdTree.Address);
          //   linkedList.AddLast(trigramInfo.Position);
          // }
          // else
          // {
          //   var trigramFileIdTree = TrigramFileTreeFactory.LoadExisting(trigramFileIdTreeAddress);
          //   if (trigramFileIdTree.TryFind(fileId, out long linkedListAddress))
          //   {
          //     var linkedList = LinkedListOfLongFactory.LoadExisting(linkedListAddress);
          //     linkedList.AddLast(trigramInfo.Position);
          //   }
          //   else
          //   {
          //     var linkedList = LinkedListOfLongFactory.AppendNew();
          //     trigramFileIdTree.Insert(fileId, linkedList.Address);
          //     linkedList.AddLast(trigramInfo.Position);
          //   }
          // }

          count++;
        }

        Console.WriteLine();
      }
    }

    Pause();
  }
}

