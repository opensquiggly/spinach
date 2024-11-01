namespace Spinach.Enumerators;

using Misc;
using TrackingObjects;

public class FastTrigramEnumerator2 : IFastEnumerator<TrigramMatchPositionKey, TextSearchMatchData>
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
    CurrentData = new TextSearchMatchData();
    CurrentData.IsUserValid = false;
    CurrentData.IsRepositoryValid = false;
    CurrentData.IsDocumentValid = false;
    CurrentData.User = new User();
    CurrentData.Repository = new Repository();
    CurrentData.Document = new Document();
    CurrentData.MatchPosition = 0;

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

  public TextSearchMatchData Current => CurrentData;

  public TextSearchMatchData CurrentData { get; private set; }
  // {
  //   get
  //   {
  //     if (CurrentPostingsListCursor == null || CurrentDocument == null) return 0;
  //
  //     return CurrentPostingsListCursor.CurrentKey - CurrentDocument.StartingOffset;
  //   }
  // }

  public TrigramMatchPositionKey CurrentKey { get; private set; }

  public IUser CurrentUser { get; private set; }

  public IRepository CurrentRepository { get; private set; }

  public IDocument CurrentDocument { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void SetCurrentUser()
  {
    if (TrigramMatchesCursor == null) return;

    if (CurrentKey.UserType == TrigramMatchesCursor.CurrentKey.UserType &&
        CurrentKey.UserId == TrigramMatchesCursor.CurrentKey.UserId)
    {
      // Nothing changed since last time
      return;
    }

    CurrentData.User.Type = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentData.User.Id = TrigramMatchesCursor.CurrentKey.UserId;

    var userCompoundKey = new UserIdCompoundKeyBlock()
    {
      UserType = CurrentData.User.Type,
      UserId = CurrentData.User.Id
    };

    bool found = Context.UserTree.TryFind(userCompoundKey, out UserInfoBlock data, out _, out _);
    if (!found)
    {
      CurrentData.IsUserValid = false;
    }
    else
    {
      CurrentData.IsUserValid = true;
      CurrentData.User.NameAddress = data.NameAddress;
      CurrentData.User.Name = Context.LoadString(data.NameAddress);
      CurrentData.User.ExternalIdAddress = data.ExternalIdAddress;
      CurrentData.User.ExternalId = Context.LoadString(data.ExternalIdAddress);
      CurrentData.User.LastRepoId = data.LastRepoId;
    }
  }

  private void SetCurrentRepository()
  {
    if (TrigramMatchesCursor == null) return;

    if (CurrentKey.UserType == TrigramMatchesCursor.CurrentKey.UserType &&
        CurrentKey.UserId == TrigramMatchesCursor.CurrentKey.UserId &&
        CurrentKey.RepoType == TrigramMatchesCursor.CurrentKey.RepoType &&
        CurrentKey.RepoId == TrigramMatchesCursor.CurrentKey.RepoId)
    {
      // Nothing changed since last time
      return;
    }

    CurrentData.Repository.UserType = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentData.Repository.UserId = TrigramMatchesCursor.CurrentKey.UserId;
    CurrentData.Repository.Type = TrigramMatchesCursor.CurrentKey.RepoType;
    CurrentData.Repository.Id = TrigramMatchesCursor.CurrentKey.RepoId;

    var repoCompoundKey = new RepoIdCompoundKeyBlock()
    {
      UserType = CurrentData.Repository.UserType,
      UserId = CurrentData.Repository.UserId,
      RepoType = CurrentData.Repository.Type,
      RepoId = CurrentData.Repository.Id
    };

    bool found = Context.RepoTree.TryFind(repoCompoundKey, out RepoInfoBlock data, out _, out _);
    if (!found)
    {
      CurrentData.IsRepositoryValid = false;
      return;
    }

    CurrentData.IsRepositoryValid = true;
    CurrentData.Repository.NameAddress = data.NameAddress;
    CurrentData.Repository.Name = Context.LoadString(data.NameAddress);
    CurrentData.Repository.ExternalIdAddress = data.ExternalIdAddress;
    CurrentData.Repository.ExternalId = Context.LoadString(data.ExternalIdAddress);
    CurrentData.Repository.RootFolderPathAddress = data.RootFolderPathAddress;
    CurrentData.Repository.RootFolderPath = Context.LoadString(data.RootFolderPathAddress);
    CurrentData.Repository.LastDocId = data.LastDocId;
  }

  private void SetCurrentDocument()
  {
    if (CurrentKey.UserType == TrigramMatchesCursor.CurrentKey.UserType &&
        CurrentKey.UserId == TrigramMatchesCursor.CurrentKey.UserId &&
        CurrentKey.RepoType == TrigramMatchesCursor.CurrentKey.RepoType &&
        CurrentKey.RepoId == TrigramMatchesCursor.CurrentKey.RepoId &&
        CurrentKey.Offset == CurrentPostingsListCursor.CurrentKey)
    {
      // Nothing changed since last time
      return;
    }

    CurrentData.Document.UserType = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentData.Document.UserId = TrigramMatchesCursor.CurrentKey.UserId;
    CurrentData.Document.RepoType = TrigramMatchesCursor.CurrentKey.RepoType;
    CurrentData.Document.RepoId = TrigramMatchesCursor.CurrentKey.RepoId;

    var docOffsetKey = new DocOffsetCompoundKeyBlock()
    {
      UserType = CurrentData.Document.UserType,
      UserId = CurrentData.Document.UserId,
      RepoType = CurrentData.Document.RepoType,
      RepoId = CurrentData.Document.RepoId,
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
      CurrentData.IsDocumentValid = false;
      return;
    }

    CurrentData.Document.DocId = DocTreeByOffsetCursor.CurrentData;
    CurrentData.Document.Length = docInfoBlock.Length;
    CurrentData.Document.StartingOffset = docInfoBlock.StartingOffset;
    CurrentData.Document.NameAddress = docInfoBlock.NameAddress;
    CurrentData.Document.Name = Context.LoadString(docInfoBlock.NameAddress);
    CurrentData.Document.ExternalIdOrPathAddress = docInfoBlock.ExternalIdOrPathAddress;
    CurrentData.Document.ExternalIdOrPath = Context.LoadString(docInfoBlock.ExternalIdOrPathAddress);
    CurrentData.MatchPosition = CurrentPostingsListCursor.CurrentKey - docInfoBlock.StartingOffset;
  }

  private void SetCurrentKey()
  {
    CurrentKey.UserType = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentKey.UserId = TrigramMatchesCursor.CurrentKey.UserId;
    CurrentKey.RepoType = TrigramMatchesCursor.CurrentKey.RepoType;
    CurrentKey.RepoId = TrigramMatchesCursor.CurrentKey.RepoId;
    CurrentKey.Offset = CurrentPostingsListCursor.CurrentKey;
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
      SetCurrentKey();
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
      SetCurrentKey();
      return true;
    }
  }

  public bool MoveUntilGreaterThanOrEqual(TrigramMatchPositionKey target)
  {
    uint nextRepoId = target.RepoId;

    if (CurrentKey.UserType == target.UserType &&
        CurrentKey.UserId == target.UserId &&
        CurrentKey.RepoType == target.RepoType &&
        CurrentKey.RepoId == target.RepoId)
    {
      if (CurrentPostingsListCursor.MoveUntilGreaterThanOrEqual(target.Offset))
      {
        SetCurrentDocument();
        SetCurrentKey();
        return true;
      }

      nextRepoId++;
    }

    var nextMatchKey = new TrigramMatchKey(target.UserType, target.UserId, target.RepoType, nextRepoId);
    if (!TrigramMatchesCursor.MoveUntilGreaterThanOrEqual(nextMatchKey)) return false;

    while (true)
    {
      SetCurrentUser();
      SetCurrentRepository();

      CurrentPostingsList = Context.DiskBlockManager.SortedVarIntListFactory.LoadExisting(TrigramMatchesCursor.CurrentData);
      if (CurrentPostingsList == null)
      {
        if (!TrigramMatchesCursor.MoveNext()) return false;
        continue;
      }

      CurrentPostingsListCursor = new DiskSortedVarIntListCursor(CurrentPostingsList);
      if (!CurrentPostingsListCursor.MoveNext())
      {
        if (!TrigramMatchesCursor.MoveNext()) return false;
        continue;
      }

      SetCurrentDocument();
      SetCurrentKey();
      return true;
    }
  }

  public void Reset()
  {
    TrigramMatchesTree = null;
    TrigramMatchesCursor = null;
    DocTreeByOffsetCursor = null;
    CurrentPostingsList = null;
    CurrentPostingsListCursor = null;
    CurrentKey.UserType = 0;
    CurrentKey.UserId = 0;
    CurrentKey.RepoType = 0;
    CurrentKey.RepoId = 0;
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
