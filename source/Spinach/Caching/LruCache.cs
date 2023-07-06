namespace Spinach.Caching;

public class LruCache<TKey, TValue>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public LruCache(int capacity)
  {
    this._capacity = capacity;
    this._cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
    this._lruList = new LinkedList<CacheItem>();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Member Variables
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private readonly int _capacity;
  private readonly Dictionary<TKey, LinkedListNode<CacheItem>> _cache;
  private readonly LinkedList<CacheItem> _lruList;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Add(TKey key, TValue value)
  {
    if (_cache.Count >= _capacity)
    {
      // Remove least recently used item.
      _cache.Remove(_lruList.Last.Value.Key);
      _lruList.RemoveLast();
    }

    // Add new node to the head of the list and to the dictionary.
    var cacheItem = new CacheItem { Key = key, Value = value };
    var node = new LinkedListNode<CacheItem>(cacheItem);
    _lruList.AddFirst(node);
    _cache.Add(key, node);
  }

  public bool TryGetValue(TKey key, out TValue val)
  {
    if (_cache.TryGetValue(key, out LinkedListNode<CacheItem> node))
    {
      // Move accessed node to the head of the list.
      _lruList.Remove(node);
      _lruList.AddFirst(node);
      val = node.Value.Value;
      return true;
    }

    val = default;
    return false;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Inner Classes
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private class CacheItem
  {
    public TKey Key { get; init; }
    public TValue Value { get; init; }
  }
}

