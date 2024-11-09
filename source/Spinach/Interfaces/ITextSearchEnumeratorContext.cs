namespace Spinach.Interfaces;

public interface ITextSearchEnumeratorContext
{
  // Properties
  DiskBlockManager DiskBlockManager { get; }
  DiskBTree<int, long> TrigramTree { get; }
  UserCache UserCache { get; }
  RepoCache RepoCache { get; }
  DocCache DocCache { get; }
  DiskBTree<DocOffsetCompoundKeyBlock, uint> DocTreeByOffset { get; }
  DiskBTreeFactory<int, long> TrigramTreeFactory { get; }
  DiskBTreeFactory<TrigramMatchKey, long> TrigramMatchesFactory { get; }
  DiskBTreeFactory<UserIdCompoundKeyBlock, UserInfoBlock> UserTreeFactory { get; }
  DiskBTreeFactory<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTreeFactory { get; }
  DiskBTreeFactory<DocIdCompoundKeyBlock, DocInfoBlock> DocTreeFactory { get; }
  DiskBTreeFactory<DocOffsetCompoundKeyBlock, uint> DocTreeByOffsetFactory { get; }
  LruCache<TrigramMatchCacheKey, DiskSortedVarIntList> PostingsListCache { get; }

  // Methods
  string LoadString(long address);
}
