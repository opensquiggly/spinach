## Summary of Blocks used in Spinach

<table>
  <tr>
    <th>Block Name</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>RepoInfoBlock</td>
    <td>
      Contains information about a repository, such as its name, description, and the path to its
      root directory.
    </td>
  </tr>
  <tr>
    <td>FileInfoBlock</td>
    <td>
      Contains information about a file, such as its name, path, and size.
    </td>
  </tr>
</table>

## Summary of Trees
<table>
  <tr>
    <th>Tree Name</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>RepoInfoPerRepoIdTree</td>
    <td>
      Stores metadata information about each repository, indexed by the repository id.
    </td>
  </tr>
  <tr>
    <td>FileInfoPerFileIdTree</td>
    <td>
      Stores metadata information about each file, indexed by the file id. Note that there
      is one FileInfoPerFileIdTree per repository. File ids are unique within their own
      repository, but they are not globally unique. The address of each repository's file
      tree is stored in the RepoInfoBlock for that repository.
    </td>
  </tr>
  <tr>
    <td>SliceAddressPerTrigramTree</td>
    <td>
      A "slice" is meant to represent storage for all the matches of a given trigram, across
      all repositories. Given a trigram (representing by an int code), this tree returns an 
      address that can be used to load a slice, which in concrete terms is an instance of 
      PostingsListAddressPerRepoIdTree.
    </td>
  </tr>
  <tr>
    <td>PostingsListAddressPerRepoIdTree</td>
    <td>
      Stores a tree of postings list addresses, one for each repository.
    </td>
  </tr>
</table>

# Summary of Caches
<table>
  <tr>
    <th>Cache Name</th>
    <th>Backing Structure(s)</th>
    <th>Description</th>
  </tr>
  <tr>
    <td>RepoInfoPerRepoIdCache</td>
    <td>RepoInfoPerRepoIdTree</td>
    <td>
      Stores metadata information about each repository, indexed by the repository id.
    </td>
  </tr>
  <tr>
    <td>FileInfoPerFileIdCache</td>
    <td>FileInfoPerFileIdTree</td>
    <td>
      Provides fast lookup of file metadata given a file id. Note that file ids are not
      globally unique, only unique within a repository, so the developer needs to make sure
      they are using the correct cache when looking up a file id.
    </td>
  <tr>
    <td>FileInfoPerRepoFileKeyCache</td>
    <td>
      RepoInfoPerRepoIdCache<br>
      FileInfoPerFileIdCache
    </td>
    <td>
      A flattened cache that stores file metadata for each file, indexed
      by the repository id / file id pairs.
    </td>
  </tr>
  <tr>
    <td>FileInfoPerFileIdTreePerRepoIdCache</td>
    <td>FileInfoPerFileIdTree</td>
    <td>
      Stores metadata information about each file, indexed by the file id. Note that there
      is one FileInfoPerFileIdCache per repository. File ids are unique within their own
      repository, but they are not globally unique. The address of each repository's file
      tree is stored in the RepoInfoBlock for that repository.
    </td>
  </tr>
  <tr>
    <td>PostingsListPerTrigramRepoKeyCache</td>
    <td>
      SliceAddressPerTrigramTree<br>
      PostingsListAddressPerRepoIdTree
    </td>
    <td>
      A flattened cache that stores postings lists for each trigram / repository id pair.
    </td>
  </tr>
  <tr>
</table>
