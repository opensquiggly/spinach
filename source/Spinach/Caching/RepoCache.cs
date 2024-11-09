namespace Spinach.Caching;

using TrackingObjects;

public class RepoCache : LruCache<
  RepoIdCompoundKeyBlock,
  Tuple<RepoInfoBlock, DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock>, int, IRepository>
>
{
  public RepoCache(
    ITextSearchManager textSearchManager,
    DiskBTree<RepoIdCompoundKeyBlock, RepoInfoBlock> repoTree, int capacity
  ) : base(capacity)
  {
    TextSearchManager = textSearchManager;
    RepoTree = repoTree;
  }

  public ITextSearchManager TextSearchManager { get; private set; }

  public DiskBTree<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTree { get; private set; }

  public bool TryFind(
    RepoIdCompoundKeyBlock key,
    out RepoInfoBlock repoInfoBlock,
    out DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock> node,
    out int nodeIndex,
    out IRepository repo
  )
  {
    if (this.TryGetValue(key, out Tuple<RepoInfoBlock, DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock>, int, IRepository> val))
    {
      repoInfoBlock = val.Item1;
      node = val.Item2;
      nodeIndex = val.Item3;
      repo = val.Item4;

      return true;
    }

    if (!RepoTree.TryFind(key, out repoInfoBlock, out node, out nodeIndex))
    {
      repo = Repository.InvalidRepository;
      return false;
    }

    repo = new Repository()
    {
      IsValid = true,
      Type = key.RepoType,
      Id = repoInfoBlock.InternalId,
      UserId = key.UserId,
      NameAddress = repoInfoBlock.NameAddress,
      Name = TextSearchManager.LoadString(repoInfoBlock.NameAddress),
      LastDocId = repoInfoBlock.LastDocId,
      ExternalIdAddress = repoInfoBlock.ExternalIdAddress,
      ExternalId = TextSearchManager.LoadString(repoInfoBlock.ExternalIdAddress),
      RootFolderPathAddress = repoInfoBlock.RootFolderPathAddress,
      RootFolderPath = TextSearchManager.LoadString(repoInfoBlock.RootFolderPathAddress)
    };

    var cachedItem = new Tuple<RepoInfoBlock, DiskBTreeNode<RepoIdCompoundKeyBlock, RepoInfoBlock>, int, IRepository>(
      repoInfoBlock, node, nodeIndex, repo
    );

    this.Add(key, cachedItem);

    return true;
  }
}
