namespace Spinach.Enumerators;

public class FastTrigramFileEnumerator : IFastEnumerator<TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramFileEnumerator(
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
    Reset();
  }
  
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBTreeCursor<long, long> BTreeCursor { get; set; }
  
  private DiskBTree<long, long> InternalFileIdTree { get; set; }
  
  private DiskLinkedListFactory<long> LinkedListOfLongFactory { get; set; }
  
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
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    if (TrigramFileIdTree == null)
    {
      return false;
    }
    
    if (PostingsListPosition != null)
    {
      PostingsListPosition.Next();
      if (!PostingsListPosition.IsPastTail)
      {
        CurrentKey = new TrigramFileInfo() { FileId = BTreeCursor.CurrentKey, Position = PostingsListPosition.Value };
        return true;
      }
    }

    while (BTreeCursor.MoveNext())
    {
      var postKey = new Tuple<int, long>(TrigramKey, BTreeCursor.CurrentKey);

      if (!PostingsListCache.TryGetValue(postKey, out DiskLinkedList<long> postingsList))
      {
        postingsList = LinkedListOfLongFactory.LoadExisting(BTreeCursor.CurrentData);
      }

      PostingsListPosition = postingsList.GetFirst();  
      if (!PostingsListPosition.IsPastTail)
      {
        CurrentKey = new TrigramFileInfo() { FileId = BTreeCursor.CurrentKey, Position = PostingsListPosition.Value };
        return true;
      }
    }

    return false;
  }

  public bool MoveUntilGreaterThanOrEqual(TrigramFileInfo target)
  {
    if (TrigramFileIdTree == null)
    {
      return false;
    }

    if (PostingsListPosition != null && target.FileId == BTreeCursor.CurrentKey)
    {
      while (!PostingsListPosition.IsPastTail)
      {
        PostingsListPosition.Next();
        if (PostingsListPosition.Value >= target.Position)
        {
          CurrentKey = new TrigramFileInfo() { FileId = BTreeCursor.CurrentKey, Position = PostingsListPosition.Value };
          return true;          
        }
      }
    }

    if (BTreeCursor.MoveUntilGreaterThanOrEqual(target.FileId))
    {
      do
      {
        var postKey = new Tuple<int, long>(TrigramKey, BTreeCursor.CurrentKey);

        if (!PostingsListCache.TryGetValue(postKey, out DiskLinkedList<long> postingsList))
        {
          postingsList = LinkedListOfLongFactory.LoadExisting(BTreeCursor.CurrentData);
        }

        PostingsListPosition = postingsList.GetFirst();
        while (!PostingsListPosition.IsPastTail)
        {
          if (PostingsListPosition.Value >= target.Position)
          {
            CurrentKey =
              new TrigramFileInfo() { FileId = BTreeCursor.CurrentKey, Position = PostingsListPosition.Value };
            return true;
          }

          PostingsListPosition.Next();
        }
      } while (BTreeCursor.MoveNext());
    }

    return false;
  }

  public void Reset()
  {
    if (!TrigramFileIdTreeCache.TryGetValue(TrigramKey, out DiskBTree<long, long> trigramFileIdTree))
    {
      if (TrigramTree.TryFind(TrigramKey, out long trigramFileIdTreeAddress))
      {
        trigramFileIdTree =
          TrigramFileTreeFactory.LoadExisting(trigramFileIdTreeAddress);

        TrigramFileIdTreeCache.Add(TrigramKey, trigramFileIdTree);
      }
    }
    
    TrigramFileIdTree = trigramFileIdTree;

    BTreeCursor = trigramFileIdTree.GetFirst();
    
    // The Reset() method should position itself before the first item
    BTreeCursor.MovePrevious();
    PostingsListPosition = null;
  }
}
