namespace Spinach.Caching;

public class LruCache<TKey, TValue>
{
  private readonly int capacity;
  private readonly Dictionary<TKey, LinkedListNode<CacheItem>> cache;
  private readonly LinkedList<CacheItem> lruList;

  public LruCache(int capacity)
  {
    this.capacity = capacity;
    this.cache = new Dictionary<TKey, LinkedListNode<CacheItem>>(capacity);
    this.lruList = new LinkedList<CacheItem>();
  }

  public bool TryGetValue(TKey key, out TValue val)
  {
    if (cache.TryGetValue(key, out LinkedListNode<CacheItem> node))
    {
      // Move accessed node to the head of the list.
      lruList.Remove(node);
      lruList.AddFirst(node);
      val = node.Value.Value;
      return true;
    }

    val = default;
    return false;
  }

  public void Add(TKey key, TValue value)
  {
    if (cache.Count >= capacity)
    {
      // Remove least recently used item.
      cache.Remove(lruList.Last.Value.Key);
      lruList.RemoveLast();
    }

    // Add new node to the head of the list and to the dictionary.
    var cacheItem = new CacheItem { Key = key, Value = value };
    var node = new LinkedListNode<CacheItem>(cacheItem);
    lruList.AddFirst(node);
    cache.Add(key, node);
  }

  private class CacheItem
  {
    public TKey Key { get; set; }
    public TValue Value { get; set; }
  }
}

