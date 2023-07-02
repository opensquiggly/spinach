namespace Spinach.Enumerators;

public class FastLiteralEnumerator : IFastEnumerator<TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerator(
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
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBTreeCursor<long, long> BTreeCursor { get; set; }

  private DiskBTree<long, long> InternalFileIdTree { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  private string Literal { get; set; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  private DiskLinkedList<long>.Position PostingsListPosition { get; set; }

  private DiskBTree<long, long> TrigramFileIdTree { get; set; }

  private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private int TrigramKey { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  public bool MoveNext() => throw new NotImplementedException();

  public void Reset() => throw new NotImplementedException();

  public TrigramFileInfo Current { get; }

  object IEnumerator.Current => Current;

  public void Dispose() => throw new NotImplementedException();

  public bool MoveUntilGreaterThanOrEqual(TrigramFileInfo target) => throw new NotImplementedException();

  public TrigramFileInfo CurrentKey { get; }

  public int CurrentData { get; }
}
