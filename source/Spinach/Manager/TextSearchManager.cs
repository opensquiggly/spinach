namespace Spinach.Manager;

using TrackingObjects;

public class TextSearchManager : ITextSearchManager
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
  }

  protected short DocInfoBlockType { get; set; }

  protected short DocIdCompoundKeyBlockType { get; set; }

  protected short TrigramKeyType { get; set; }

  protected short TrigramMatchKeyType { get; set; }

  protected short UserIdCompoundKeyBlockType { get; set; }

  protected short UserInfoBlockType { get; set; }

  protected short RepoIdCompoundKeyBlockType { get; set; }

  protected short RepoInfoBlockType { get; set; }

  public DiskBlockManager DiskBlockManager { get; }

  public string FileName { get; set; }

  public bool IsOpen { get; set; } = false;

  public DiskBTree<UserIdCompoundKeyBlock, UserInfoBlock> UserTree { get; private set; }

  public DiskBTree<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTree { get; private set; }

  public DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> DocTree { get; private set; }

  protected DiskBTreeFactory<UserIdCompoundKeyBlock, UserInfoBlock> UserTreeFactory { get; private set; }

  protected DiskBTreeFactory<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTreeFactory { get; private set; }
  protected DiskBTreeFactory<DocIdCompoundKeyBlock, DocInfoBlock> DocTreeFactory { get; private set; }

  public void CreateNewIndexFile(string filename)
  {
    DiskBlockManager.Close();
    DiskBlockManager.CreateOrOpen(filename);
    _headerBlock = DiskBlockManager.GetHeaderBlock();

    UserTree = UserTreeFactory.AppendNew(25);
    RepoTree = RepoTreeFactory.AppendNew(25);
    DocTree = DocTreeFactory.AppendNew(25);

    // TrigramTree = TrigramTreeFactory.AppendNew(25);

    _headerBlock.Address1 = UserTree.Address;
    _headerBlock.Address2 = RepoTree.Address;
    _headerBlock.Address3 = DocTree.Address;
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
    RepoTree = RepoTreeFactory.LoadExisting(_headerBlock.Address2);
    DocTree = DocTreeFactory.LoadExisting(_headerBlock.Address3);

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

  public void Flush()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    DiskBlockManager.Flush();
  }

  public string LoadImmutableString(long address)
  {
    if (address == 0)
    {
      return string.Empty;
    }

    return DiskBlockManager.ImmutableStringFactory.LoadExisting(address).GetValue();
  }

  public void AddUser(ushort userType, string userName, string userExternalId)
  {
    uint nextUserId = (uint) ++_headerBlock.Data1;
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
    Console.WriteLine($"Result = {result}");
  }

  public void AddRepository(ushort userType, uint userId, ushort repoType, string externalId, string name, string rootFolder)
  {
    var userIdCompoundKeyBlock = new UserIdCompoundKeyBlock
    {
      UserType = userType,
      UserId = userId
    };

    bool found = UserTree.TryFind(userIdCompoundKeyBlock, out UserInfoBlock userInfoBlock, out DiskBTreeNode<UserIdCompoundKeyBlock, UserInfoBlock> node, out int nodeIndex);
    if (found)
    {
      RepoIdCompoundKeyBlock repoIdCompoundKeyBlock = new RepoIdCompoundKeyBlock();
      repoIdCompoundKeyBlock.UserType = userInfoBlock.UserType;
      repoIdCompoundKeyBlock.UserId = userInfoBlock.UserId;
      repoIdCompoundKeyBlock.RepoType = repoType;

      RepoInfoBlock repoInfoBlock = new RepoInfoBlock();

      repoInfoBlock.InternalId = ++userInfoBlock.LastRepoId; // TODO - No way to write this back to disk
      repoInfoBlock.LastDocId = 0;
      Console.WriteLine($"Repo Id = {repoInfoBlock.InternalId}");
      repoIdCompoundKeyBlock.RepoId = repoInfoBlock.InternalId;

      node.ReplaceDataAtIndex(userInfoBlock, nodeIndex);

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
    }
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

  public void IndexLocalFiles(ushort userType, uint userId, ushort repoType, uint repoId)
  {
    uint currentFileId = 1;
    ulong currentOffset = 0;

    var repoIdCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = userType, UserId = userId, RepoType = repoType, RepoId = repoId
    };

    bool found = RepoTree.TryFind(repoIdCompoundKey, out RepoInfoBlock data, out DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock> node, out int nodeIndex);
    string rootFolderPath = LoadString(data.RootFolderPathAddress);
    uint currentDocId = data.LastDocId;

    foreach (string filePath in Directory.GetFiles(rootFolderPath, "*.*", SearchOption.AllDirectories))
    {
      if (!IncludeFileInIndex(filePath))
      {
        continue;
      }

      long externalIdOrPathAddress = DiskBlockManager.ImmutableStringFactory.Append(filePath).Address;

      var docIdCompoundKey = new DocIdCompoundKeyBlock()
      {
        UserType = userType,
        UserId = userId,
        RepoType = repoType,
        RepoId = repoId,
        Id = ++currentDocId
      };

      var docInfoBlock = new DocInfoBlock()
      {
        NameAddress = 0,
        ExternalIdOrPathAddress = externalIdOrPathAddress,
        Length = GetFileLength(filePath),
        StartingOffset = currentOffset
      };

      DocTree.Insert(docIdCompoundKey, docInfoBlock);
      Console.Write($"Adding file ... Id = {docIdCompoundKey.Id}, ");
      Console.Write($"UserType = {docIdCompoundKey.UserType}, ");
      Console.Write($"UserId = {docIdCompoundKey.UserId}, ");
      Console.Write($"RepoType = {docIdCompoundKey.RepoType}, ");
      Console.Write($"RepoId = {docIdCompoundKey.RepoId}, ");
      Console.WriteLine($"{filePath}");
    }

    data.LastDocId = currentDocId;
    node.ReplaceDataAtIndex(data, nodeIndex);
  }

  public void PrintLocalFiles(ushort userType, uint userId, ushort repoType, uint repoId)
  {
    var repoIdCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = userType, UserId = userId, RepoType = repoType, RepoId = repoId
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
        LastDocId = cursor.CurrentData.LastDocId
      };

      yield return repo;
    }
  }
}
