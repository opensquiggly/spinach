namespace Spinach.Blocks;

public struct UserIdCompoundKeyBlock : IComparable<UserIdCompoundKeyBlock>, IComparable
{
  public UserIdCompoundKeyBlock()
  {
  }

  public UserIdCompoundKeyBlock(ushort userType, uint userId)
  {
    UserType = userType;
    UserId = userId;
  }

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public int CompareTo(UserIdCompoundKeyBlock other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((UserIdCompoundKeyBlock) obj);
}
