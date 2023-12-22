# Overview

This document discusses the data structures of Spinach and how it uses the Eugene persistence
engine to index files for fast regex searching.

# Naming Conventions

## Blocks
Blocks are fixed size value types defined as C# structs that can be stored in files managed
by Eugene. Blocks must only contain scalar types. If you need to store strings in a block,
you'll need to create code that writes the string to a Eugene DiskImmutableString, get the address
of the string, and store the address in the block. Likewise, when reading back the data, you'll
need to create code that rehydrates the string from the address.

Blocks end with the word "Block" and are located in the "Blocks" folder underneath the "source"
folder.

When we use the block in a variable name, we generally drop the "Block" suffix, as it is redundant.
We generally assume that, since everything with Eugene deals with fixed size blocks, that anything
we're passing into Eugene collections is a block, and we don't need to be constantly reminded of it.

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

## Trees
We need several b-tree structures, using the DiskBTree<> base class provided by Eugene. For
clarity and convenience, we prefer to create named, derived classes for each b-tree, rather
than declaring types directly using the DiskBTree<> base class. This makes the code more readable,
as the reader of the code can infer the intention of the tree based on its name. Creating named
classes also gives us a place to add any additional tree-specific methods if needed.

B-trees that are derived from DiskBTree<> are placed in the "Trees" folder underneath the "source"
folder.

Because we use various trees of various key and data types, we want to follow a naming convention
that makes it easy to identify the purpose and types of each tree.

The first part of the name will identify the purpose and/or payload data type of the tree. This
is followed by the word "Per" followed by the key type, and finally ends with the word "Tree".

For example, a tree indexed by the repository id, containing a RepoInfoBlock holding information
about the repository, would be named:

```
RepoInfoPerRepoIdTree
--------   ------
   ^    ---  ^   ----
   |     ^   |    ^
   |     |   |    |
   |     |   |    +--- The "Tree" suffix tells us the class is derived from DiskBTree<>
   |     |   |
   |     |   +--- Identifies the key used to index the tree
   |     |
   |     +--- The word "Per" separates the data type from the key type
   |
   +--- Tells us the payload data type of the tree
```

As noted in the previous section, when we refer to a RepoInfo we really mean to say a 
RepoInfoBlock, but by convention we drop the "Block" suffix to keep the data structure names
shorter.

# Dealing with Long, Nested Names

The above naming conventions work well for simple cases, when the data type is a scalar value.
In some cases, however, Spinach uses a series of nested types, which would create very long names
if we adhered strictly to the rules above. To deal with this, we allow a secondary placeholder
name to be used as a shortened name representing a larger data structure.

Let's look at how this plays out in the code.

For each trigram, we need a b-tree that stores the postings lists *for each repository*. This
b-tree that is referenced by the first trigram, is indexed by the repository id, and gives us
back an *address* of where we can load the postings list for that repository.

What should we name these trees and caches? Let's start at the bottom and work our way up.

For each trigram/repository id pair, we need a postings list. We create the postings list, and
put its *address* into the b-tree that corresponds to the repository id for the specific trigram.

Following the conventions outlined above, we name this tree as follows:

```
PostingsListAddressPerRepoIdTree
------------       ---      ----
    ^       ------- ^ ------ ^
    |          ^    |   ^    |
    |          |    |   |    +--- As per usual, the "Tree" suffix tells us the class is derived from DiskBTree<>
    |          |    |   |
    |          |    |   +--- We have one postings list for each repository
    |          |    |
    |          |    +--- As per usual the word "Per" separates the key type from the data type
    |          |
    |          +--- But the tree doesn't actually store the postings list directly,
    |               it stores the address of where the postings list is
    |
    +--- The payload should ultimately result in a postings list
```

It's a little bit long, but it's not too bad, and the name allows the developer to quickly decifer
the underlying intent. The name tells us "we have a tree, which given a repository id, will give
us back a postings list address, which we can then use to load the underlying postings list."

So far, so good. We are sticking with our naming conventions. But now we need to realize that we
need to store one of these things, one of these "PostingsListAddressPerRepoIdTrees", for each
trigram. If we continued following our naming conventions, and recognizing that we actually need
to store the address to the above tree, not the tree itself, we would need another b-tree named:

```
PostingsListAddressPerTrigramTreeAddressPerTrigramTree
```

