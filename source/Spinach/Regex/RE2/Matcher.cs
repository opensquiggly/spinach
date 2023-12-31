// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/syntax/compile.go

namespace Spinach.Regex;

/**
     * A stateful iterator that interprets a regex {@code Pattern} on a specific input. Its interface
     * mimics the JDK 1.4.2 {@code java.util.regex.Matcher}.
     *
     * <p>
     * Conceptually, a Matcher consists of four parts:
     * </p>
     * <ol>
     * <li>A compiled regular expression {@code Pattern}, set at construction and fixed for the lifetime
     * of the matcher.</li>
     *
     * <li>The remainder of the input string, set at construction or {@link #reset()} and advanced by
     * each match operation such as {@link #find}, {@link #matches} or {@link #lookingAt}.</li>
     *
     * <li>The current match information, accessible via {@link #start}, {@link #end}, and
     * {@link #group}, and updated by each match operation.</li>
     *
     * <li>The append position, used and advanced by {@link #appendReplacement} and {@link #appendTail}
     * if performing a search and replace from the input to an external {@code StringBuffer}.</li>
     *
     * </ol>
     *
     * <p>
     * See the <a href="package.html">package-level documentation</a> for an overview of how to use this
     * API.
     * </p>
     *
     * @author rsc@google.com (Russ Cox)
     */
public class Matcher
{
  // The pattern being matched.
  private Pattern _pattern;

  // The group indexes, in [start, end) pairs.  Zeroth pair is overall match.
  private int[] groups;

  // The number of submatches (groups) in the pattern.
  private int _groupCount;

  private string inputSequence;

  // The input length in UTF16 codes.
  private int _inputLength;

  // The append position: where the next append should start.
  private int appendPos;

  // Is there a current match?
  private bool hasMatch;

  // Have we found the submatches (groups) of the current match?
  // group[0], group[1] are set regardless.
  private bool hasGroups;

  // The anchor flag to use when repeating the match to find subgroups.
  private int anchorFlag;

  public Matcher(Pattern pattern)
  {
    if (pattern == null)
    {
      throw new NullReferenceException("pattern is null");
    }

    this._pattern = pattern;
    RE2 re2 = pattern.re2();
    _groupCount = re2.NumberOfCapturingGroups;
    groups = new int[2 + 2 * _groupCount];
  }

  /** Creates a new {@code Matcher} with the given pattern and input. */
  public Matcher(Pattern pattern, string input)
  {
    if (pattern == null)
    {
      throw new NullReferenceException("pattern is null");
    }

    this._pattern = pattern;
    RE2 re2 = pattern.re2();
    _groupCount = re2.NumberOfCapturingGroups;
    groups = new int[2 + 2 * _groupCount];

    reset(input);
  }

  /** Returns the {@code Pattern} associated with this {@code Matcher}. */
  public Pattern pattern() => _pattern;

  /**
         * Resets the {@code Matcher}, rewinding input and discarding any match information.
         *
         * @return the {@code Matcher} itself, for chained method calls
         */
  public Matcher reset()
  {
    _inputLength = inputSequence.Length;
    appendPos = 0;
    hasMatch = false;
    hasGroups = false;
    return this;
  }

  /**
         * Resets the {@code Matcher} and changes the input.
         *
         * @param input the new input string
         * @return the {@code Matcher} itself, for chained method calls
         */
  public Matcher reset(string input)
  {
    if (input == null)
    {
      throw new NullReferenceException("input is null");
    }

    inputSequence = input;
    reset();
    return this;
  }

  /**
         * Returns the start position of the most recent match.
         *
         * @throws IllegalStateException if there is no match
         */
  public int start() => start(0);

  /**
         * Returns the end position of the most recent match.
         *
         * @throws IllegalStateException if there is no match
         */
  public int end() => end(0);

  /**
         * Returns the start position of a subgroup of the most recent match.
         *
         * @param group the group index; 0 is the overall match
         * @throws IllegalStateException if there is no match
         * @throws IndexOutOfRangeException  if {@code group &lt; 0} or {@code group &gt; groupCount()}
         */
  public int start(int group)
  {
    loadGroup(group);
    return groups[2 * group];
  }

  /**
         * Returns the end position of a subgroup of the most recent match.
         *
         * @param group the group index; 0 is the overall match
         * @throws IllegalStateException if there is no match
         * @throws IndexOutOfRangeException  if {@code group &lt; 0} or {@code group &gt; groupCount()}
         */
  public int end(int group)
  {
    loadGroup(group);
    return groups[2 * group + 1];
  }

