namespace Spinach.Caching;

using TrackingObjects;

public class DocCache : LruCache<
  DocIdCompoundKeyBlock,
  Tuple<DocInfoBlock, DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock>, int, IDocument>
>
{
  public DocCache(
    ITextSearchManager textSearchManager,
    RepoCache repoCache,
    DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> docTree, int capacity
  ) : base(capacity)
  {
    TextSearchManager = textSearchManager;
    RepoCache = repoCache;
    DocTree = docTree;
  }

  public ITextSearchManager TextSearchManager { get; private set; }

  public RepoCache RepoCache { get; private set; }

  public DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> DocTree { get; private set; }

  public bool TryFind(
    DocIdCompoundKeyBlock key,
    out DocInfoBlock docInfoBlock,
    out DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock> node,
    out int nodeIndex,
    out IDocument doc
  )
  {
    if (this.TryGetValue(key, out Tuple<DocInfoBlock, DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock>, int, IDocument> val))
    {
      docInfoBlock = val.Item1;
      node = val.Item2;
      nodeIndex = val.Item3;
      doc = val.Item4;

      return true;
    }

    if (!DocTree.TryFind(key, out docInfoBlock, out node, out nodeIndex))
    {
      doc = Document.InvalidDocument;
      return false;
    }

    string externalIdOrPath = TextSearchManager.LoadString(docInfoBlock.ExternalIdOrPathAddress);

    var repoKey = new RepoIdCompoundKeyBlock()
    {
      UserType = key.UserType,
      UserId = key.UserId,
      RepoType = key.RepoType,
      RepoId = key.RepoId
    };

    if (!RepoCache.TryFind(repoKey, out RepoInfoBlock repoInfoBlock, out _, out _, out IRepository repo))
    {
      doc = Document.InvalidDocument;
      return false;
    }

    doc = new Document()
    {
      IsValid = true,
      UserType = key.UserType,
      UserId = key.UserId,
      RepoType = key.RepoType,
      RepoId = key.RepoId,
      DocId = key.Id,
      OriginalLength = docInfoBlock.OriginalLength,
      CurrentLength = docInfoBlock.CurrentLength,
      StartingOffset = docInfoBlock.StartingOffset,
      Status = docInfoBlock.Status,
      IsIndexed = docInfoBlock.IsIndexed,
      NameAddress = docInfoBlock.NameAddress,
      Name = TextSearchManager.LoadString(docInfoBlock.NameAddress),
      ExternalIdOrPathAddress = docInfoBlock.ExternalIdOrPathAddress,
      ExternalIdOrPath = externalIdOrPath,
      FullPath = Path.Combine(repo.RootFolderPath, externalIdOrPath)
    };

    var cachedItem = new Tuple<DocInfoBlock, DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock>, int, IDocument>(
      docInfoBlock, node, nodeIndex, doc
    );

    this.Add(key, cachedItem);

    return true;
  }
}
