namespace Spinach.Enumerators;

public class FastLiteralEnumerator : IFastEnumerator<TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastLiteralEnumerator(
    TextSearchIndex textSearchIndex,
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
    Offset = Literal.Length - 3;
    Enumerable1 = textSearchIndex.GetFastTrigramFileEnumerable(TrigramHelper.GetLeadingTrigramKey(Literal));
    Enumerable2 = textSearchIndex.GetFastTrigramFileEnumerable(TrigramHelper.GetTrailingTrigramKey(Literal));
    Enumerator1 = Enumerable1.GetFastEnumerator();
    Enumerator2 = Enumerable2.GetFastEnumerator();
    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBTreeCursor<long, long> BTreeCursor { get; set; }

  private FastTrigramFileEnumerable Enumerable1 { get; }

  private FastTrigramFileEnumerable Enumerable2 { get; }

  private IFastEnumerator<TrigramFileInfo, int> Enumerator1 { get; }

  private IFastEnumerator<TrigramFileInfo, int> Enumerator2 { get; }

  private DiskBTree<long, long> InternalFileIdTree { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  private string Literal { get; set; }

  private int Offset { get; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  private DiskLinkedList<long>.Position PostingsListPosition { get; set; }

  private DiskBTree<long, long> TrigramFileIdTree { get; set; }

  private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private int TrigramKey { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public TrigramFileInfo Current => CurrentKey;

  public int CurrentData { get; }

  public TrigramFileInfo CurrentKey { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private int CompareTo(TrigramFileInfo tfi1, TrigramFileInfo tfi2)
  {
    // The trigram positions need to correlate to the corresponding
    // positions within the target string literal
    if (tfi1.FileId < tfi2.FileId) return -1;
    if (tfi1.FileId > tfi2.FileId) return 1;
    if (tfi1.Position < tfi2.Position - Offset) return -1;
    if (tfi1.Position > tfi2.Position - Offset) return 1;

    return 0;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    bool hasValue1 = Enumerator1.MoveNext();
    bool hasValue2 = Enumerator2.MoveNext();

    while (hasValue1 && hasValue2)
    {
      int comparison = CompareTo(Enumerator1.CurrentKey, Enumerator2.CurrentKey);

      if (comparison < 0)
      {
        var next = new TrigramFileInfo(Enumerator2.CurrentKey.FileId, Enumerator2.CurrentKey.Position - Offset);
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(next);
      }
      else if (comparison > 0)
      {
        var next = new TrigramFileInfo(Enumerator1.CurrentKey.FileId, Enumerator1.CurrentKey.Position + Offset);
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(next);
      }
      else
      {
        CurrentKey = Enumerator1.CurrentKey;
        return true;
      }
    }

    return false;
  }

  public bool MoveUntilGreaterThanOrEqual(TrigramFileInfo target)
  {
    bool hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(target);
    bool hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(target);

    while (hasValue1 && hasValue2)
    {
      int comparison = CompareTo(Enumerator1.CurrentKey, Enumerator2.CurrentKey);

      if (comparison < 0)
      {
        var next = new TrigramFileInfo(Enumerator2.CurrentKey.FileId, Enumerator2.CurrentKey.Position - Offset);
        hasValue1 = Enumerator1.MoveUntilGreaterThanOrEqual(next);
      }
      else if (comparison > 0)
      {
        var next = new TrigramFileInfo(Enumerator1.CurrentKey.FileId, Enumerator1.CurrentKey.Position + Offset);
        hasValue2 = Enumerator2.MoveUntilGreaterThanOrEqual(next);
      }
      else
      {
        CurrentKey = Enumerator1.CurrentKey;
        return true;
      }
    }

    return false;
  }

  public void Reset()
  {
    Enumerator1.Reset();
    Enumerator2.Reset();
  }
}
