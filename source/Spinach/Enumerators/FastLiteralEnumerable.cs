namespace Spinach.Enumerators;

public class FastLiteralEnumerable : IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerable(
    LruCache<int, DiskBTree<long, long>> trigramFileIdTreeCache,
    DiskBTree<int, long> trigramTree,
    DiskBTreeFactory<long, long> trigramFileTreeFactory,
    DiskBTree<long, long> internalFileIdTree,
    LruCache<Tuple<int, long>, DiskLinkedList<long>> postingsListCache,
    DiskLinkedListFactory<long> linkedListOfLongFactory,
    string literal
  )
  {
    TrigramFileIdTreeCache = trigramFileIdTreeCache;
    TrigramTree = trigramTree;
    TrigramFileTreeFactory = trigramFileTreeFactory;
    InternalFileIdTree = internalFileIdTree;
    PostingsListCache = postingsListCache;
    LinkedListOfLongFactory = linkedListOfLongFactory;
    Literal = literal;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBTree<long, long> InternalFileIdTree { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  private string Literal { get; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<TrigramFileInfo> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramFileInfo, int> GetFastEnumerator()
  {
    return new FastLiteralEnumerator(
      TrigramFileIdTreeCache,
      TrigramTree,
      TrigramFileTreeFactory,
      InternalFileIdTree,
      PostingsListCache,
      LinkedListOfLongFactory,
      Literal
    );
  }
}
