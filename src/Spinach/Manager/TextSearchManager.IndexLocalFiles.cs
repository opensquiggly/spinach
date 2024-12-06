namespace Spinach.Manager;

using System.Diagnostics;

public partial class TextSearchManager
{
  public bool IndexLocalFilesForSliceOfTime(Stopwatch watch, CancellationToken cancellationToken, int milliseconds = 5000)
  {
    var cursor = new DiskBTreeCursor<RepoIdCompoundKeyBlock, RepoInfoBlock>(RepoTree);
    cursor.Reset();

    while (cursor.MoveNext())
    {
      if (watch.ElapsedMilliseconds > milliseconds)
        return true;

      if (cancellationToken.IsCancellationRequested)
        return true;

      var repoInfo = cursor.CurrentData;
      var repoKey = cursor.CurrentKey;

      // Skip if repo doesn't have files to index
      if (!repoInfo.HasFilesToIndex)
        continue;

      // Skip if repo is already being indexed
      if (repoInfo.IsIndexing)
        continue;

      // Mark repo as being indexed
      repoInfo.IsIndexing = true;
      cursor.CurrentNode.ReplaceDataAtIndex(repoInfo, cursor.CurrentIndex);
      RepoCache.Clear();

      // Index files for this repo
      bool hasMoreFiles = IndexLocalFilesForSliceOfTime(
        repoKey.UserType,
        repoKey.UserId,
        repoKey.RepoType,
        repoKey.RepoId,
        watch,
        cancellationToken,
        milliseconds
      );

      // Update repo state
      repoInfo = cursor.CurrentData;  // Reload in case it changed
      repoInfo.IsIndexing = false;
      repoInfo.HasFilesToIndex = hasMoreFiles;
      cursor.CurrentNode.ReplaceDataAtIndex(repoInfo, cursor.CurrentIndex);
      RepoCache.Clear();

      // If we still have files to index in this repo, return true
      if (hasMoreFiles)
        return true;

      _currentEnumerator = null;
    }

    // Return false when all repos are complete
    return false;
  }
}
