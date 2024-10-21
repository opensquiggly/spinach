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
    FileInfoKeyType = DiskBlockManager.RegisterBlockType<FileInfoKey>();
    TrigramKeyType = DiskBlockManager.RegisterBlockType<TrigramKey>();
    TrigramMatchKeyType = DiskBlockManager.RegisterBlockType<TrigramMatchKey>();

    FileExternalIdTable = new FileExternalIdTable(DiskBlockManager);
    FileInternalIdTable = new FileInternalIdTable(DiskBlockManager);

    _headerBlock = DiskBlockManager.GetHeaderBlock();

    // The FileIdTree is used to look up file names based on their internal id
    // Given a key of internal file id, it returns an address to a DiskImmutableString
    FileIdTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<FileInfoKey, long>(
        FileInfoKeyType,
        DiskBlockManager.LongBlockType
      );

    FileInfoTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<FileInfoKey, FileInfoBlock>(
        FileInfoKeyType,
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

    TrigramMatchesFactory =
      DiskBlockManager.BTreeManager.CreateFactory<TrigramMatchKey, long>(
        TrigramMatchKeyType,
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
    // TrigramPostingsListCache = new LruCache<int, DiskSortedVarIntList>(2200000);
    TrigramMatchesCache = new LruCache<int, DiskBTree<TrigramMatchKey, long>>(2200000);
    PostingsListCache = new LruCache<TrigramMatchCacheKey, DiskSortedVarIntList>(2200000);
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

  public DiskBTree<FileInfoKey, long> InternalFileIdTree { get; private set; }

  public DiskBTree<FileInfoKey, FileInfoBlock> InternalFileInfoTree { get; private set; }

  public InternalFileInfoTable InternalFileInfoTable { get; set; }

  public DiskBTree<int, long> TrigramTree { get; set; }

  private DiskBTree<TrigramMatchKey, long> TrigramMatches { get; set; }

  private short TrigramKeyType { get; set; }

  private short TrigramMatchKeyType { get; set; }

  private short FileInfoKeyType { get; set; }

  private DiskBTreeFactory<FileInfoKey, long> FileIdTreeFactory { get; set; }

  private DiskBTreeFactory<FileInfoKey, FileInfoBlock> FileInfoTreeFactory { get; set; }

  private DiskBTreeFactory<long, RepoInfoBlock> RepoIdTreeFactory { get; set; }

  private DiskBTreeFactory<int, long> TrigramTreeFactory { get; set; }

  public DiskBTreeFactory<TrigramMatchKey, long> TrigramMatchesFactory { get; set; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  // private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; set; }

  // private LruCache<int, DiskSortedVarIntList> TrigramPostingsListCache { get; set; }

  private LruCache<int, DiskBTree<TrigramMatchKey, long>> TrigramMatchesCache { get; set; }

  private LruCache<TrigramMatchCacheKey, DiskSortedVarIntList> PostingsListCache { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private long GetNextInternalRepoId()
  {
    _headerBlock.Data1++;
    DiskBlockManager.WriteHeaderBlock(ref _headerBlock, true);
    return _headerBlock.Data1;
  }

  public DiskBTree<TrigramMatchKey, long> LoadTrigramMatches(int trigramKey)
  {
    if (TrigramMatchesCache.TryGetValue(trigramKey, out DiskBTree<TrigramMatchKey, long> trigramMatches))
    {
      return trigramMatches;
    }

    if (TrigramTree.TryFind(trigramKey, out long trigramMatchesAddress))
    {
      var existingTrigramMatches =
        TrigramMatchesFactory.LoadExisting(trigramMatchesAddress);

      TrigramMatchesCache.Add(trigramKey, existingTrigramMatches);
      return existingTrigramMatches;
    }

    return null;
  }

  public DiskBTree<TrigramMatchKey, long> LoadOrAddTrigramMatches(int trigramKey, out bool created)
  {
    created = false;

    DiskBTree<TrigramMatchKey, long> result = LoadTrigramMatches(trigramKey);
    if (result != null)
    {
      return result;
    }

    var trigramMatches = TrigramMatchesFactory.AppendNew(25);
    TrigramTree.Insert(trigramKey, trigramMatches.Address);
    TrigramMatchesCache.Add(trigramKey, trigramMatches);
    created = true;

    return trigramMatches;
  }

  public DiskSortedVarIntList LoadOrAddTrigramPostingsList(TrigramMatchCacheKey key)
  {
    if (PostingsListCache.TryGetValue(key, out DiskSortedVarIntList postingsList))
    {
      return postingsList;
    }

    var trigramMatchKey = new TrigramMatchKey(key.UserType, key.UserId, key.RepoId);

    if (TrigramTree.TryFind(key.TrigramKey, out long trigramMatchesAddress))
    {
      DiskBTree<TrigramMatchKey, long> trigramMatches = TrigramMatchesFactory.LoadExisting(trigramMatchesAddress);
      if (trigramMatches == null)
      {
        throw new Exception(
          "Could not find an existing postings list stored in the TrigramMatches key. The index file appears to be corrupted.");
      }

      if (trigramMatches.TryFind(trigramMatchKey, out long postingsListAddress))
      {
        DiskSortedVarIntList existingPostingsList =
          DiskBlockManager.SortedVarIntListFactory.LoadExisting(postingsListAddress);

        PostingsListCache.Add(key, existingPostingsList);
        return existingPostingsList;
      }

      DiskSortedVarIntList newPostingsList = DiskBlockManager.SortedVarIntListFactory.AppendNew();
      trigramMatches.Insert(trigramMatchKey, newPostingsList.Address);
      PostingsListCache.Add(key, newPostingsList);

      return newPostingsList;
    }

    DiskBTree<TrigramMatchKey, long> newTrigramMatches = TrigramMatchesFactory.AppendNew(25);
    postingsList = DiskBlockManager.SortedVarIntListFactory.AppendNew();
    newTrigramMatches.Insert(trigramMatchKey, postingsList.Address);
    TrigramTree.Insert(key.TrigramKey, newTrigramMatches.Address);
    PostingsListCache.Add(key, postingsList);

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
      PostingsListCache,
      DiskBlockManager.SortedVarIntListFactory,
      TrigramMatchesFactory,
      trigramKey
    );
  }

  public FastTrigramFileEnumerable GetFastTrigramFileEnumerable(int key)
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    // return new FastTrigramFileEnumerable(
    //   InternalFileInfoTable,
    //   TrigramTree,
    //   TrigramPostingsListCache,
    //   DiskBlockManager.SortedVarIntListFactory,
    //   key
    // );
    throw new NotImplementedException();
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

    InternalFileInfoTable = new InternalFileInfoTable(DiskBlockManager, InternalFileInfoTree);
    InternalFileInfoTable.EnsureBuilt();

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

    var cursor = new DiskBTreeCursor<FileInfoKey, long>(InternalFileIdTree);
    while (cursor.MoveNext())
    {
      Console.WriteLine($"UserType = {cursor.CurrentKey.UserType}, UserId = {cursor.CurrentKey.UserId}, RepoId = {cursor.CurrentKey.RepoId}, FileId = {cursor.CurrentKey.FileId}");
    }

    // InternalFileInfoTable = new InternalFileInfoTable(DiskBlockManager, InternalFileInfoTree);
    // InternalFileInfoTable.EnsureBuilt();

    // Console.WriteLine($"Currently Indexed Files: {InternalFileInfoTable.FileCount}");
    // ulong startingIndex = 0;
    // InternalFileInfoTable.InternalFileInfo resultFileInfo = null;

    // (startingIndex, resultFileInfo) = InternalFileInfoTable.FindFirstWithOffsetGreaterThanOrEqual(startingIndex, 10);
    // Console.WriteLine($"File corresponding to offset 10 is {resultFileInfo.Name}");
    //
    // (_, resultFileInfo) = InternalFileInfoTable.FindFirstWithOffsetGreaterThanOrEqual(startingIndex, 1000);
    // Console.WriteLine($"File corresponding to offset 1000 is {resultFileInfo.Name}");

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

  public void IndexFiles()
  {
    ulong totalOffset = 0;

    var cursor = new DiskBTreeCursor<FileInfoKey, FileInfoBlock>(InternalFileInfoTree);
    cursor.Reset();

    while (cursor.MoveNext())
    {
      FileInfoBlock fileInfoBlock = cursor.CurrentData;
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(fileInfoBlock.NameAddress);
      string name = nameString.GetValue();
      Console.Write($"Indexing {fileInfoBlock.InternalId} : {name} ...");

      string content = File.ReadAllText(name);

      var trigramExtractor = new TrigramExtractor(content);
      int count = 0;

      foreach (TrigramInfo trigramInfo in trigramExtractor)
      {
        var trigramMatchCacheKey = new TrigramMatchCacheKey
        {
          TrigramKey = trigramInfo.Key,
          UserType = cursor.CurrentKey.UserType,
          UserId = cursor.CurrentKey.UserId,
          RepoId = cursor.CurrentKey.RepoId
        };

        DiskSortedVarIntList postingsList = LoadOrAddTrigramPostingsList(trigramMatchCacheKey);

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

  public void IndexLocalFiles(ushort userType, uint userId, uint repoId, string folderPath)
  {
    ulong currentFileId = 1;
    ulong currentOffset = 0;

    foreach (string filePath in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
    {
      if (!IncludeFileInIndex(filePath))
      {
        continue;
      }

      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(filePath);
      var fileInfoKey = new FileInfoKey(userType, userId, repoId, currentFileId);
      InternalFileIdTree.Insert(fileInfoKey, nameString.Address);
      FileInfoBlock fileInfoBlock = default;
      fileInfoBlock.InternalId = (ulong)currentFileId;
      fileInfoBlock.NameAddress = nameString.Address;
      fileInfoBlock.Length = GetFileLength(filePath);
      fileInfoBlock.StartingOffset = currentOffset;
      Console.WriteLine($"{currentFileId} : {filePath} (Length = {fileInfoBlock.Length})");
      InternalFileInfoTree.Insert(fileInfoKey, fileInfoBlock);
      currentFileId++;
      currentOffset += (ulong) fileInfoBlock.Length;
    }

    // InternalFileInfoTable = new InternalFileInfoTable(DiskBlockManager, InternalFileInfoTree);
    // InternalFileInfoTable.EnsureBuilt();
  }

  public void PrintFileIdsInRange(long firstFileId, long lastFileId)
  {
    // for (long fileId = firstFileId; fileId <= lastFileId; fileId++)
    // {
    //   // long nameAddress = InternalFileIdTree.Find(fileId);
    //   FileInfoBlock fileInfoBlock = InternalFileInfoTree.Find(fileId);
    //   DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(fileInfoBlock.NameAddress);
    //   Console.WriteLine($"{fileId}: {nameString.GetValue()} (Length = {fileInfoBlock.Length})");
    // }
  }
}