  /**
         * Returns the most recent match.
         *
         * @throws IllegalStateException if there is no match
         */
  public String group() => group(0);

  /**
         * Returns the subgroup of the most recent match.
         *
         * @throws IllegalStateException if there is no match
         * @throws IndexOutOfRangeException  if {@code group &lt; 0} or {@code group &gt; groupCount()}
         */
  public String group(int group)
  {
    int s = start(group);
    int e = end(group);
    if (s < 0 && e < 0)
    {
      // Means the subpattern didn't get matched at all.
      return null;
    }

    return substring(s, e);
  }

  /**
         * Returns the number of subgroups in this pattern.
         *
         * @return the number of subgroups; the overall match (group 0) does not count
         */
  public int groupCount() => _groupCount;

  /** Helper: finds subgroup information if needed for group. */
  private void loadGroup(int group)
  {
    if (group < 0 || group > _groupCount)
    {
      throw new IndexOutOfRangeException("Group index out of bounds: " + group);
    }

    if (!hasMatch)
    {
      throw new IllegalStateException("perhaps no match attempted");
    }

    if (group == 0 || hasGroups)
    {
      return;
    }

    // Include the character after the matched text (if there is one).
    // This is necessary in the case of inputSequence abc and pattern
    // (a)(b$)?(b)? . If we do pass in the trailing c,
    // the groups evaluate to new String[] {"ab", "a", null, "b" }
    // If we don't, they evaluate to new String[] {"ab", "a", "b", null}
    // We know it won't affect the total matched because the previous call
    // to match included the extra character, and it was not matched then.
    int end = groups[1] + 1;
    if (end > _inputLength)
    {
      end = _inputLength;
    }

    bool ok =
      _pattern.re2().Match(inputSequence, groups[0], end, anchorFlag, groups, 1 + _groupCount);
    // Must match - hasMatch says that the last call with these
    // parameters worked just fine.
    if (!ok)
    {
      throw new IllegalStateException("inconsistency in matching group data");
    }

    hasGroups = true;
  }

  /**
         * Matches the entire input against the pattern (anchored start and end). If there is a match,
         * {@code matches} sets the match state to describe it.
         *
         * @return true if the entire input matches the pattern
         */
  public bool matches() => genMatch(0, RE2.ANCHOR_BOTH);

  /**
         * Matches the beginning of input against the pattern (anchored start). If there is a match,
         * {@code lookingAt} sets the match state to describe it.
         *
         * @return true if the beginning of the input matches the pattern
         */
  public bool lookingAt() => genMatch(0, RE2.ANCHOR_START);

  /**
         * Matches the input against the pattern (unanchored). The search begins at the end of the last
         * match, or else the beginning of the input. If there is a match, {@code find} sets the match
         * state to describe it.
         *
         * @return true if it finds a match
         */
  public bool find()
  {
    int start = 0;
    if (hasMatch)
    {
      start = groups[1];
      if (groups[0] == groups[1])
      {
        // empty match - nudge forward
        start++;
      }
    }

    return genMatch(start, RE2.UNANCHORED);
  }

  /**
         * Matches the input against the pattern (unanchored), starting at a specified position. If there
         * is a match, {@code find} sets the match state to describe it.
         *
         * @param start the input position where the search begins
         * @return true if it finds a match
         * @throws IndexOutOfRangeException  if start is not a valid input position
         */
  public bool find(int start)
  {
    if (start < 0 || start > _inputLength)
    {
      throw new IndexOutOfRangeException("start index out of bounds: " + start);
    }

    reset();
    return genMatch(start, 0);
  }

  /** Helper: does match starting at start, with RE2 anchor flag. */
  private bool genMatch(int startByte, int anchor)
  {
    // TODO(rsc): Is matches/lookingAt supposed to reset the append or input positions?
    // From the JDK docs, looks like no.
    bool ok = _pattern.re2().Match(inputSequence, startByte, _inputLength, anchor, groups, 1);
    if (!ok)
    {
      return false;
    }

    hasMatch = true;
    hasGroups = false;
    anchorFlag = anchor;

    return true;
  }

  /** Helper: return substring for [start, end). */
  public string substring(int start, int end) =>
    // This is fast for both StringBuilder and String.
    inputSequence.Substring(start, end - start);

  /** Helper for Pattern: return input length. */
  public int inputLength() => _inputLength;

