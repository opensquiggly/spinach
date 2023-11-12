namespace Spinach;

public class TextSearchIndex
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TextSearchIndex()
  {
    DiskBlockManager = new DiskBlockManager();
    RepoInfoBlockType = DiskBlockManager.RegisterBlockType<RepoInfoBlock>();
    FileInfoBlockType = DiskBlockManager.RegisterBlockType<FileInfoBlock>();
    TrigramKeyType = DiskBlockManager.RegisterBlockType<TrigramKey>();

    FileExternalIdTable = new FileExternalIdTable(DiskBlockManager);
    FileInternalIdTable = new FileInternalIdTable(DiskBlockManager);

    _headerBlock = DiskBlockManager.GetHeaderBlock();

    // The FileIdTree is used to look up file names based on their internal id
    // Given a key of internal file id, it returns an address to a DiskImmutableString
    FileIdTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<long, long>(
        DiskBlockManager.LongBlockType,
        DiskBlockManager.LongBlockType
      );

    FileInfoTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<long, FileInfoBlock>(
        DiskBlockManager.LongBlockType,
        FileInfoBlockType
      );

    RepoIdTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<long, RepoInfoBlock>(
        DiskBlockManager.LongBlockType,
        RepoInfoBlockType
      );

    RepoInternalIdTable = new RepoInternalIdTable(DiskBlockManager, RepoIdTreeFactory);

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

    // TrigramFileIdTreeCache = new LruCache<int, DiskBTree<long, long>>(2200000);
    TrigramPostingsListCache = new LruCache<int, DiskSortedVarIntList>(2200000);
    PostingsListCache = new LruCache<Tuple<int, long>, DiskLinkedList<long>>(2200000);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private short RepoInfoBlockType { get; set; }

  private short FileInfoBlockType { get; set; }

  private string Path { get; }

  public DiskBlockManager DiskBlockManager { get; }

  private HeaderBlock _headerBlock;

  private RepoInternalIdTable RepoInternalIdTable { get; set; }

  private RepoExternalIdTable RepoExternalIdTable { get; }

  private FileInternalIdTable FileInternalIdTable { get; }

  private FileExternalIdTable FileExternalIdTable { get; }

  public string FileName { get; set; }

  public bool IsOpen { get; set; } = false;

  public DiskBTree<long, long> InternalFileIdTree { get; private set; }

  public DiskBTree<long, FileInfoBlock> InternalFileInfoTree { get; private set; }

  public InternalFileInfoTable InternalFileInfoTable { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  private short TrigramKeyType { get; set; }

  private DiskBTreeFactory<long, long> FileIdTreeFactory { get; set; }

  private DiskBTreeFactory<long, FileInfoBlock> FileInfoTreeFactory { get; set; }

  private DiskBTreeFactory<long, RepoInfoBlock> RepoIdTreeFactory { get; set; }

  private DiskBTreeFactory<int, long> TrigramTreeFactory { get; set; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  // private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; set; }

  private LruCache<int, DiskSortedVarIntList> TrigramPostingsListCache { get; set; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private long GetNextInternalRepoId()
  {
    _headerBlock.Data1++;
    DiskBlockManager.WriteHeaderBlock(ref _headerBlock, true);
    return _headerBlock.Data1;
  }

  public DiskSortedVarIntList LoadTrigramPostingsList(int trigramKey)
  {
    if (TrigramPostingsListCache.TryGetValue(trigramKey, out DiskSortedVarIntList postingsList))
    {
      return postingsList;
    }

    if (TrigramTree.TryFind(trigramKey, out long postingsListAddress))
    {
      DiskSortedVarIntList existingPostingsList =
        DiskBlockManager.SortedVarIntListFactory.LoadExisting(postingsListAddress);

      TrigramPostingsListCache.Add(trigramKey, existingPostingsList);
      return existingPostingsList;
    }

    return null;
  }

  public DiskSortedVarIntList LoadOrAddTrigramPostingsList(int trigramKey, out bool created)
  {
    created = false;

    DiskSortedVarIntList result = LoadTrigramPostingsList(trigramKey);
    if (result != null)
    {
      return result;
    }

    DiskSortedVarIntList postingsList = DiskBlockManager.SortedVarIntListFactory.AppendNew();
    TrigramTree.Insert(trigramKey, postingsList.Address);
    TrigramPostingsListCache.Add(trigramKey, postingsList);
    created = true;

    return postingsList;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Open()
  {
    DiskBlockManager.CreateOrOpen(Path);
    _headerBlock = DiskBlockManager.GetHeaderBlock();
  }

  public void Close()
  {
    if (DiskBlockManager != null && IsOpen)
    {
      DiskBlockManager.Flush();
      DiskBlockManager.Close();
    }

    IsOpen = false;
  }

  public FastTrigramEnumerable GetFastTrigramEnumerable(int trigramKey)
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramEnumerable(
      TrigramTree,
      TrigramPostingsListCache,
      DiskBlockManager.SortedVarIntListFactory,
      trigramKey
    );
  }

  public FastTrigramFileEnumerable GetFastTrigramFileEnumerable(int key)
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramFileEnumerable(
      InternalFileInfoTable,
      TrigramTree,
      TrigramPostingsListCache,
      DiskBlockManager.SortedVarIntListFactory,
      key
    );
  }

  public FastLiteralEnumerable GetFastLiteralEnumerable(string literal) =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    new FastLiteralEnumerable(this, literal);

  public FastLiteralFileEnumerable GetFastLiteralFileEnumerable(string literal)
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastLiteralFileEnumerable(
      this,
      InternalFileInfoTable,
      literal
    );
  }

  public IEnumerable<RegexEnumerable.MatchingFile> RegexEnumerable(string regex) => new RegexEnumerable(this, regex);

  public long AddRepository(string externalRepoId, string repoName, string rootFolder)
  {
    long internalId = (externalRepoId != null) ? RepoInternalIdTable.FindRepo(externalRepoId) : 0;
    if (internalId == 0)
    {
      internalId = GetNextInternalRepoId();
      RepoInternalIdTable.AddRepo(internalId, externalRepoId, repoName, rootFolder);
      if (externalRepoId != null)
      {
        RepoExternalIdTable.AddRepo(internalId, externalRepoId, repoName);
      }
    }

    return internalId;
  }

  public IEnumerable<RepoInfoBlock> GetRepositories() => RepoInternalIdTable.GetRepositories();

  public DiskBTreeCursor<long, RepoInfoBlock> GetRepositoriesCursor() => RepoInternalIdTable.GetRepositoriesCursor();

  public RepoInfoBlock FindRepository(long internalId) => RepoInternalIdTable.FindRepository(internalId);

  public void GetRepository(long internalRepoId)
  {
    // Look up a repository using its internally assigned numeric id
  }

  public long AddFile(long internalRepoId, string filename) =>
    // 1) Generate a new sequentially increasing integer id for the repository
    //    -> Need a place to store the last repository id
    // 2) Add repository record to internal id dictionary
    //    -> Best stored as a BPlusTreeOfLong data structure to handle large numbers of repos
    // 3) Add repository record to external id string
    //    -> This is a unique id that is supplied by the client

    // Add the filename to the filename trigram index

    0;

  public void IndexFile(long internalRepoId, long internalFileId, string content)
  {
    // Get each trigram
    // Add trigram to the triegram trie tree if it doesn't already exist
  }

  public void GetAllTrigrams(string trigram)
  {
    // Return an enumerator that enumerats all trigrams
    // returning the tuple <internal-repo-id, internal-file-id, position>
    // for each trigram found in the index
  }

  public void CreateNewIndexFile(string filename)
  {
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);
    _headerBlock = DiskBlockManager.GetHeaderBlock();

    RepoInternalIdTable = new RepoInternalIdTable(DiskBlockManager, RepoIdTreeFactory);
    DiskBTree<long, RepoInfoBlock> tree = RepoInternalIdTable.AppendNew();
    InternalFileIdTree = FileIdTreeFactory.AppendNew(25);
    InternalFileInfoTree = FileInfoTreeFactory.AppendNew(25);
    TrigramTree = TrigramTreeFactory.AppendNew(25);

    _headerBlock.Address1 = InternalFileIdTree.Address;
    _headerBlock.Address2 = TrigramTree.Address;
    _headerBlock.Address3 = tree.Address;
    _headerBlock.Address4 = InternalFileInfoTree.Address;

    DiskBlockManager.WriteHeaderBlock(ref _headerBlock, true);
    DiskBlockManager.Flush();

    Console.WriteLine($"InternalFileIdTree Stored at Address: {_headerBlock.Address1}");
    Console.WriteLine($"TrigramTree Stored at Address: {_headerBlock.Address2}");

    IsOpen = true;
    FileName = filename;
  }

  public void OpenExistingIndexFile(string filename)
  {
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);
    _headerBlock = DiskBlockManager.GetHeaderBlock();

    InternalFileIdTree = FileIdTreeFactory.LoadExisting(_headerBlock.Address1);
    InternalFileInfoTree = FileInfoTreeFactory.LoadExisting(_headerBlock.Address4);
    TrigramTree = TrigramTreeFactory.LoadExisting(_headerBlock.Address2);
    RepoInternalIdTable.Load(_headerBlock.Address3);

    Console.WriteLine($"InternalFileIdTree Loaded from Address: {_headerBlock.Address1}");
    Console.WriteLine($"TrigramTree Loaded from Address: {_headerBlock.Address2}");

    InternalFileInfoTable = new InternalFileInfoTable(DiskBlockManager, InternalFileInfoTree);
    InternalFileInfoTable.EnsureBuilt();

    Console.WriteLine($"Currently Indexed Files: {InternalFileInfoTable.FileCount}");
    ulong startingIndex = 0;
    InternalFileInfoTable.InternalFileInfo resultFileInfo = null;

    (startingIndex, resultFileInfo) = InternalFileInfoTable.FindFirstWithOffsetGreaterThanOrEqual(startingIndex, 10);
    Console.WriteLine($"File corresponding to offset 10 is {resultFileInfo.Name}");

    (_, resultFileInfo) = InternalFileInfoTable.FindFirstWithOffsetGreaterThanOrEqual(startingIndex, 1000);
    Console.WriteLine($"File corresponding to offset 1000 is {resultFileInfo.Name}");

    IsOpen = true;
    FileName = filename;
  }

  public void Flush() => DiskBlockManager.Flush();

  public string LoadImmutableString(long address)
  {
    if (address == 0)
    {
      return string.Empty;
    }

    return DiskBlockManager.ImmutableStringFactory.LoadExisting(address).GetValue();
  }

  public bool IncludeFileInIndex(string filename)
  {
    if (filename.Contains("/.git/"))
    {
      return false;
    }

    if (filename.Contains("/obj/"))
    {
      return false;
    }

    if (filename.Contains("/bin/"))
    {
      return false;
    }

    if (filename.Contains("node_modules"))
    {
      return false;
    }

    if (filename.EndsWith(".jpg") || filename.EndsWith(".gif") || filename.EndsWith(".png") || filename.EndsWith(".dll"))
    {
      return false;
    }

    if (FileHelper.FileIsBinary(filename))
    {
      return false;
    }

    return true;
  }

  public void IndexFiles(long firstFileId, long lastFileId)
  {
    ulong totalOffset = 0;

    for (long fileId = firstFileId; fileId <= lastFileId; fileId++)
    {
      // long nameAddress = InternalFileIdTree.Find(fileId);
      FileInfoBlock fileInfoBlock = InternalFileInfoTree.Find(fileId);
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(fileInfoBlock.NameAddress);
      string name = nameString.GetValue();

      Console.Write($"Indexing {fileId} : {name} ...");

      string content = File.ReadAllText(name);

      var trigramExtractor = new TrigramExtractor(content);
      int count = 0;

      foreach (TrigramInfo trigramInfo in trigramExtractor)
      {
        DiskSortedVarIntList postingsList = LoadOrAddTrigramPostingsList(trigramInfo.Key, out bool _);

        // TODO: As-is, this is very inefficient
        postingsList.AppendData(new ulong[] { totalOffset + (ulong)trigramInfo.Position });

        count++;
      }

      Console.WriteLine($" {count} trigrams");
      totalOffset += (ulong)fileInfoBlock.Length;
    }
  }

  private int GetFileLength(string path)
  {
    int characterCount = 0;
    using var reader = new StreamReader(path);
    while (reader.Read() != -1)
    {
      characterCount++;
    }

    return characterCount;
  }

  public void IndexLocalFiles(string folderPath)
  {
    long currentFileId = 1;

    foreach (string filePath in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
    {
      if (!IncludeFileInIndex(filePath))
      {
        continue;
      }

      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(filePath);
      InternalFileIdTree.Insert(currentFileId, nameString.Address);
      FileInfoBlock fileInfoBlock = default;
      fileInfoBlock.InternalId = (ulong)currentFileId;
      fileInfoBlock.NameAddress = nameString.Address;
      fileInfoBlock.Length = GetFileLength(filePath);
      Console.WriteLine($"{currentFileId} : {filePath} (Length = {fileInfoBlock.Length})");
      InternalFileInfoTree.Insert(currentFileId, fileInfoBlock);
      currentFileId++;
    }
  }

  public void PrintFileIdsInRange(long firstFileId, long lastFileId)
  {
    for (long fileId = firstFileId; fileId <= lastFileId; fileId++)
    {
      // long nameAddress = InternalFileIdTree.Find(fileId);
      FileInfoBlock fileInfoBlock = InternalFileInfoTree.Find(fileId);
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(fileInfoBlock.NameAddress);
      Console.WriteLine($"{fileId}: {nameString.GetValue()} (Length = {fileInfoBlock.Length})");
    }
  }
}
