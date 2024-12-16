namespace Spinach.Enumerators;

using Misc;
using TrackingObjects;

public class FastTrigramEnumerator2 : IFastEnumerator<MatchWithRepoOffsetKey, MatchData>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerator2(string trigram, ITextSearchEnumeratorContext context, int adjustedOffset = 0)
  {
    if (trigram is not { Length: 3 })
    {
      throw new ArgumentOutOfRangeException("Trigrams should be 3 letters long");
    }

    TrigramKey = char.ToLower(trigram[0]) * 128 * 128 + char.ToLower(trigram[1]) * 128 + char.ToLower(trigram[2]);
    AdjustedOffset = adjustedOffset;
    Context = context;
    CurrentKey = new MatchWithRepoOffsetKey();
    CurrentData = new MatchData();
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

  public int AdjustedOffset { get; }

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

  public MatchData Current => CurrentData;

  public MatchData CurrentData { get; }

  public MatchWithRepoOffsetKey CurrentKey { get; }

  // public IUser CurrentUser { get; private set; }
  //
  // public IRepository CurrentRepository { get; private set; }
  //
  // public IDocument CurrentDocument { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private void SetCurrentUser()
  {
    // Console.WriteLine("Entering SetCurrentUser()");
    if (TrigramMatchesCursor == null)
    {
      // Console.WriteLine("SetCurrentUser: TrigramMatchesCursor is null, returning");
      return;
    }

    if (MatchWithRepoOffsetKey.IsSameUser(CurrentKey, TrigramMatchesCursor.CurrentKey))
    {
      // Console.WriteLine("SetCurrentUser: Same user as current, skipping update");
      return;
    }

    // Console.WriteLine($"SetCurrentUser: Updating user - Type: {TrigramMatchesCursor.CurrentKey.UserType}, Id: {TrigramMatchesCursor.CurrentKey.UserId}");
    CurrentData.User.Type = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentData.User.Id = TrigramMatchesCursor.CurrentKey.UserId;

    var userCompoundKey = new UserIdCompoundKeyBlock()
    {
      UserType = CurrentData.User.Type,
      UserId = CurrentData.User.Id
    };

    Context.UserCache.TryFind(userCompoundKey, out _, out _, out _, out IUser user);
    CurrentData.User = user;
  }

  private void SetCurrentRepository()
  {
    // Console.WriteLine("Entering SetCurrentRepository()");
    if (TrigramMatchesCursor == null)
    {
      // Console.WriteLine("SetCurrentRepository: TrigramMatchesCursor is null, returning");
      return;
    }

    if (MatchWithRepoOffsetKey.IsSameRepo(CurrentKey, TrigramMatchesCursor.CurrentKey))
    {
      // Console.WriteLine("SetCurrentRepository: Same repo as current, skipping update");
      return;
    }

    // Console.WriteLine($"SetCurrentRepository: Updating repo - Type: {TrigramMatchesCursor.CurrentKey.RepoType}, Id: {TrigramMatchesCursor.CurrentKey.RepoId}");
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

    Context.RepoCache.TryFind(repoCompoundKey, out _, out _, out _, out IRepository repo);
    CurrentData.Repository = repo;
  }

  private void SetCurrentDocument()
  {
    // Console.WriteLine("Entering SetCurrentDocument()");
    if (MatchWithRepoOffsetKey.IsSameRepo(CurrentKey, TrigramMatchesCursor.CurrentKey) &&
        CurrentKey.Offset == (long)CurrentPostingsListCursor.CurrentKey)
    {
      // Console.WriteLine("SetCurrentDocument: Same document as current, skipping update");
      return;
    }

    // Console.WriteLine($"SetCurrentDocument: Updating document - Offset: {CurrentPostingsListCursor.CurrentKey}");
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
    if (DocTreeByOffsetCursor.IsPastEnd)
    {
      // Console.WriteLine("SetCurrentDocument: DocTreeByOffsetCursor is past end, resetting to end");
      DocTreeByOffsetCursor.ResetToEnd();
      DocTreeByOffsetCursor.MovePrevious();
    }
    else if (DocTreeByOffsetCursor.CurrentKey.CompareTo(docOffsetKey) > 0)
    {
      // Console.WriteLine("SetCurrentDocument: Current key is greater than target, moving previous");
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

    bool docFound = Context.DocCache.TryFind(docIdCompoundKey, out _, out _, out _, out IDocument doc);
    // Console.WriteLine($"SetCurrentDocument: Document found: {docFound}");
    if (docFound)
    {
      CurrentData.Document = doc;
      CurrentData.MatchPosition = (long)CurrentPostingsListCursor.CurrentKey - (long)doc.StartingOffset - AdjustedOffset;
      // Console.WriteLine($"SetCurrentDocument: Updated match position to {CurrentData.MatchPosition}");
    }
  }

  private void SetCurrentKey()
  {
    // Console.WriteLine("Entering SetCurrentKey()");
    CurrentKey.UserType = TrigramMatchesCursor.CurrentKey.UserType;
    CurrentKey.UserId = TrigramMatchesCursor.CurrentKey.UserId;
    CurrentKey.RepoType = TrigramMatchesCursor.CurrentKey.RepoType;
    CurrentKey.RepoId = TrigramMatchesCursor.CurrentKey.RepoId;
    CurrentKey.Offset = (long)CurrentPostingsListCursor.CurrentKey;
    // Console.WriteLine($"SetCurrentKey: Updated key - UserType: {CurrentKey.UserType}, UserId: {CurrentKey.UserId}, RepoType: {CurrentKey.RepoType}, RepoId: {CurrentKey.RepoId}, Offset: {CurrentKey.Offset}");
  }

  private bool SkipDocument()
  {
    // Console.WriteLine("Entering SkipDocument()");
    if (!CurrentData!.Document.IsValid)
    {
      // Console.WriteLine("SkipDocument: Document is not valid");
      return true;
    }
    if (CurrentData.Document.CurrentLength > Context.Options.MaxDocSize)
    {
      // Console.WriteLine("SkipDocument: Document exceeds max size");
      return true;
    }
    if (CurrentData.Document.Status != DocStatus.Normal)
    {
      // Console.WriteLine("SkipDocument: Document status is not normal");
      return true;
    }

    // Console.WriteLine("SkipDocument: Document is valid and within limits");
    return false;
  }

  private bool MoveUntilGteInCurrentPostingsList(ulong targetOffset)
  {
    // Console.WriteLine($"Entering MoveUntilGteInCurrentPostingsList with target offset: {targetOffset}");
    while (true)
    {
      if (!CurrentPostingsListCursor.MoveUntilGreaterThanOrEqual(targetOffset))
      {
        // Console.WriteLine("MoveUntilGteInCurrentPostingsList: No more offsets in postings list");
        return false;
      }

      SetCurrentDocument();
      SetCurrentKey();

      if (!SkipDocument()) break;

      // Console.WriteLine("MoveUntilGteInCurrentPostingsList: Skipping document, updating target offset");
      targetOffset = CurrentData.Document.StartingOffset + (ulong)CurrentData.Document.CurrentLength;
    }

    // Console.WriteLine("MoveUntilGteInCurrentPostingsList: Successfully moved to valid document");
    return true;
  }

  private void PrintCurrent()
  {
    // Used for debugging
    Console.WriteLine($"User Type: {CurrentData.User.Type}");
    Console.WriteLine($"User Id: {CurrentData.User.Id}");
    Console.WriteLine($"Repo Type: {CurrentData.Repository.Type}");
    Console.WriteLine($"Repo Id: {CurrentData.Repository.Id}");
    Console.WriteLine($"Document Id: {CurrentData.Document.DocId}");
    Console.WriteLine($"Document Path: {CurrentData.Document.ExternalIdOrPath}");
    Console.WriteLine($"Starting Offset: {CurrentData.Document.StartingOffset}");
    Console.WriteLine($"Current Length: {CurrentData.Document.CurrentLength}");
    Console.WriteLine($"Postings List Key: {CurrentPostingsListCursor.CurrentKey}");
    Console.WriteLine($"Match Position: {CurrentData.MatchPosition}");
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    // Console.WriteLine("Entering MoveNext()");
    if (TrigramMatchesTree == null)
    {
      // Console.WriteLine("MoveNext: TrigramMatchesTree is null");
      return false;
    }
    if (TrigramMatchesCursor == null)
    {
      // Console.WriteLine("MoveNext: TrigramMatchesCursor is null");
      return false;
    }
    if (CurrentPostingsListCursor == null)
    {
      // Console.WriteLine("MoveNext: CurrentPostingsListCursor is null");
      return false;
    }

    if (CurrentPostingsListCursor.MoveNext())
    {
      // Console.WriteLine("MoveNext: Successfully moved to next posting");
      SetCurrentDocument();
      SetCurrentKey();

      return true;
    }

    while (true)
    {
      if (!TrigramMatchesCursor.MoveNext())
      {
        // Console.WriteLine("MoveNext: No more trigram matches");
        return false;
      }

      SetCurrentUser();
      SetCurrentRepository();

      CurrentPostingsList = Context.DiskBlockManager.SortedVarIntListFactory.LoadExisting(TrigramMatchesCursor.CurrentData);
      if (CurrentPostingsList == null)
      {
        // Console.WriteLine("MoveNext: Failed to load postings list");
        continue;
      }

      CurrentPostingsListCursor = new DiskSortedVarIntListCursor(CurrentPostingsList);
      if (!CurrentPostingsListCursor.MoveNext())
      {
        // Console.WriteLine("MoveNext: Empty postings list");
        continue;
      }

      SetCurrentDocument();
      SetCurrentKey();

      if (SkipDocument())
      {
        // Console.WriteLine("MoveNext: Skipping document, creating skip key");
        var skipToKey = new MatchWithRepoOffsetKey()
        {
          UserType = CurrentKey.UserType,
          UserId = CurrentKey.UserId,
          RepoType = CurrentKey.RepoType,
          RepoId = CurrentKey.RepoId,
          Offset = (long)(CurrentData.Document.StartingOffset + (ulong)CurrentData.Document.CurrentLength)
        };

        return MoveUntilGreaterThanOrEqual(skipToKey);
      }

      // Console.WriteLine("MoveNext: Found valid document");
      return true;
    }
  }

  public bool MoveUntilGreaterThanOrEqual(MatchWithRepoOffsetKey target)
  {
    // Console.WriteLine($"Entering MoveUntilGreaterThanOrEqual with target - UserType: {target.UserType}, UserId: {target.UserId}, RepoType: {target.RepoType}, RepoId: {target.RepoId}, Offset: {target.Offset}");
    uint nextRepoId = target.RepoId;

    if (MatchWithRepoOffsetKey.IsSameRepo(CurrentKey, target))
    {
      // Console.WriteLine("MoveUntilGreaterThanOrEqual: Same repo as current");
      if (MoveUntilGteInCurrentPostingsList((ulong)target.Offset)) return true;

      nextRepoId++;
    }
    else if (target.WithZeroOffsets() < CurrentKey.WithZeroOffsets())
    {
      // Console.WriteLine("MoveUntilGreaterThanOrEqual: Target is less than current key");
      nextRepoId = CurrentKey.RepoId;
    }

    var nextMatchKey = new TrigramMatchKey(target.UserType, target.UserId, target.RepoType, nextRepoId);
    if (!TrigramMatchesCursor.MoveUntilGreaterThanOrEqual(nextMatchKey))
    {
      // Console.WriteLine("MoveUntilGreaterThanOrEqual: No more matches");
      return false;
    }

    while (true)
    {
      SetCurrentUser();
      SetCurrentRepository();

      CurrentPostingsList = Context.DiskBlockManager.SortedVarIntListFactory.LoadExisting(TrigramMatchesCursor.CurrentData);
      if (CurrentPostingsList == null)
      {
        // Console.WriteLine("MoveUntilGreaterThanOrEqual: Failed to load postings list");
        if (!TrigramMatchesCursor.MoveNext()) return false;
        continue;
      }

      CurrentPostingsListCursor = new DiskSortedVarIntListCursor(CurrentPostingsList);
      if (!CurrentPostingsListCursor.MoveNext())
      {
        // Console.WriteLine("MoveUntilGreaterThanOrEqual: Empty postings list");
        if (!TrigramMatchesCursor.MoveNext()) return false;
        continue;
      }

      SetCurrentDocument();
      SetCurrentKey();

      if (SkipDocument())
      {
        // Console.WriteLine("MoveUntilGreaterThanOrEqual: Skipping document, creating skip key");
        var skipToKey = new MatchWithRepoOffsetKey()
        {
          UserType = CurrentKey.UserType,
          UserId = CurrentKey.UserId,
          RepoType = CurrentKey.RepoType,
          RepoId = CurrentKey.RepoId,
          Offset = (long)(CurrentData.Document.StartingOffset + (ulong)CurrentData.Document.CurrentLength - (ulong)AdjustedOffset)
        };

        return MoveUntilGreaterThanOrEqual(skipToKey);
      }

      // Console.WriteLine("MoveUntilGreaterThanOrEqual: Found valid document");
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
    CurrentKey.AdjustedOffset = AdjustedOffset;

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
