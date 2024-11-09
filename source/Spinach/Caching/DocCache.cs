namespace Spinach.Caching;

using TrackingObjects;

public class DocCache : LruCache<
  DocIdCompoundKeyBlock,
  Tuple<DocInfoBlock, DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock>, int, IDocument>
>
{
  public DocCache (
    ITextSearchManager textSearchManager,
    DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> docTree, int capacity
  ) : base(capacity)
  {
    TextSearchManager = textSearchManager;
    DocTree = docTree;
  }

  public ITextSearchManager TextSearchManager { get; private set; }

  public DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> DocTree { get; private set; }

  public bool TryFind (
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

    doc = new Document()
    {
      IsValid = true,
      UserType = key.UserType,
      UserId = key.UserId,
      RepoType = key.RepoType,
      RepoId = key.RepoId,
      DocId = key.Id,
      Length = docInfoBlock.Length,
      StartingOffset = docInfoBlock.StartingOffset,
      NameAddress = docInfoBlock.NameAddress,
      Name = TextSearchManager.LoadString(docInfoBlock.NameAddress),
      ExternalIdOrPathAddress = docInfoBlock.ExternalIdOrPathAddress,
      ExternalIdOrPath = externalIdOrPath,
      Content = File.ReadAllText(externalIdOrPath)
    };

    var cachedItem = new Tuple<DocInfoBlock, DiskBTreeNode<DocIdCompoundKeyBlock, DocInfoBlock>, int, IDocument>(
      docInfoBlock, node, nodeIndex, doc
    );

    this.Add(key, cachedItem);

    return true;
  }
}
