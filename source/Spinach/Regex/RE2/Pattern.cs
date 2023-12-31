// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/syntax/compile.go

namespace Spinach.Regex;

/**
     * A compiled representation of an RE2 regular expression, mimicking the
     * {@code java.util.regex.Pattern} API.
     *
     * <p>
     * The matching functions take {@code String} arguments instead of the more general Java
     * {@code CharSequence} since the latter doesn't provide UTF-16 decoding.
     * </p>
     *
     * <p>
     * See the <a href='package.html'>package-level documentation</a> for an overview of how to use this
     * API.
     * </p>
     *
     * @author rsc@google.com (Russ Cox)
     */
public class Pattern
{
  /** Flag: case insensitive matching. */
  public static int CASE_INSENSITIVE = 1;

  /** Flag: dot ({@code .}) matches all characters, including newline. */
  public static int DOTALL = 2;

  /**
         * Flag: multiline matching: {@code ^} and {@code $} match at beginning and end of line, not just
         * beginning and end of input.
         */
  public static int MULTILINE = 4;

  /**
         * Flag: Unicode groups (e.g. {@code \p\ Greek\} ) will be syntax errors.
         */
  public static int DISABLE_UNICODE_GROUPS = 8;

  // The pattern string at construction time.
  private string _pattern;

  // The flags at construction time.
  private int _flags;

  // The compiled RE2 regexp.
  private RE2 _re2;

  // This is visible for testing.
  Pattern(String pattern, int flags, RE2 re2)
  {
    if (pattern == null)
    {
      throw new NullReferenceException("pattern is null");
    }

    if (re2 == null)
    {
      throw new NullReferenceException("re2 is null");
    }

    this._pattern = pattern;
    this._flags = flags;
    this._re2 = re2;
  }

  /**
         * Releases memory used by internal caches associated with this pattern. Does not change the
         * observable behaviour. Useful for tests that detect memory leaks via allocation tracking.
         */
  public void reset() => _re2.Reset();

  /**
         * Returns the flags used in the constructor.
         */
  public int flags() => _flags;

  /**
         * Returns the pattern used in the constructor.
         */
  public string pattern() => _pattern;

  public RE2 re2() => _re2;

  /**
         * Creates and returns a new {@code Pattern} corresponding to compiling {@code regex} with the
         * default flags (0).
         *
         * @param regex the regular expression
         * @throws PatternSyntaxException if the pattern is malformed
         */
  public static Pattern compile(string regex) => compile(regex, regex, 0);

  /**
         * Creates and returns a new {@code Pattern} corresponding to compiling {@code regex} with the
         * default flags (0).
         *
         * @param regex the regular expression
         * @param flags bitwise OR of the flag constants {@code CASE_INSENSITIVE}, {@code DOTALL}, and
         * {@code MULTILINE}
         * @throws PatternSyntaxException if the regular expression is malformed
         * @throws ArgumentException if an unknown flag is given
         */
  public static Pattern compile(string regex, int flags)
  {
    String flregex = regex;
    if ((flags & CASE_INSENSITIVE) != 0)
    {
      flregex = "(?i)" + flregex;
    }

    if ((flags & DOTALL) != 0)
    {
      flregex = "(?s)" + flregex;
    }

    if ((flags & MULTILINE) != 0)
    {
      flregex = "(?m)" + flregex;
    }

    if ((flags & ~(MULTILINE | DOTALL | CASE_INSENSITIVE | DISABLE_UNICODE_GROUPS)) != 0)
    {

      throw new ArgumentException(
        "Flags should only be a combination "
        + "of MULTILINE, DOTALL, CASE_INSENSITIVE, DISABLE_UNICODE_GROUPS");
    }

    return compile(flregex, regex, flags);
  }

  /**
         * Helper: create new Pattern with given regex and flags. Flregex is the regex with flags applied.
         */
  private static Pattern compile(string flregex, string regex, int flags)
  {
    int re2Flags = RE2.PERL;
    if ((flags & DISABLE_UNICODE_GROUPS) != 0)
    {
      re2Flags &= ~RE2.UNICODE_GROUPS;
    }

    return new Pattern(regex, flags, RE2.CompileImpl(flregex, re2Flags, /*longest=*/ false));
  }

