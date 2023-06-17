# Project Status
THIS PROJECT IS NEW AND NOT YET READY TO BE USED.

# About Spinach
Spinach is a text/code search indexing library for .NET projects.
It is inspired by the open source code search tool Zoekt, and is
optimized for performing high-speed regular expression based searching
across large code repositories.

Being delivered as a library rather than an end-user product means
that you have the flexibility to embed code searching capabilities
into your own projects. If you're looking for an actual code search
engine that you can install and run, check out our other project,
Popeye.

You might think of Spinach as being a little bit like Lucene.NET,
except it is a much smaller, lighter-weight solution that is specialized
for performing regular expression searches. It aims to provide ultra
high speed code searching on par with Zoekt, in a package that is
easily consumable by .NET developers.

Why might you need a regular expression indexing / code search library 
in your project? We can think of lots of interesting projects that might
be interested in these capabilities.

* Internal Developer Portals / Platforms - With the rise of platform engineering,
  everyone is realizing that high quality developer portals can help
  improve developer happiness and productivity. Tools like these are
  highly diverse in functionality, which leads to a "build vs. buy"
  decision. For those organizations that don't have time to build their
  own portal, we think OpenSquiggly can be a great solution for them. But
  for other companies that want to build their own portal for whatever reasons,
  they might like to incorporate code searching into their portals.
* Text Editors / IDEs - Anyone building text editors or IDEs, either 
  client-side or cloud-based, may be interested. Tools like these usually
  have regex search built it. By indexing the source files, regex searches
  can be run much faster.
* Software Cataloging - Software catalogs are a subset of developer portals.
  Any time you are ingesting resources from around the organization with the
  hopes of cataloging them, it makes sense to index the data it for fast searching. 
* Other Developer Productivity Tools - There are undoubtedly countless other
  developer-oriented productivity tools that we haven't even thought of that
  could benefit greatly from integrated code searching. 
* Non-Developer Usages - Although we're primarily thinking of Spinach for
  building code search tools for developers, there's no reason why other
  types of users might not also find fast regex searching useful.  

# Sister Projects
Spinach is one part of a series of projects designed to deliver high
speed code searching capabilities.

* Eugene  - Eugene is a Nuget package that provides general purpose 
            persistent data structures
* Spinach - Spinach, this package, is a Nuget packages that builds on top 
            of Eugene and  provides an API and indexing file format for 
            performing regular expression text searching
* Popeye  - Popeye is an installable code searching engine that can be
            installed in various ways, including Docker, Kubernetes, etc.
            Essentially, Popeye aims to be a C# version of Zoekt that uses
            similar approaches to indexing the text repositories as does
            Zoekt. Popeye is not a direct, line-for-line port of Zoekt,
            but is based on similar ideas.

Popeye uses the Spinach package, which in turn uses Eugene.

# Motivation
To understand the motivation for Spinach, one should start by understanding
what Zoekt is.

Zoekt is an open source code searching engine written in Go. Being written in Go,
it wasn't overly useful for our projects here at OpenSquiggly, being that
our codebase is written in C#. We wanted a C# version of Zoekt, and so we
went about reading the Zoekt codebase to consider how we might access it
from C# or perhaps port it to C# line-by-line.

Unfortunately, due to the many philosophical differences between Go and C#,
directly porting a Go project to C# seemed quite difficult.

We thought about using Zoekt in a separate container and accessing it using
its REST API, but that just seemed like a gigantic hassle. We really wanted
a pure C# solution that could be tightly and properly integrated with OpenSquiggly.

Next, we thought about whether we should port our entire product to Go, but
we decided not to do that.

Finally, we wondered if we could write a new C# project from scratch that would
follow the same ideas as Zoekt.

Let's take the time to understand at a very high level what Zoekt does. Suppose
we are running a regular expression search for:

```
quickly.*browning.*foxhound
```

Admittedly this is a bit of a contrived example, but it's useful to illustrate
the point.

First, Zoekt extracts the three known literals from the string, getting:

* quickly
* browning
* foxhound

and then goes about finding all the files in the corpus that contain these three
literals. Once it finds a file with all three of these string literals, it runs
a regex search over the file to see if it matches the full regex.

Okay, but how does it quickly find those matching files? Aha! Now we're getting to the
heart of the matter with this problem.

In a nutshell, Zoekt builds up an index of trigrams that it can quickly read off
of disk to search for string literals using their trigrams. Zoekt doesn't document
their file format in very much detail - their design document describes it in
broad brush strokes, but the details are sketchy.

What we needed was a way to do the same thing in C#, and it the process we thought
it would be nice if we abstracted away the reusable components into libraries that
can be embedded into projects using Nuget. Thus Eugene and Spianch were born.

# More About How Zoekt Works
Zoekt looks for literals using trigrams. Trigrams are sequences of three letters.

* The trigrams for "quickly" are: qui, uic, ick, ckl, kly
* The trigrams for "browning" are: bro, row, own, wni, nin, ing
* The trigrams for "foxhound" are: fox, oxh, xho, hou, oun, und

