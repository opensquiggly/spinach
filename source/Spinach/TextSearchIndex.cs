using Eugene;
using Eugene.Blocks;
using Spinach.Blocks;

namespace Spinach;

using Caching;
using Eugene.Collections;
using Helpers;
using SpinachExplorer.Enumerators;
using System.ComponentModel.Design.Serialization;
using Trigrams;

public class TextSearchIndex
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TextSearchIndex()
  {
    DiskBlockManager = new DiskBlockManager();
    DiskBlockManager.RegisterBlockType<RepoInfoBlock>();
    RepoExternalIdTable = new RepoExternalIdTable(DiskBlockManager);
    RepoInternalIdTable = new RepoInternalIdTable(DiskBlockManager);
    FileExternalIdTable = new FileExternalIdTable(DiskBlockManager);
    FileInternalIdTable = new FileInternalIdTable(DiskBlockManager);

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
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private string Path { get; }

  public DiskBlockManager DiskBlockManager { get; }

  private RepoInternalIdTable RepoInternalIdTable { get; }

  private RepoExternalIdTable RepoExternalIdTable { get; }

  private FileInternalIdTable FileInternalIdTable { get; }

  private FileExternalIdTable FileExternalIdTable { get; }

  public string FileName { get; set; }

  public bool IsOpen { get; set; } = false;

  public DiskBTree<long, long> InternalFileIdTree { get; private set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  private short TrigramKeyType { get; set; }

  private DiskBTreeFactory<long, long> FileIdTreeFactory { get; set; }

  private DiskBTreeFactory<int, long> TrigramTreeFactory { get; set; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; set; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private long GetNextInternalRepoId() => 0;

  public DiskBTree<long, long> LoadTrigramFileIdTree(int trigramKey)
  {
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

    return null;
  }

  private DiskBTree<long, long> LoadOrAddTrigramFileIdTree(int trigramKey, long fileId, out bool created)
  {
    created = false;

    DiskBTree<long, long> result = LoadTrigramFileIdTree(trigramKey);
    if (result != null)
    {
      return result;
    }

    DiskBTree<long, long> newTrigramFileIdTree = TrigramFileTreeFactory.AppendNew(25);
    DiskLinkedList<long> linkedList = LinkedListOfLongFactory.AppendNew();
    newTrigramFileIdTree.Insert(fileId, linkedList.Address);
    TrigramTree.Insert(trigramKey, newTrigramFileIdTree.Address);
    TrigramFileIdTreeCache.Add(trigramKey, newTrigramFileIdTree);
    created = true;

    return newTrigramFileIdTree;
  }

  private DiskLinkedList<long> LoadPostingsList(int trigramKey, long fileId)
  {
    var key = new Tuple<int, long>(trigramKey, fileId);

    if (PostingsListCache.TryGetValue(key, out DiskLinkedList<long> postingsList))
    {
      return postingsList;
    }

    DiskBTree<long, long> trigramFileIdTree = LoadTrigramFileIdTree(trigramKey);
    if (trigramFileIdTree == null)
    {
      return null;
    }

    if (trigramFileIdTree.TryFind(fileId, out long postingsListAddress))
    {
      DiskLinkedList<long> loadedPostingsList = LinkedListOfLongFactory.LoadExisting(postingsListAddress);
      PostingsListCache.Add(key, loadedPostingsList);

      return loadedPostingsList;
    }

    return null;
  }

  private DiskLinkedList<long> LoadOrAddPostingsList(int trigramKey, long fileId)
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

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Open() => DiskBlockManager.CreateOrOpen(Path);

  public void Close() => IsOpen = false;

  public TrigramFileEnumerator GetTrigramFileEnumerator(int key)
  {
    return new TrigramFileEnumerator(
      TrigramFileIdTreeCache,
      TrigramTree,
      TrigramFileTreeFactory,
      InternalFileIdTree,
      PostingsListCache,
      LinkedListOfLongFactory,
      key
    );
  }

  public long AddRepository(string externalRepoId, string repoName)
  {
    long internalId = RepoInternalIdTable.FindRepo(externalRepoId);
    if (internalId == 0)
    {
      internalId = GetNextInternalRepoId();
      RepoInternalIdTable.AddRepo(internalId, externalRepoId, repoName);
      RepoExternalIdTable.AddRepo(internalId, externalRepoId, repoName);
    }

    return internalId;
  }

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

    InternalFileIdTree = FileIdTreeFactory.AppendNew(25);
    TrigramTree = TrigramTreeFactory.AppendNew(25);

    HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();
    headerBlock.Address1 = InternalFileIdTree.Address;
    headerBlock.Address2 = TrigramTree.Address;

    DiskBlockManager.WriteHeaderBlock(ref headerBlock);

    Console.WriteLine($"InternalFileIdTree Stored at Address: {headerBlock.Address1}");
    Console.WriteLine($"TrigramTree Stored at Address: {headerBlock.Address2}");

    IsOpen = true;
    FileName = filename;
  }

  public void OpenExistingIndexFile(string filename)
  {
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);

    HeaderBlock headerBlock = DiskBlockManager.GetHeaderBlock();

    InternalFileIdTree = FileIdTreeFactory.LoadExisting(headerBlock.Address1);
    TrigramTree = TrigramTreeFactory.LoadExisting(headerBlock.Address2);

    Console.WriteLine($"InternalFileIdTree Loaded from Address: {headerBlock.Address1}");
    Console.WriteLine($"TrigramTree Loaded from Address: {headerBlock.Address2}");

    IsOpen = true;
    FileName = filename;
  }

  public void IndexFiles(long firstFileId, long lastFileId)
  {
    for (long fileId = firstFileId; fileId <= lastFileId; fileId++)
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

      if (FileHelper.FileIsBinary(name))
      {
        continue;
      }

      Console.Write($"Indexing {fileId} : {name} ...");

      string content = File.ReadAllText(name);

      var trigramExtractor = new TrigramExtractor(content);
      int count = 0;

      foreach (TrigramInfo trigramInfo in trigramExtractor)
      {
        // char ch1 = (char)(trigramInfo.Key / 128L / 128L);
        // char ch2 = (char)(trigramInfo.Key % (128L * 128L) / 128L);
        // char ch3 = (char)(trigramInfo.Key % 128L);

        // Console.WriteLine($"Position: {trigramInfo.Position} ... '{ch1}{ch2}{ch3}'");
        DiskLinkedList<long> postingsList = LoadOrAddPostingsList(trigramInfo.Key, fileId);
        postingsList.AddLast(trigramInfo.Position);

        count++;
      }

      Console.WriteLine($" {count} trigrams");
    }
  }

  public void IndexLocalFiles(string folderPath)
  {
    long currentFileId = 1;

    foreach (string filePath in Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories))
    {
      Console.WriteLine($"{currentFileId} : {filePath}");
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(filePath);
      InternalFileIdTree.Insert(currentFileId, nameString.Address);
      currentFileId++;
    }
  }

  public void PrintFileIdsInRange(long firstFileId, long lastFileId)
  {
    for (long fileId = firstFileId; fileId <= lastFileId; fileId++)
    {
      long nameAddress = InternalFileIdTree.Find(fileId);
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
      Console.WriteLine($"{fileId}: {nameString.GetValue()}");
    }
  }
}
