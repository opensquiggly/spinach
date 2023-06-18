using Eugene;
using Eugene.Blocks;
using Spinach.Blocks;

namespace Spinach;

public class TextSearchIndex
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public TextSearchIndex(string path)
  {
    DiskBlockManager = new DiskBlockManager();
    DiskBlockManager.RegisterBlockType<RepoInfoBlock>();
    RepoExternalIdTable = new RepoExternalIdTable(DiskBlockManager);
    RepoInternalIdTable = new RepoInternalIdTable(DiskBlockManager);
    FileExternalIdTable = new FileExternalIdTable(DiskBlockManager);
    FileInternalIdTable = new FileInternalIdTable(DiskBlockManager);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private string Path { get; }

  private DiskBlockManager DiskBlockManager { get; }

  private RepoInternalIdTable RepoInternalIdTable { get; }

  private RepoExternalIdTable RepoExternalIdTable { get; }

  private FileInternalIdTable FileInternalIdTable { get; }

  private FileExternalIdTable FileExternalIdTable { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private long GetNextInternalRepoId() => 0;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Open() => DiskBlockManager.CreateOrOpen(Path);

  public long AddRepository(string externalRepoId, string repoName)
  {
    long internalId = RepoInternalIdTable.FindRepo(externalRepoId);
    if (internalId == 0)
    {
      internalId = GetNextInternalRepoId();
      RepoInternalIdTable.AddRepo(internalId, externalRepoId, repoName);
      RepoExternalIdTable.AddRepo(internalId, externalRepoId, repoName);
    }

    return internalId;
  }

  public void GetRepository(long internalRepoId)
  {
    // Look up a repository using its internally assigned numeric id
  }

  public long AddFile(long internalRepoId, string filename) =>
    // 1) Generate a new sequentially increasing integer id for the repository
    //    -> Need a place to store the last repository id
    // 2) Add repository record to internal id dictionary
    //    -> Best stored as a BPlusTreeOfLong data structure to handle large numbers of repos
    // 3) Add repository record to external id string
    //    -> This is a unique id that is supplied by the client 

    // Add the filename to the filename trigram index

    0;

  public void IndexFile(long internalRepoId, long internalFileId, string content)
  {
    // Get each trigram
    // Add trigram to the triegram trie tree if it doesn't already exist
  }

  public void GetAllTrigrams(string trigram)
  {
    // Return an enumerator that enumerats all trigrams
    // returning the tuple <internal-repo-id, internal-file-id, position>
    // for each trigram found in the index
  }
}
