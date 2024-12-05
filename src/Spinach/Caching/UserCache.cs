namespace Spinach.Caching;

using TrackingObjects;

public class UserCache : LruCache<
  UserIdCompoundKeyBlock,
  Tuple<UserInfoBlock, DiskBTreeNode<UserIdCompoundKeyBlock, UserInfoBlock>, int, IUser>
>
{
  public UserCache(
    ITextSearchManager textSearchManager,
    DiskBTree<UserIdCompoundKeyBlock, UserInfoBlock> userTree, int capacity
  ) : base(capacity)
  {
    TextSearchManager = textSearchManager;
    UserTree = userTree;
  }

  public ITextSearchManager TextSearchManager { get; private set; }

  public DiskBTree<UserIdCompoundKeyBlock, UserInfoBlock> UserTree { get; private set; }

  public bool TryFind(
    UserIdCompoundKeyBlock key,
    out UserInfoBlock userInfoBlock,
    out DiskBTreeNode<UserIdCompoundKeyBlock, UserInfoBlock> node,
    out int nodeIndex,
    out IUser user
  )
  {
    if (this.TryGetValue(key, out Tuple<UserInfoBlock, DiskBTreeNode<UserIdCompoundKeyBlock, UserInfoBlock>, int, IUser> val))
    {
      userInfoBlock = val.Item1;
      node = val.Item2;
      nodeIndex = val.Item3;
      user = val.Item4;

      return true;
    }

    if (!UserTree.TryFind(key, out userInfoBlock, out node, out nodeIndex))
    {
      user = User.InvalidUser;
      return false;
    }

    user = new User()
    {
      IsValid = true,
      Id = userInfoBlock.UserId,
      Type = userInfoBlock.UserType,
      NameAddress = userInfoBlock.NameAddress,
      Name = TextSearchManager.LoadString(userInfoBlock.NameAddress),
      ExternalIdAddress = userInfoBlock.ExternalIdAddress,
      ExternalId = TextSearchManager.LoadString(userInfoBlock.ExternalIdAddress),
      LastRepoId = userInfoBlock.LastRepoId
    };

    var cachedItem = new Tuple<UserInfoBlock, DiskBTreeNode<UserIdCompoundKeyBlock, UserInfoBlock>, int, IUser>(
      userInfoBlock, node, nodeIndex, user
    );

    this.Add(key, cachedItem);

    return true;
  }
}
