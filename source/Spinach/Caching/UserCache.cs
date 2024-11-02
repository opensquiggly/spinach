namespace Spinach.Caching;

public class UserCache : LruCache<UserIdCompoundKeyBlock, IUser>
{
  public UserCache(DiskBlockManager diskBlockManager, int capacity) : base(capacity)
  {
    DiskBlockManager = diskBlockManager;
  }

  private DiskBlockManager DiskBlockManager { get; set; }
}