  /**
         * Matches a string against a regular expression.
         *
         * @param regex the regular expression
         * @param input the input
         * @return true if the regular expression matches the entire input
         * @throws PatternSyntaxException if the regular expression is malformed
         */
  public static bool matches(string regex, string input) => compile(regex).matcher(input).matches();

  public bool matches(string input) => this.matcher(input).matches();

  /**
         * Creates a new {@code Matcher} matching the pattern against the input.
         *
         * @param input the input string
         */
  public Matcher matcher(string input) => new Matcher(this, input);

  /**
         * Splits input around instances of the regular expression. It returns an array giving the strings
         * that occur before, between, and after instances of the regular expression. Empty strings that
         * would occur at the end of the array are omitted.
         *
         * @param input the input string to be split
         * @return the split strings
         */
  public string[] split(string input) => split(input, 0);

  /**
         * Splits input around instances of the regular expression. It returns an array giving the strings
         * that occur before, between, and after instances of the regular expression.
         *
         * <p>
         * If {@code limit &lt;= 0}, there is no limit on the size of the returned array. If
         * {@code limit == 0}, empty strings that would occur at the end of the array are omitted. If
         * {@code limit &gt; 0}, at most limit strings are returned. The final string contains the remainder
         * of the input, possibly including additional matches of the pattern.
         * </p>
         *
         * @param input the input string to be split
         * @param limit the limit
         * @return the split strings
         */
  public string[] split(string input, int limit) => split(new Matcher(this, input), limit);

  /** Helper: run split on m's input. */
  private String[] split(Matcher m, int limit)
  {
    int matchCount = 0;
    int arraySize = 0;
    int last = 0;
    while (m.find())
    {
      matchCount++;
      if (limit != 0 || last < m.start())
      {
        arraySize = matchCount;
      }

      last = m.end();
    }

    if (last < m.inputLength() || limit != 0)
    {
      matchCount++;
      arraySize = matchCount;
    }

    int trunc = 0;
    if (limit > 0 && arraySize > limit)
    {
      arraySize = limit;
      trunc = 1;
    }

    String[] array = new String[arraySize];
    int i = 0;
    last = 0;
    m.reset();
    while (m.find() && i < arraySize - trunc)
    {
      array[i++] = m.substring(last, m.start());
      last = m.end();
    }

    if (i < arraySize)
    {
      array[i] = m.substring(last, m.inputLength());
    }

    return array;
  }

  /**
         * Returns a literal pattern string for the specified string.
         *
         * <p>
         * This method produces a string that can be used to create a <code>Pattern</code> that would
         * match the string <code>s</code> as if it were a literal pattern.
         * </p>
         * Metacharacters or escape sequences in the input sequence will be given no special meaning.
         *
         * @param s The string to be literalized
         * @return A literal string replacement
         */
  public static String quote(String s) => RE2.QuoteMeta(s);

  public override string ToString() => _pattern;

  /**
         * Returns the number of capturing groups in this matcher's pattern. Group zero denotes the entire
         * pattern and is excluded from this count.
         *
         * @return the number of capturing groups in this pattern
         */
  public int GroupCount => _re2.NumberOfCapturingGroups;

  object ReadResolve() =>
    // The deserialized version will be missing the RE2 instance, so we need to create a new,
    // compiled version.
    Pattern.compile(_pattern, _flags);

  public override bool Equals(Object o)
  {
    if (this == o)
    {
      return true;
    }

    if (o == null || this.GetType() != o.GetType())
    {
      return false;
    }

    var other = (Pattern)o;
    return _flags == other._flags && _pattern.Equals(other._pattern);
  }

  public override int GetHashCode()
  {
    int result = _pattern.GetHashCode();
    result = 31 * result + _flags;
    return result;
  }

  //private static long serialVersionUID = 0;
}