Whew!! That's a mouthful. And it's not very clear what it means. So we need to come up with a
shorter name that is still clear but easier to pronounce and faster to type.

We could debate what might be a good name for this, but for lack of a perfect name sent down by
God himself, let's call this a "Slice".

We're adopting a new convention where:

```
Slice = PostingsListAddressPerRepoIdTree
```

So now we can name the tree that stores the slice (and remember, what we really need to store is the
slice's *address*, not the slice itself) as follows:

```
SliceAddressPerTrigramTree
```

Luckily, this is as deep as our Spinach data structures go. We don't need to create any further
extracted names.

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


## Caches
To reduce disk access, most of the time we do not want to read and write data directly to the
underlying b-trees. Caches are LRU caches meant to ensure fast access to the data in the b-trees,
while at the same time limiting the maximum amount of memory that is used, allowing the user to
use a Spinach-consuming application on machines with lower memory requirements without worrying
about getting "out of memory" errors.

Spinach provides a base class named "LruCache<>", identifying the key and data payload type that
is being cached. As with the b-trees, we create named caches derived from LruCache<>, helping the
reader of the code identify the purpose and types of each cache.

Classes derived from LruCache<> are placed in the "Caching" folder underneath the "source" folder.

The naming convention for caches is similar to that of the b-trees. The first part of the name
identifies the purpose and/or payload data type of the cache. This is followed by the word "Per"
followed by the key type, and finally ends with the word "Cache".

For example, a cache indexed by the repository id, containing a RepoInfoBlock holding information
about the repository, would be named:

```
RepoInfoPerRepoIdCache
--------   ------
   ^    ---  ^   ----
   |     ^   |    ^
   |     |   |    |
   |     |   |    +--- The "Cache" suffix tells us the class is derived from LruCache<>
   |     |   |         Note that a "Cache" is generally presumed to be backed by a similarly
   |     |   |         named "Tree", but we sometimes combine types of nested trees to make
   |     |   |         a single flattened cache backed up by two or more trees or caches.
   |     |   |
   |     |   +--- Identifies the key used to index the cache
   |     |
   |     +--- The word "Per" separates the data type from the key type
   |
   +--- Tells us the payload data type of the cache
```

# Caching Trees and Other Caches

Caches need not only cache block data structure. They can also cache trees and even other caches.

Caching of trees is useful because it allows us to not have to rehydrate the tree data structure
from disk each time we need to use it. This will tend to create long names for data types unless
an alias is used, as discussed above.

When a tree is cached, it should follow the naming convention as discussed previous. The general
form of a tree cache is:

### Naming Template:

```
<CoreDataType>Per<TreeKeyType>TreePer<CacheKeyType>Cache
```

### Example:

```
FileInfoPerFileIdTreePerRepoIdCache
```

Notice that the "Per" separator will appear in the name more than once, which is unfortunate. For
this reason, aliased names are allowed and suggested to be used when possible and when an good alias
name can be used for the underlying tree. Unfortunately, it is not always easy to think of a good
alias name.

Reading of the type name should be done from right to left. In the above example, we have a cache,
indexed by the repository id, of trees which are indexed by file id and contain FileInfoBlocks.

Likewise, caches themselves can also be cached. As with cached trees, this is done when there is more 
than one of the same type of cache. Consider, for example, the FileInfoPerFileIdTree. There is one of 
these trees per repository. We could either cache the tree itself per repository, or if we choose to 
create a cache for the tree, we could cache the corresponding cache per repository.

Consideration should be given as to whether or not a tree cache is necessary. Following the above
example, if we have a FileInfoPerFileIdTree, then we might want to create a corresponding,
FileInfoPerFileIdCache, which would give us faster lookup times to find a FileInfoBlock given a 
file id. But if we are intending the cache the FileInfoBlock in another cache somewhere else, then we 
might not need to utilize a FileInfoPerFileIdCache.

This last point is particularly relevant if we are using flattened caches, which we'll discuss in
the next section.

# Flattened Caches

You might think we'd want a cache to go along with the tree, and if we did, we'd naturally want
to name it:

```
PostingsListAddressPerRepoIdCache
```

But we're not going to do that because what we want to do is flatten this two-tiered data structure
and create a single postings list cache that is keyed per trigram/repository id pair.

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