Suppose we're looking for all documents that contain the string literal "quickly".

First we get a list of all the documents that contain the leading trigram, "qui".
Then we get another list of all the documents containing the trailing trigram, "kly" 
and see if the trigram "kly" exists at position qui + 4. Then we perform the interesection
of these two lists. Finally, we check the document to see if it truly contains
the full literal "quickly" at position qui.

In actuality we don't have to search for specifically leading and trailing trigrams, we 
can search for any two trigrams of our choosing within the literal. As long as we
have two lists of trigrams, we can intersect the list and hopefully wind up with a 
relatively small list of matching documents. This observation allows for some 
optimizations - we can search for the least frequently occurring trigrams to minimize 
the number of documents we need to search.

If we carefully arrange the documents so that they always come back ordered by
sequential file ids, then intersecting the two lists can be very fast because we can skip
over large numbers of unmatched documents as we perform the intersection.

This is why we need a custom file format that we have fine grained control over.
We want to iterate over the indexes in very specific ways, with some strategically
applied optimizations, to return results back to the user very quickly.

So that's what Zoekt does. It builds up these indexes and then does all the other
necessary work to use them to look up literals and run regex searches on candidate
documents.

The problem comes when we start thinking about how big these indexes might be.
Suppose we are trying to index 1,000,000 repositories on GitHub. Disk space is cheap
these days (well, not as cheap in the cloud, but still pretty cheap), the real problem
is how long it takes to look up a trigram in a huge index, and how many disk accesses
will be needed. Zoekt is aiming to provide lightning fast code searching that can return
thousands of results in millisecond timeframes, so disk access times become significant
on those scales.

In order to reduce disk accesses, Zoekt performs some vaguely documented magic. The
documentation is a little fuzzy on whether it does or does not cache the indexes in
memory, and whether it does or does not use a lot of memory. According to some documents
released by Zoekt's current maintainer, memory usage in Zoekt is potentially problematic.
If the Zoekt instance is not given enough memory, it may produce out-of-memory errors.

Zoekt also imposes some annoying limitations to limit its memory usage. It breaks up the
index into shards, with one repo per shard, and the sets the maximum size of a shard to 1GB.

In the world of cloud providers, as of this writing in the year 2023, memory is still
quite expensive and adds up quickly. If you need a VM with 4GB of memory, Azure or AWS
will give that to you for around $70/month or so. But if you want a 32GB VM, that might
run you $350/month.

So to run Zoekt, you have to make some calculations and some tradeoffs. Do you want to
overpay by spinning up a VM with more memory than you'll probably need, or do you want
to save money and run the risk of running out of memory? Running out of memory means
that someone is probably waking up a 3am to go fix the problem, and no one wants to do
that. We all want to sleep easy at night.

That's the problem we're trying to address with Eugene, Spinach, and Popeye.

We wanted a way to build these trigram indexes with clean, flexible, easy to modify C#
code, and also have a way to put an LRU cache in front of the data structures so that
the amount of memory used can be controlled more carefully. The goal is to make the
indexing run very fast even on small memory VM instances, and to never produce out of
memory errors.

# What about ElasticSearch or Lucene.NET?
An astute reader might wonder if what we just described doesn't sound a whole lot like
what ElasticSearch and Lucene.NET already do. Couldn't we use one of those to accomplish
what we're describing?

Yes, in theory, ElasticSearch and Lucene.NET do similar things, and should, at least in
theory, work for what we need. In practice though, it just doesn't work well. Those
solutions are too heavy-weight and are not optimized enough to do the kind of high volume,
large repository regex code searching that we desire.

In fact, our initial implementation for OpenSquiggly's code search was based on using
ElasticSearch to build trigram indexes. It did work, but the performance was lackluster,
required a lot of memory, and it was clear that the solution just wasn't going to scale
as we needed it to. When it comes to code searching, customers are looking to index thousands, 
or perhaps even hundreds of thousands of code repositories. A more general purpose solution 
like ElasticSearch just can't cope with that kind of scale.

# Where Did the Names Come From?
Zoekt is a Dutch word that means "seek".

The creator of Zoekt used the following tag line in his documentation:

```
"Zoekt, en gij zult spinazie eten" - Jan Eertink

("seek, and ye shall eat spinach" - My primary school teacher)
```

Here in America, everyone knows that Popeye the Sailor Man gets strong by eating spinach,
and so our names are based on this theme. Eugene is named after the character Eugene the Jeep
in the comic series.

We also like that the name Popeye bids a friendly salute to the now discontinued Atlassian
code search tool named FishEye. We always wondered why Atlassian didn't keep investing in
FishEye; perhaps it was just too far ahead of its time. In today's environment, with a
resurgence of interest in internal developer portals and platforms, and the strong need to
find ways to cope with the ever increasing complexity of software projects, we think good code
searching engines might be poised for a comeback.
