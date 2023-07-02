namespace Spinach.Enumerators;

public class TrigramFileEnumerator : IEnumerable<TrigramFileInfo>
{
  public TrigramFileEnumerator(
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

  private int TrigramKey { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  private LruCache<int, DiskBTree<long, long>> TrigramFileIdTreeCache { get; }

  private DiskBTreeFactory<long, long> TrigramFileTreeFactory { get; set; }

  private DiskBTree<long, long> InternalFileIdTree { get; set; }

  private LruCache<Tuple<int, long>, DiskLinkedList<long>> PostingsListCache { get; set; }

  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }

  public IEnumerator<TrigramFileInfo> GetEnumerator()
  {
    if (!TrigramFileIdTreeCache.TryGetValue(TrigramKey, out DiskBTree<long, long> trigramFileIdTree))
    {
      if (TrigramTree.TryFind(TrigramKey, out long trigramFileIdTreeAddress))
      {
        trigramFileIdTree =
          TrigramFileTreeFactory.LoadExisting(trigramFileIdTreeAddress);

        TrigramFileIdTreeCache.Add(TrigramKey, trigramFileIdTree);
      }
      else
      {
        trigramFileIdTree = null;
      }
    }

    if (trigramFileIdTree == null)
    {
      goto exitEnumeration;
    }

    DiskBTreeCursor<long, long> cursor = trigramFileIdTree.GetFirst();
    while (!cursor.IsPastEnd)
    {
      if (!InternalFileIdTree.TryFind(cursor.CurrentKey, out long nameAddress))
      {
        continue;
      }

      var postKey = new Tuple<int, long>(TrigramKey, cursor.CurrentKey);

      if (!PostingsListCache.TryGetValue(postKey, out DiskLinkedList<long> postingsList))
      {
        postingsList = LinkedListOfLongFactory.LoadExisting(cursor.CurrentData);
      }

      DiskLinkedList<long>.Position postListPos = postingsList.GetFirst();
      while (!postListPos.IsPastTail)
      {
        yield return new TrigramFileInfo() { FileId = cursor.CurrentKey, Position = postListPos.Value };
        postListPos.Next();
      }

      cursor.MoveNext();
    }

  exitEnumeration:
    ;
  }

  IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
