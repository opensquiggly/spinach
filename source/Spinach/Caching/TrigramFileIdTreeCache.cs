namespace Spinach.Caching;

public class TrigramFileIdTreeCache : LruCache<int, DiskBTree<long, long>>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TrigramFileIdTreeCache(
    TrigramTree trigramTree,
    DiskBTreeFactory<long, long> trigramFileTreeFactory,
    int capacity = 128 * 128 * 128) : base(capacity)
  {
    TrigramTree = trigramTree;
    TrigramFileTreeFactory = trigramFileTreeFactory;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private TrigramTree TrigramTree { get; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public DiskBTree<long, long> LoadTrigramFileIdTree(int trigramKey)
  {
    if (this.TryGetValue(trigramKey, out DiskBTree<long, long> btree))
    {
      return btree;
    }

    if (TrigramTree.TryFind(trigramKey, out long trigramFileIdTreeAddress))
    {
      DiskBTree<long, long> trigramFileIdTree =
        TrigramFileTreeFactory.LoadExisting(trigramFileIdTreeAddress);

      this.Add(trigramKey, trigramFileIdTree);
      return trigramFileIdTree;
    }

    return null;
  }
}