  /**
         * Quotes '\' and '$' in {@code s}, so that the returned string could be used in
         * {@link #appendReplacement} as a literal replacement of {@code s}.
         *
         * @param s the string to be quoted
         * @return the quoted string
         */
  public static string quoteReplacement(string s)
  {
    if (s.IndexOf('\\') < 0 && s.IndexOf('$') < 0)
    {
      return s;
    }

    var sb = new StringBuilder();
    for (int i = 0; i < s.Length; ++i)
    {
      char c = s[i];
      if (c == '\\' || c == '$')
      {
        sb.Append('\\');
      }

      sb.Append(c);
    }

    return sb.ToString();
  }

  /**
         * Appends to {@code sb} two strings: the text from the append position up to the beginning of the
         * most recent match, and then the replacement with submatch groups substituted for references of
         * the form {@code $n}, where {@code n} is the group number in decimal. It advances the append
         * position to where the most recent match ended.
         *
         * <p>
         * To embed a literal {@code $}, use \$ (actually {@code "\\$"} with string escapes). The escape
         * is only necessary when {@code $} is followed by a digit, but it is always allowed. Only
         * {@code $} and {@code \} need escaping, but any character can be escaped.
         * </p>
         *
         * <p>
         * The group number {@code n} in {@code $n} is always at least one digit and expands to use more
         * digits as long as the resulting number is a valid group number for this pattern. To cut it off
         * earlier, escape the first digit that should not be used.
         * </p>
         *
         * @param sb the {@link StringBuilder} to append to
         * @param replacement the replacement string
         * @return the {@code Matcher} itself, for chained method calls
         * @throws IllegalStateException if there was no most recent match
         * @throws IndexOutOfRangeException  if replacement refers to an invalid group
         */
  public Matcher appendReplacement(StringBuilder sb, String replacement)
  {
    int s = start();
    int e = end();
    if (appendPos < s)
    {
      sb.Append(substring(appendPos, s));
    }

    appendPos = e;
    appendReplacementInternal(sb, replacement);
    return this;
  }

  private void appendReplacementInternal(StringBuilder sb, String replacement)
  {
    int last = 0;
    int i = 0;
    int m = replacement.Length;
    for (; i < m - 1; i++)
    {
      if (replacement[i] == '\\')
      {
        if (last < i)
        {
          sb.Append(replacement.Substring(last, i));
        }

        i++;
        last = i;
        continue;
      }

      if (replacement[i] == '$')
      {
        int c = replacement[i + 1];
        if ('0' <= c && c <= '9')
        {
          int n = c - '0';
          if (last < i)
          {
            sb.Append(replacement.Substring(last, i));
          }

          for (i += 2; i < m; i++)
          {
            c = replacement[i];
            if (c < '0' || c > '9' || n * 10 + c - '0' > _groupCount)
            {
              break;
            }

            n = n * 10 + c - '0';
          }

          if (n > _groupCount)
          {
            throw new Exception("n > number of groups: " + n);
          }

          String g = group(n);
          if (g != null)
          {
            sb.Append(g);
          }

          last = i;
          i--;
          continue;
        }
      }
    }

    if (last < m)
    {
      sb.Append(replacement.Substring(last, m));
    }
  }

  /**
         * Appends to {@code sb} the substring of the input from the append position to the end of the
         * input.
         *
         * @param sb the {@link StringBuilder} to append to
         * @return the argument {@code sb}, for method chaining
         */
  public StringBuilder appendTail(StringBuilder sb)
  {
    sb.Append(substring(appendPos, inputLength()));
    return sb;
  }

  /**
         * Returns the input with all matches replaced by {@code replacement}, interpreted as for
         * {@code appendReplacement}.
         *
         * @param replacement the replacement string
         * @return the input string with the matches replaced
         * @throws IndexOutOfRangeException  if replacement refers to an invalid group
         */
  public String replaceAll(String replacement) => replace(replacement, true);

  /**
         * Returns the input with the first match replaced by {@code replacement}, interpreted as for
         * {@code appendReplacement}.
         *
         * @param replacement the replacement string
         * @return the input string with the first match replaced
         * @throws IndexOutOfRangeException  if replacement refers to an invalid group
         */
  public String replaceFirst(String replacement) => replace(replacement, false);

  /** Helper: replaceAll/replaceFirst hybrid. */
  private String replace(String replacement, bool all)
  {
    reset();
    var sb = new System.Text.StringBuilder();
    while (find())
    {
      appendReplacement(sb, replacement);
      if (!all)
      {
        break;
      }
    }

    appendTail(sb);
    return sb.ToString();
  }
}
