# Overview

Enumerators are an important aspect of the Spinach architecture. They allow the developer to iterate over
trigram, literal, and regular expression matches in a Spinach index file. The Spinach enumerators abstract
away the details of finding matches and allow the developer to easily loop over matching files.

# IFastEnumerator<TKey, TData>

The IFastEnumerator interface is provided by Eugene and used heavily in Spinach. It is designed to iterate
over any data structure containing a key and a value, and is most specifically designed for iterating over
b-trees.

In the case of b-trees or any other tree-like data structure indexed by a key, the enumerator returns results
in sorted order. Spinach takes advantage of this property throughout the design. Namely, when multi-repository
matches are returned, the IFastEnumerator is guaranteed to return the results already sorted by the repository
id, which makes it easy to group the results by repository without need to sort the results after the fact.

IFastEnumerator implements the .NET's built-in IEnumerator interface. The "Fast" in IFastEnumerator refers to 
to an extra method, MoveUntilGreaterThanOrEqual(), which is not part of the built-in IEnumerator interface.

MoveUntilGreaterThanOrEqual() can be used to quickly skip over large swaths of index trees that we've determined
cannot possibly contain any matches. This comes into play most notable when performing intersections of two
other IFastEnumerators, and is an important aspect of what makes Spinach able to perform fast index matches.

For example, imagine that we are iterating over an intersection of two enumerators, one enumerator matching
all the string literals "quick" and another matching all the string literals "brown". Suppose
the first enumerator has landed on a hit at (RepoId = 123, Position 4567) and the second enumerator has landed
on a hit at (RepoId = 234, Position = 5678).

Since we know the candidate document must contain both "quick" and "brown" we can use this information to tell
the first enumerator to "MoveUntilGreaterThanOrEqual()" of (RepoId = 234, Position = 5678). In this way the two
enumerators can ping-pong back and forth between each other, skipping over large chunks of the index that we
know will not contain matches.

# FastTrigramEnumerator

The most fundamental enumerator provided by Spinach is FastTrigramEnumerator, which is used to iterate over
all the trigram matches in a Spinach index file. Recall that trigrams are 3-byte sequences of characters.
A FastTrigramEnumerator iterating over the trigram "qui" will return one result each time "qui" appears in
the repository.

The key for the enumerator is an instance of TrigramRepoIdPosition, which contains two properties, the RepoId
and the Position (offset) with the repository where the trigram was found.

Note that the Position is the offset within the repository, assuming that all files in the repository have
been sequentially concatenated together in the order of their file ids.

Given a position within a repository, we can do a little extra work to determine the associated file id and
offset of the match. To make these lookups easier, Spinach also provides FastTrigramFileEnumerator, discussed
next.

# FastTrigramFileEnumerator

A FastTrigramFileEnumerator is similar to FastTrigramEnumerator, except that it automatically determines which
file id that match occurs in, and gives back a key of TrigramRepoFilePositionInfo, which contains the RepoId,
the FileId, and the Position (offset) of the match within the file.

FastTrigramFileEnumerator is easier to work with than the more basic FastTrigramEnumerator, but is slightly 
slower because of the extra work it does to look up the associated file and calculate the file offset. To make
these lookups as fast as possible, Spinach uses LRU-cached data structures.

# FastLiteralEnumerator

Whereas the FastTrigramEnumerator can only iterate of three-byte sequences, the FastLiteralEnumerator can
iterate over any arbitrary string literal. Under the covers, FastLiteralEnumerator works by performing an
intersection of two trigrams.

For example, suppose we want to find all the matches for the string literal "quickly". The leading trigram
of "quickly" is "qui" and the trailing trigram is "kly". Since we know that any file containing "quickly"
must in turn contain both the trigrams, we can intersect the two underlying enumerators, and then perform
one last check to ensure that the full literal "quickly" is indeed present at the matched offset.

As with FastTrigramEnumerator, FastLiteralEnumerator returns a key of TrigramRepoPositionInfo.

# FastLiteralFileEnumerator

FastLiteralFileEnumerator is to FastLiteralEnumerator what FastTrigramFileEnumerator is to FastTrigramEnumerator.
It looks up the associated file id using the repository offset and returns a key of TrigramRepoFilePositionInfo.

# FastRegexEnumerator

TODO. We don't currently have one of these. Do we need one, or can we just provide FastRegexFileEnumerator?

# FastRegexFileEnumerator

TODO
