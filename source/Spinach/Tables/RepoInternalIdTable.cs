namespace Spinach;

public class RepoInternalIdTable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public RepoInternalIdTable(DiskBlockManager diskBlockManager, DiskBTreeFactory<long, RepoInfoBlock> factory)
  {
    DiskBlockManager = diskBlockManager;
    Factory = factory;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBlockManager DiskBlockManager { get; }

  private DiskBTreeFactory<long, RepoInfoBlock> Factory { get; }

  public DiskBTree<long, RepoInfoBlock> RepoIdTree { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long FindRepo(string externalId) => 0;

  public DiskBTree<long, RepoInfoBlock> AppendNew()
  {
    RepoIdTree = Factory.AppendNew(25);
    return RepoIdTree;
  }
  public void Load(long address) => RepoIdTree = Factory.LoadExisting(address);

  public void AddRepo(long internalId, string externalId, string name, string rootFolder)
  {
    RepoInfoBlock repoInfoBlock = default;

    if (externalId != null)
    {
      Eugene.Collections.DiskImmutableString externalIdString =
        DiskBlockManager.ImmutableStringFactory.Append(externalId);
      repoInfoBlock.ExternalIdAddress = externalIdString.Address;
    }
    else
    {
      repoInfoBlock.ExternalIdAddress = 0;
    }

    DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(name);
    DiskImmutableString rootFolderString = DiskBlockManager.ImmutableStringFactory.Append(rootFolder);

    repoInfoBlock.InternalId = (uint) internalId;
    repoInfoBlock.NameAddress = nameString.Address;
    repoInfoBlock.RootFolderPathAddress = rootFolderString.Address;

    RepoIdTree.Insert(internalId, repoInfoBlock);
  }

  public IEnumerable<RepoInfoBlock> GetRepositories()
  {
    var cursor = new DiskBTreeCursor<long, RepoInfoBlock>(RepoIdTree);

    while (cursor.MoveNext())
    {
      yield return cursor.CurrentData;
    }
  }

  public DiskBTreeCursor<long, RepoInfoBlock> GetRepositoriesCursor() => new(RepoIdTree);

  public RepoInfoBlock FindRepository(long internalId) => RepoIdTree.Find(internalId);
}
