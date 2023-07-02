namespace Spinach;

public class RepoInternalIdTable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public RepoInternalIdTable(DiskBlockManager diskBlockManager)
  {
    DiskBlockManager = diskBlockManager;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskBlockManager DiskBlockManager { get; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public long FindRepo(string externalId) => 0;

  public void AddRepo(long internalId, string externalId, string name)
  {
    Eugene.Collections.DiskImmutableString externalIdString = DiskBlockManager.ImmutableStringFactory.Append(externalId);
    Eugene.Collections.DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.Append(name);

    RepoInfoBlock repoInfoBlock = default;
    repoInfoBlock.InternalId = internalId;
    repoInfoBlock.ExternalIdAddress = externalIdString.Address;
    repoInfoBlock.NameAddress = nameString.Address;
  }

}
