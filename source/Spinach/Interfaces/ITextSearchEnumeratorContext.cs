namespace Spinach.Interfaces;

public interface ITextSearchEnumeratorContext
{
  // Properties
  DiskBlockManager DiskBlockManager { get; }
  DiskBTree<int, long> TrigramTree { get; }
  DiskBTree<UserIdCompoundKeyBlock, UserInfoBlock> UserTree { get; }
  DiskBTree<RepoIdCompoundKeyBlock, RepoInfoBlock> RepoTree { get; }
  DiskBTree<DocIdCompoundKeyBlock, DocInfoBlock> DocTree { get; }
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