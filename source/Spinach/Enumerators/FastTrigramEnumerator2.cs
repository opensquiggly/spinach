namespace Spinach.Enumerators;

using TrackingObjects;

public class FastTrigramEnumerator2 : IFastEnumerator<TrigramMatchPositionKey, ulong>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerator2(string trigram, ITextSearchEnumeratorContext context)
  {
    if (trigram is not {Length: 3})
    {
      throw new ArgumentOutOfRangeException("Trigrams should be 3 letters long");
    }

    TrigramKey = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);
    Context = context;
    CurrentKey = new TrigramMatchPositionKey();

    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private int TrigramKey { get; }

  ITextSearchEnumeratorContext Context { get; set; }

  private DiskBTree<TrigramMatchKey, long> TrigramMatchesTree { get; set; }

  private DiskBTreeCursor<TrigramMatchKey, long> TrigramMatchesCursor { get; set; }

  private DiskBTreeCursor<DocOffsetCompoundKeyBlock, uint> DocTreeByOffsetCursor { get; set; }

  private DiskSortedVarIntList CurrentPostingsList { get; set; }

  private DiskSortedVarIntListCursor CurrentPostingsListCursor { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public TrigramMatchPositionKey Current => CurrentKey;

  public ulong CurrentData
  {
    get
    {
      if (CurrentPostingsListCursor == null || CurrentDocument == null) return 0;

      return CurrentPostingsListCursor.CurrentKey - CurrentDocument.StartingOffset;
    }
  }

  public TrigramMatchPositionKey CurrentKey { get; private set; }

  public ushort CurrentUserType { get; private set; }

  public uint CurrentUserId { get; private set; }

  public ushort CurrentRepoType { get; private set; }

  public uint CurrentRepoId { get; private set; }

  public IUser CurrentUser { get; private set; }

  public IRepository CurrentRepository { get; private set; }

  public IDocument CurrentDocument { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void SetCurrentUser()
  {
    if (TrigramMatchesCursor == null) return;

    if (CurrentUserType == TrigramMatchesCursor.CurrentKey.UserType &&
        CurrentUserId == TrigramMatchesCursor.CurrentKey.UserId)
    {
      // Nothing changed since last time
      return;
    }

    CurrentUserType = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentUserId = TrigramMatchesCursor.CurrentKey.UserId;

    var userCompoundKey = new UserIdCompoundKeyBlock()
    {
      UserType = CurrentUserType,
      UserId = CurrentUserId
    };

    bool found = Context.UserTree.TryFind(userCompoundKey, out UserInfoBlock data, out _, out _);
    if (!found)
    {
      CurrentUser = new User()
      {
        IsValid = false,
        Type = CurrentUserType,
        Id = CurrentUserId,
        NameAddress = 0,
        Name = "User Not Found",
        ExternalIdAddress = 0,
        ExternalId = null,
        LastRepoId = 0
      };
    }
    else
    {
      CurrentUser = new User()
      {
        IsValid = true,
        Type = CurrentUserType,
        Id = CurrentUserId,
        NameAddress = data.NameAddress,
        Name = Context.LoadString(data.NameAddress),
        ExternalIdAddress = data.ExternalIdAddress,
        ExternalId = Context.LoadString(data.ExternalIdAddress),
        LastRepoId = data.LastRepoId
      };
    }
  }

  private void SetCurrentRepository()
  {
    if (TrigramMatchesCursor == null) return;

    if (CurrentUserType == TrigramMatchesCursor.CurrentKey.UserType &&
        CurrentUserId == TrigramMatchesCursor.CurrentKey.UserId &&
        CurrentRepoType == TrigramMatchesCursor.CurrentKey.RepoType &&
        CurrentRepoId == TrigramMatchesCursor.CurrentKey.RepoId)
    {
      // Nothing changed since last time
      return;
    }

    CurrentRepoType = TrigramMatchesCursor.CurrentKey.RepoType;
    CurrentRepoId = TrigramMatchesCursor.CurrentKey.RepoId;

    var repoCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = CurrentUserType,
      UserId = CurrentUserId,
      RepoType = CurrentRepoType,
      RepoId = CurrentRepoId
    };

    bool found = Context.RepoTree.TryFind(repoCompoundKey, out RepoInfoBlock data, out _, out _);
    if (!found)
    {
      CurrentRepository = null;
      return;
    }

    CurrentRepository = new Repository()
    {
      Type = CurrentRepoType,
      Id = CurrentRepoId,
      NameAddress = data.NameAddress,
      Name = Context.LoadString(data.NameAddress),
      ExternalIdAddress = data.ExternalIdAddress,
      ExternalId = Context.LoadString(data.ExternalIdAddress),
      RootFolderPathAddress = data.RootFolderPathAddress,
      RootFolderPath = Context.LoadString(data.RootFolderPathAddress),
      LastDocId = data.LastDocId
    };
  }

  private void SetCurrentDocument()
  {
    var docOffsetKey = new DocOffsetCompoundKeyBlock()
    {
      UserType = CurrentUserType,
      UserId = CurrentUserId,
      RepoType = CurrentRepoType,
      RepoId = CurrentRepoId,
      StartingOffset = CurrentPostingsListCursor.CurrentKey
    };

    DocTreeByOffsetCursor.MoveUntilGreaterThanOrEqual(docOffsetKey);
    if (DocTreeByOffsetCursor.IsPastEnd || DocTreeByOffsetCursor.CurrentKey.CompareTo(docOffsetKey) > 0)
    {
      DocTreeByOffsetCursor.MovePrevious();
    }

    var docIdCompoundKey = new DocIdCompoundKeyBlock()
    {
      UserType = DocTreeByOffsetCursor.CurrentKey.UserType,
      UserId = DocTreeByOffsetCursor.CurrentKey.UserId,
      RepoType = DocTreeByOffsetCursor.CurrentKey.RepoType,
      RepoId = DocTreeByOffsetCursor.CurrentKey.RepoId,
      Id = DocTreeByOffsetCursor.CurrentData
    };

    bool found = Context.DocTree.TryFind(docIdCompoundKey, out DocInfoBlock docInfoBlock, out _, out _);
    if (!found)
    {
      CurrentDocument = null;
      return;
    }

    CurrentDocument = new Document()
    {
      UserType = DocTreeByOffsetCursor.CurrentKey.UserType,
      UserId = DocTreeByOffsetCursor.CurrentKey.UserId,
      RepoType = DocTreeByOffsetCursor.CurrentKey.RepoType,
      RepoId = DocTreeByOffsetCursor.CurrentKey.RepoId,
      DocId = DocTreeByOffsetCursor.CurrentData,
      StartingOffset = docInfoBlock.StartingOffset,
      NameAddress = docInfoBlock.NameAddress,
      Name = Context.LoadString(docInfoBlock.NameAddress),
      ExternalIdOrPathAddress = docInfoBlock.ExternalIdOrPathAddress,
      ExternalIdOrPath = Context.LoadString(docInfoBlock.ExternalIdOrPathAddress)
    };
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    if (TrigramMatchesTree == null) return false;
    if (TrigramMatchesCursor == null) return false;
    if (CurrentPostingsListCursor == null) return false;

    if (CurrentPostingsListCursor.MoveNext())
    {
      SetCurrentDocument();
      return true;
    }

    while (true)
    {
      if (!TrigramMatchesCursor.MoveNext()) return false;

      SetCurrentUser();
      SetCurrentRepository();

      CurrentPostingsList = Context.DiskBlockManager.SortedVarIntListFactory.LoadExisting(TrigramMatchesCursor.CurrentData);
      if (CurrentPostingsList == null) continue;

      CurrentPostingsListCursor = new DiskSortedVarIntListCursor(CurrentPostingsList);
      if (!CurrentPostingsListCursor.MoveNext()) continue;

      SetCurrentDocument();
      return true;
    }
  }

  public bool MoveUntilGreaterThanOrEqual(TrigramMatchPositionKey target)
  {
    throw new NotImplementedException();
  }

  public void Reset()
  {
    TrigramMatchesTree = null;
    TrigramMatchesCursor = null;
    DocTreeByOffsetCursor = null;
    CurrentPostingsList = null;
    CurrentPostingsListCursor = null;
    CurrentUserType = 0;
    CurrentUserId = 0;
    CurrentRepoType = 0;
    CurrentRepoId = 0;
    CurrentUser = null;
    CurrentRepository = null;
    CurrentDocument = null;

    bool trigramFound = Context.TrigramTree.TryFind(TrigramKey, out long trigramMatchesAddress, out _, out _);
    if (!trigramFound) return;

    TrigramMatchesTree = Context.TrigramMatchesFactory.LoadExisting(trigramMatchesAddress);
    if (TrigramMatchesTree == null) return;

    TrigramMatchesCursor = new DiskBTreeCursor<TrigramMatchKey, long>(TrigramMatchesTree);
    if (!TrigramMatchesCursor.MoveNext()) return;

    DocTreeByOffsetCursor = new DiskBTreeCursor<DocOffsetCompoundKeyBlock, uint>(Context.DocTreeByOffset);

    SetCurrentUser();
    SetCurrentRepository();

    CurrentPostingsList = Context.DiskBlockManager.SortedVarIntListFactory.LoadExisting(TrigramMatchesCursor.CurrentData);
    if (CurrentPostingsList == null) return;

    CurrentPostingsListCursor = new DiskSortedVarIntListCursor(CurrentPostingsList);
  }
}
