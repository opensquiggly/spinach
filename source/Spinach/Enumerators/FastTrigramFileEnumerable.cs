namespace Spinach.Enumerators;

public class FastTrigramFileEnumerable : IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramFileEnumerable(
    LruCache<int, DiskBTree<long, long>> trigramFileIdTreeCache,
    DiskBTree<int, long> trigramTree,
    DiskBTreeFactory<long, long> trigramFileTreeFactory,
    DiskBTree<long, long> internalFileIdTree,
    LruCache<Tuple<int, long>, DiskLinkedList<long>> postingsListCache,
    DiskLinkedListFactory<long> linkedListOfLongFactory,
    int trigramKey
  )
  {
    TrigramFileIdTreeCache = trigramFileIdTreeCache;
    TrigramTree = trigramTree;
    TrigramFileTreeFactory = trigramFileTreeFactory;
    InternalFileIdTree = internalFileIdTree;
    PostingsListCache = postingsListCache;
    LinkedListOfLongFactory = linkedListOfLongFactory;
    TrigramKey = trigramKey;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBTree<long, long> InternalFileIdTree { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private int TrigramKey { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<TrigramFileInfo> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramFileInfo, int> GetFastEnumerator()
  {
    return new FastTrigramFileEnumerator(
      TrigramFileIdTreeCache,
      TrigramTree,
      TrigramFileTreeFactory,
      InternalFileIdTree,
      PostingsListCache,
      LinkedListOfLongFactory,
      TrigramKey
    );
  }
}
