namespace Spinach.Manager;

using Misc;
using TrackingObjects;

public class TextSearchManager : ITextSearchManager, ITextSearchEnumeratorContext
{
  private HeaderBlock _headerBlock;

  public TextSearchManager()
  {
    DiskBlockManager = new DiskBlockManager();

    TrigramKeyType = DiskBlockManager.RegisterBlockType<TrigramKey>();
    TrigramMatchKeyType = DiskBlockManager.RegisterBlockType<TrigramMatchKey>();
    UserIdCompoundKeyBlockType = DiskBlockManager.RegisterBlockType<UserIdCompoundKeyBlock>();
    UserInfoBlockType = DiskBlockManager.RegisterBlockType<UserInfoBlock>();
    RepoIdCompoundKeyBlockType = DiskBlockManager.RegisterBlockType<RepoIdCompoundKeyBlock>();
    RepoInfoBlockType = DiskBlockManager.RegisterBlockType<RepoInfoBlock>();
    DocInfoBlockType = DiskBlockManager.RegisterBlockType<DocInfoBlock>();
    DocIdCompoundKeyBlockType = DiskBlockManager.RegisterBlockType<DocIdCompoundKeyBlock>();
    DocOffsetCompoundKeyBlockType = DiskBlockManager.RegisterBlockType<DocOffsetCompoundKeyBlock>();

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

    UserTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<UserIdCompoundKeyBlock, UserInfoBlock>(
        UserIdCompoundKeyBlockType,
        UserInfoBlockType
      );

    RepoTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<RepoIdCompoundKeyBlock, RepoInfoBlock>(
        RepoIdCompoundKeyBlockType,
        RepoInfoBlockType
      );

    DocTreeFactory =
      DiskBlockManager.BTreeManager.CreateFactory<DocIdCompoundKeyBlock, DocInfoBlock>(
        DocIdCompoundKeyBlockType,
        DocInfoBlockType
      );

    DocTreeByOffsetFactory =
      DiskBlockManager.BTreeManager.CreateFactory<DocOffsetCompoundKeyBlock, uint>(
        DocOffsetCompoundKeyBlockType,
        DiskBlockManager.IntBlockType
      );
  }

  public ITextSearchOptions Options { get; } = TextSearchOptions.Default;

  protected short DocInfoBlockType { get; set; }

  protected short DocIdCompoundKeyBlockType { get; set; }

  protected short DocOffsetCompoundKeyBlockType { get; set; }

  protected short TrigramKeyType { get; set; }

  protected short TrigramMatchKeyType { get; set; }

  protected short UserIdCompoundKeyBlockType { get; set; }

  protected short UserInfoBlockType { get; set; }

  protected short RepoIdCompoundKeyBlockType { get; set; }

  protected short RepoInfoBlockType { get; set; }

  public DiskBlockManager DiskBlockManager { get; }

  public string FileName { get; set; }

  public bool IsOpen { get; set; } = false;

  public DiskBTree<int, long> TrigramTree { get; set; }

  public UserCache UserCache { get; set; }

  public DiskBTree<UserIdCompoundKeyBlock, UserInfoBlock> UserTree { get; private set; }

  public RepoCache RepoCache { get; set; }

  public DiskBTree<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTree { get; private set; }

  public DocCache DocCache { get; set; }

  public DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> DocTree { get; private set; }

  public DiskBTree<DocOffsetCompoundKeyBlock, uint> DocTreeByOffset { get; private set; }

  public DiskBTreeFactory<int, long> TrigramTreeFactory { get; set; }

  public DiskBTreeFactory<TrigramMatchKey, long> TrigramMatchesFactory { get; set; }

  public DiskBTreeFactory<UserIdCompoundKeyBlock, UserInfoBlock> UserTreeFactory { get; private set; }

  public DiskBTreeFactory<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTreeFactory { get; private set; }
  public DiskBTreeFactory<DocIdCompoundKeyBlock, DocInfoBlock> DocTreeFactory { get; private set; }

  public DiskBTreeFactory<DocOffsetCompoundKeyBlock, uint> DocTreeByOffsetFactory { get; private set; }

  public LruCache<TrigramMatchCacheKey, DiskSortedVarIntList> PostingsListCache { get; set; }

  public void CreateNewIndexFile(string filename)
  {
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);
    _headerBlock = DiskBlockManager.GetHeaderBlock();

    UserTree = UserTreeFactory.AppendNew(25);
    UserCache = new UserCache(this, UserTree, 10000);
    RepoTree = RepoTreeFactory.AppendNew(25);
    RepoCache = new RepoCache(this, RepoTree, 10000);
    DocTree = DocTreeFactory.AppendNew(25);
    DocCache = new DocCache(this, RepoCache, DocTree, 10000);
    DocTreeByOffset = DocTreeByOffsetFactory.AppendNew(25);
    TrigramTree = TrigramTreeFactory.AppendNew(25);

    PostingsListCache = new LruCache<TrigramMatchCacheKey, DiskSortedVarIntList>(2200000);

    _headerBlock.Address1 = UserTree.Address;
    _headerBlock.Address2 = RepoTree.Address;
    _headerBlock.Address3 = DocTree.Address;
    _headerBlock.Address4 = DocTreeByOffset.Address;
    _headerBlock.Address5 = TrigramTree.Address;

    DiskBlockManager.WriteHeaderBlock(ref _headerBlock, true);
    DiskBlockManager.Flush();

    IsOpen = true;
    FileName = filename;
  }

  public void OpenExistingIndexFile(string filename)
  {
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);
    _headerBlock = DiskBlockManager.GetHeaderBlock();

    UserTree = UserTreeFactory.LoadExisting(_headerBlock.Address1);
    UserCache = new UserCache(this, UserTree, 10000);
    RepoTree = RepoTreeFactory.LoadExisting(_headerBlock.Address2);
    RepoCache = new RepoCache(this, RepoTree, 10000);
    DocTree = DocTreeFactory.LoadExisting(_headerBlock.Address3);
    DocCache = new DocCache(this, RepoCache, DocTree, 10000);
    DocTreeByOffset = DocTreeByOffsetFactory.LoadExisting(_headerBlock.Address4);
    TrigramTree = TrigramTreeFactory.LoadExisting(_headerBlock.Address5);

    PostingsListCache = new LruCache<TrigramMatchCacheKey, DiskSortedVarIntList>(2200000);

    IsOpen = true;
    FileName = filename;
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

  public void Flush() =>
    // ReSharper disable once ArrangeMethodOrOperatorBody
    DiskBlockManager.Flush();

  public string LoadImmutableString(long address)
  {
    if (address == 0)
    {
      return string.Empty;
    }

    return DiskBlockManager.ImmutableStringFactory.LoadExisting(address).GetValue();
  }

  public uint AddUser(ushort userType, string userName, string userExternalId)
  {
    uint nextUserId = (uint)++_headerBlock.Data1;
    DiskBlockManager.WriteHeaderBlock(ref _headerBlock);
    DiskBlockManager.Flush();

    var userIdCompoundKeyBlock = new UserIdCompoundKeyBlock();
    userIdCompoundKeyBlock.UserType = userType;
    userIdCompoundKeyBlock.UserId = nextUserId;

    var userInfoBlock = new UserInfoBlock();
    userInfoBlock.UserType = userType;
    userInfoBlock.UserId = nextUserId;
    userInfoBlock.LastRepoId = 0;

    userInfoBlock.ExternalIdAddress = 0L;
    if (userName != null)
    {
      DiskImmutableString userNameString = DiskBlockManager.ImmutableStringFactory.Append(userName);
      userInfoBlock.NameAddress = userNameString.Address;
    }
    else
    {
      userInfoBlock.NameAddress = 0L;
    }

    if (userExternalId != null)
    {
      DiskImmutableString externalIdString = DiskBlockManager.ImmutableStringFactory.Append(userExternalId);
      userInfoBlock.ExternalIdAddress = externalIdString.Address;
    }
    else
    {
      userInfoBlock.ExternalIdAddress = 0L;
    }

    bool result = UserTree.Insert(userIdCompoundKeyBlock, userInfoBlock);

    return nextUserId;
  }

  public uint AddRepository(ushort userType, uint userId, ushort repoType, string externalId, string name, string rootFolder)
  {
    var userIdCompoundKeyBlock = new UserIdCompoundKeyBlock
    {
      UserType = userType,
      UserId = userId
    };

    bool found = UserCache.TryFind(userIdCompoundKeyBlock, out UserInfoBlock userInfoBlock, out DiskBTreeNode<UserIdCompoundKeyBlock, UserInfoBlock> node, out int nodeIndex, out _);
    if (found)
    {
      var repoIdCompoundKeyBlock = new RepoIdCompoundKeyBlock();
      repoIdCompoundKeyBlock.UserType = userInfoBlock.UserType;
      repoIdCompoundKeyBlock.UserId = userInfoBlock.UserId;
      repoIdCompoundKeyBlock.RepoType = repoType;

      var repoInfoBlock = new RepoInfoBlock();

      userInfoBlock.LastRepoId++;
      repoInfoBlock.InternalId = userInfoBlock.LastRepoId;
      repoInfoBlock.LastDocId = 0;
      Console.WriteLine($"Repo Id = {repoInfoBlock.InternalId}");
      repoIdCompoundKeyBlock.RepoId = repoInfoBlock.InternalId;

      node.ReplaceDataAtIndex(userInfoBlock, nodeIndex);
      UserCache.Clear();

      if (externalId != null)
      {
        DiskImmutableString externalIdString = DiskBlockManager.ImmutableStringFactory.Append(externalId);
        repoInfoBlock.ExternalIdAddress = externalIdString.Address;
      }
      else
      {
        repoInfoBlock.ExternalIdAddress = 0L;
      }

      if (name != null)
      {
        DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(name);
        repoInfoBlock.NameAddress = nameString.Address;
      }
      else
      {
        repoInfoBlock.NameAddress = 0L;
      }

      if (rootFolder != null)
      {
        DiskImmutableString rootFolderPathString = DiskBlockManager.ImmutableStringFactory.Append(rootFolder);
        repoInfoBlock.RootFolderPathAddress = rootFolderPathString.Address;
      }
      else
      {
        repoInfoBlock.RootFolderPathAddress = 0L;
      }

      RepoTree.Insert(repoIdCompoundKeyBlock, repoInfoBlock);
      return repoInfoBlock.InternalId;
    }

    return 0;
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

  private int GetFileLength(string path)
  {
    // Surely there is a faster way than this
    int characterCount = 0;
    using var reader = new StreamReader(path);
    while (reader.Read() != -1)
    {
      characterCount++;
    }

    return characterCount;
  }
  public static string GetRelativePath(string basePath, string filePath)
  {
    if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
    {
      basePath += Path.DirectorySeparatorChar;
    }

    string relativePath = filePath.Substring(basePath.Length);

    if (relativePath.StartsWith(Path.DirectorySeparatorChar.ToString()))
    {
      relativePath = relativePath.Substring(1);
    }

    return relativePath;
  }

  public bool AddSingleFileToIndex(ushort userType, uint userId, ushort repoType, uint repoId, string relativePath)
  {
    var repoIdCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId
    };

    bool found = RepoTree.TryFind(repoIdCompoundKey, out RepoInfoBlock data,
      out DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock> node, out int nodeIndex);
    if (!found) return false;

    string rootFolderPath = LoadString(data.RootFolderPathAddress);
    string fullPath = Path.Combine(rootFolderPath, relativePath);

    if (!IncludeFileInIndex(fullPath)) return false;

    long externalIdOrPathAddress = DiskBlockManager.ImmutableStringFactory.Append(relativePath).Address;

    var docIdCompoundKey = new DocIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId,
      Id = data.LastDocId + 1
    };

    var docOffsetCompoundKey = new DocOffsetCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId,
      StartingOffset = data.LastDocStartingOffset + (ulong)data.LastDocLength
    };

    long length = GetFileLength(fullPath);

    var docInfoBlock = new DocInfoBlock()
    {
      NameAddress = 0,
      ExternalIdOrPathAddress = externalIdOrPathAddress,
      Status = DocStatus.Normal,
      IsIndexed = false,
      OriginalLength = length,
      CurrentLength = length,
      StartingOffset = data.LastDocStartingOffset + (ulong)data.LastDocLength
    };

    DocTree.Insert(docIdCompoundKey, docInfoBlock);
    DocTreeByOffset.Insert(docOffsetCompoundKey, docIdCompoundKey.Id);

    data.LastDocId++;
    data.LastDocStartingOffset += (ulong)data.LastDocLength;
    data.LastDocLength = length;
    node.ReplaceDataAtIndex(data, nodeIndex);
    RepoCache.Clear();

    return true;
  }

  public void IndexLocalFiles(ushort userType, uint userId, ushort repoType, uint repoId)
  {
    uint currentFileId = 1;
    ulong currentOffset = 0;
    ulong lastOffset = 0;
    long lastLength = 0;

    var repoIdCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId
    };

    bool found = RepoTree.TryFind(repoIdCompoundKey, out RepoInfoBlock data, out DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock> node, out int nodeIndex);
    if (!found) return;

    string rootFolderPath = LoadString(data.RootFolderPathAddress);
    uint currentDocId = data.LastDocId;

    foreach (string filePath in Directory.GetFiles(rootFolderPath, "*.*", SearchOption.AllDirectories))
    {
      if (!IncludeFileInIndex(filePath))
      {
        continue;
      }

      string relativePath = GetRelativePath(rootFolderPath, filePath);
      long externalIdOrPathAddress = DiskBlockManager.ImmutableStringFactory.Append(relativePath).Address;

      var docIdCompoundKey = new DocIdCompoundKeyBlock()
      {
        UserType = userType,
        UserId = userId,
        RepoType = repoType,
        RepoId = repoId,
        Id = ++currentDocId
      };

      var docOffsetCompoundKey = new DocOffsetCompoundKeyBlock()
      {
        UserType = userType,
        UserId = userId,
        RepoType = repoType,
        RepoId = repoId,
        StartingOffset = currentOffset
      };

      long length = GetFileLength(filePath);

      var docInfoBlock = new DocInfoBlock()
      {
        NameAddress = 0,
        ExternalIdOrPathAddress = externalIdOrPathAddress,
        Status = DocStatus.Normal,
        IsIndexed = false,
        OriginalLength = length,
        CurrentLength = length,
        StartingOffset = currentOffset
      };

      DocTree.Insert(docIdCompoundKey, docInfoBlock);
      DocTreeByOffset.Insert(docOffsetCompoundKey, docIdCompoundKey.Id);

      // Console.Write($"Adding file ... Id = {docIdCompoundKey.Id}, ");
      // Console.Write($"UserType = {docIdCompoundKey.UserType}, ");
      // Console.Write($"UserId = {docIdCompoundKey.UserId}, ");
      // Console.Write($"RepoType = {docIdCompoundKey.RepoType}, ");
      // Console.Write($"RepoId = {docIdCompoundKey.RepoId}, ");
      // Console.Write($"Starting Offset = {docInfoBlock.StartingOffset}, ");
      // Console.WriteLine($"{filePath}");

      lastOffset = currentOffset;
      lastLength = length;
      currentOffset += (ulong)docInfoBlock.OriginalLength;
    }

    data.LastDocId = currentDocId;
    data.LastDocStartingOffset = lastOffset;
    data.LastDocLength = lastLength;
    node.ReplaceDataAtIndex(data, nodeIndex);
    RepoCache.Clear();
  }

  public DiskSortedVarIntList LoadOrAddTrigramPostingsList(TrigramMatchCacheKey key)
  {
    if (PostingsListCache.TryGetValue(key, out DiskSortedVarIntList postingsList))
    {
      return postingsList;
    }

    var trigramMatchKey = new TrigramMatchKey(key.UserType, key.UserId, key.RepoType, key.RepoId);

    if (TrigramTree.TryFind(key.TrigramKey, out long trigramMatchesAddress, out _, out _))
    {
      DiskBTree<TrigramMatchKey, long> trigramMatches = TrigramMatchesFactory.LoadExisting(trigramMatchesAddress);
      if (trigramMatches == null)
      {
        throw new Exception(
          "Could not find an existing postings list stored in the TrigramMatches key. The index file appears to be corrupted.");
      }

      if (trigramMatches.TryFind(trigramMatchKey, out long postingsListAddress, out _, out _))
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

  public void IndexFiles()
  {
    var cursor = new DiskBTreeCursor<DocIdCompoundKeyBlock, DocInfoBlock>(DocTree);
    cursor.Reset();

    while (cursor.MoveNext())
    {
      DocInfoBlock docInfoBlock = cursor.CurrentData;

      if (docInfoBlock.IsIndexed) continue;

      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(docInfoBlock.ExternalIdOrPathAddress);
      string name = nameString.GetValue();
      Console.Write($"Indexing: {name}");

      var repoIdCompoundKey = new RepoIdCompoundKeyBlock()
      {
        UserType = cursor.CurrentKey.UserType,
        UserId = cursor.CurrentKey.UserId,
        RepoType = cursor.CurrentKey.RepoType,
        RepoId = cursor.CurrentKey.RepoId
      };
      bool found = RepoCache.TryFind(repoIdCompoundKey, out RepoInfoBlock repoInfoBlock, out _, out _, out _);
      if (!found) continue;
      string rootFolderPath = LoadString(repoInfoBlock.RootFolderPathAddress);
      string fullPath = Path.Combine(rootFolderPath, name);
      string content = File.ReadAllText(fullPath);

      var trigramExtractor = new TrigramExtractor(content);
      int count = 0;

      foreach (TrigramInfo trigramInfo in trigramExtractor)
      {
        var trigramMatchCacheKey = new TrigramMatchCacheKey
        {
          TrigramKey = trigramInfo.Key,
          UserType = cursor.CurrentKey.UserType,
          UserId = cursor.CurrentKey.UserId,
          RepoType = cursor.CurrentKey.RepoType,
          RepoId = cursor.CurrentKey.RepoId
        };

        DiskSortedVarIntList postingsList = LoadOrAddTrigramPostingsList(trigramMatchCacheKey);

        // TODO: As-is, this is very inefficient
        postingsList.AppendData(new ulong[] { docInfoBlock.StartingOffset + (ulong)trigramInfo.Position });

        count++;
      }

      Console.WriteLine($" {count} trigrams");

      docInfoBlock.IsIndexed = true;
      cursor.CurrentNode.ReplaceDataAtIndex(docInfoBlock, cursor.CurrentIndex);
      DocCache.Clear(); // Not the best way to do this
    }
  }

  public void PrintLocalFiles(ushort userType, uint userId, ushort repoType, uint repoId)
  {
    var repoIdCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId
    };

    bool found = RepoTree.TryFind(repoIdCompoundKey, out RepoInfoBlock data, out _, out _);
    if (!found) return;

    string rootFolderPath = LoadString(data.RootFolderPathAddress);

    foreach (string filePath in Directory.GetFiles(rootFolderPath, "*.*", SearchOption.AllDirectories))
    {
      if (!IncludeFileInIndex(filePath))
      {
        Console.WriteLine($"EXCLUDE: {filePath}");
      }
      else
      {
        Console.WriteLine($"{filePath}");
      }
    }
  }

  public bool TryFindDocument(
    ushort userType,
    uint userId,
    ushort repoType,
    uint repoId,
    string name,
    out ulong docId,
    out DocInfoBlock docInfoBlock,
    out DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock> node,
    out int nodeIndex
  )
  {
    var cursor = new DiskBTreeCursor<DocIdCompoundKeyBlock, DocInfoBlock>(DocTree);

    var firstKey = new DocIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId,
      Id = 0
    };

    bool hasData = cursor.MoveUntilGreaterThanOrEqual(firstKey);
    while (hasData && cursor.CurrentKey.UserType == userType && cursor.CurrentKey.UserId == userId && cursor.CurrentKey.RepoType == repoType && cursor.CurrentKey.RepoId == repoId)
    {
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(cursor.CurrentData.ExternalIdOrPathAddress);
      string currentName = nameString.GetValue();
      if (currentName == name)
      {
        docId = cursor.CurrentKey.Id;
        docInfoBlock = cursor.CurrentData;
        node = cursor.CurrentNode;
        nodeIndex = cursor.CurrentIndex;
        return true;
      }

      hasData = cursor.MoveNext();
    }

    docId = 0;
    docInfoBlock = default;
    node = default;
    nodeIndex = -1;

    return false;
  }

  public bool SetDocStatus(
    ushort userType,
    uint userId,
    ushort repoType,
    uint repoId,
    string name,
    DocStatus status,
    out ulong docId,
    out DocInfoBlock docInfoBlock,
    out DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock> node,
    out int nodeIndex
  )
  {
    bool found = TryFindDocument(userType, userId, repoType, repoId, name, out docId, out docInfoBlock, out node, out nodeIndex);
    if (!found) return false;

    docInfoBlock.Status = status;
    node.ReplaceDataAtIndex(docInfoBlock, nodeIndex);
    DocCache.Clear();

    return true;
  }

  public IEnumerable<UserInfoBlock> GetUserBlocks()
  {
    var cursor = new DiskBTreeCursor<UserIdCompoundKeyBlock, UserInfoBlock>(UserTree);

    while (cursor.MoveNext())
    {
      yield return cursor.CurrentData;
    }
  }

  public string LoadString(long address)
  {
    if (address == 0) return String.Empty;
    return DiskBlockManager.ImmutableStringFactory.LoadExisting(address).GetValue();
  }

  public IEnumerable<IUser> GetUsers()
  {
    foreach (UserInfoBlock userInfoBlock in GetUserBlocks())
    {
      var user = new User()
      {
        Id = userInfoBlock.UserId,
        Type = userInfoBlock.UserType,
        Name = LoadString(userInfoBlock.NameAddress),
        ExternalId = LoadString(userInfoBlock.ExternalIdAddress),
        LastRepoId = userInfoBlock.LastRepoId
      };

      yield return user;
    }
  }

  public IEnumerable<RepoInfoBlock> GetRepoBlocks()
  {
    var cursor = new DiskBTreeCursor<RepoIdCompoundKeyBlock, RepoInfoBlock>(RepoTree);

    while (cursor.MoveNext())
    {
      yield return cursor.CurrentData;
    }
  }

  public IEnumerable<IRepository> GetRepositories()
  {
    var cursor = new DiskBTreeCursor<RepoIdCompoundKeyBlock, RepoInfoBlock>(RepoTree);

    while (cursor.MoveNext())
    {
      var repo = new Repository()
      {
        UserType = cursor.CurrentKey.UserType,
        UserId = cursor.CurrentKey.UserId,
        Type = cursor.CurrentKey.RepoType,
        Id = cursor.CurrentKey.RepoId,
        NameAddress = cursor.CurrentData.NameAddress,
        Name = LoadString(cursor.CurrentData.NameAddress),
        ExternalIdAddress = cursor.CurrentData.ExternalIdAddress,
        ExternalId = LoadString(cursor.CurrentData.ExternalIdAddress),
        RootFolderPathAddress = cursor.CurrentData.RootFolderPathAddress,
        RootFolderPath = LoadString(cursor.CurrentData.RootFolderPathAddress),
        LastDocId = cursor.CurrentData.LastDocId,
        LastDocLength = cursor.CurrentData.LastDocLength,
        LastDocStartingOffset = cursor.CurrentData.LastDocStartingOffset
      };

      yield return repo;
    }
  }

  public IEnumerable<IDocument> GetDocuments(ushort userType, uint userId, ushort repoType, uint repoId)
  {
    var cursor = new DiskBTreeCursor<DocIdCompoundKeyBlock, DocInfoBlock>(DocTree);

    var firstKey = new DocIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId,
      Id = 0
    };

    var pastKey = new DocIdCompoundKeyBlock()
    {
      UserType = userType,
      UserId = userId,
      RepoType = repoType,
      RepoId = repoId + 1,
      Id = 0
    };

    bool hasValue = cursor.MoveUntilGreaterThanOrEqual(firstKey);

    while (hasValue && cursor.CurrentKey.CompareTo(pastKey) < 0)
    {
      var doc = new Document()
      {
        UserType = cursor.CurrentKey.UserType,
        UserId = cursor.CurrentKey.UserId,
        RepoType = cursor.CurrentKey.RepoType,
        RepoId = cursor.CurrentKey.RepoId,
        DocId = cursor.CurrentKey.Id,
        Status = cursor.CurrentData.Status,
        IsIndexed = cursor.CurrentData.IsIndexed,
        StartingOffset = cursor.CurrentData.StartingOffset,
        OriginalLength = cursor.CurrentData.OriginalLength,
        CurrentLength = cursor.CurrentData.CurrentLength,
        NameAddress = cursor.CurrentData.NameAddress,
        Name = LoadString(cursor.CurrentData.NameAddress),
        ExternalIdOrPathAddress = cursor.CurrentData.ExternalIdOrPathAddress,
        ExternalIdOrPath = LoadString(cursor.CurrentData.ExternalIdOrPathAddress),
        // RootFolderPathAddress = cursor.CurrentData.RootFolderPathAddress,
        // RootFolderPath = LoadString(cursor.CurrentData.RootFolderPathAddress),
        // LastDocId = cursor.CurrentData.LastDocId
      };

      yield return doc;

      hasValue = cursor.MoveNext();
    }
  }
}
