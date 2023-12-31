// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/syntax/regexp.go

namespace Spinach.Regex;

/**
     * Regular expression abstract syntax tree. Produced by parser, used by compiler. NB, this
     * corresponds to {@code syntax.regexp} in the Go implementation; Go's {@code regexp} is called
     * {@code RE2} in Java.
     */
public class Regexp
{
  public enum Op
  {
    NO_MATCH = 0, // Matches no strings.
    EMPTY_MATCH = 1, // Matches empty string.
    LITERAL = 2, // Matches runes[] sequence
    CHAR_CLASS = 3, // Matches Runes interpreted as range pair list
    ANY_CHAR_NOT_NL = 4, // Matches any character except '\n'
    ANY_CHAR = 5, // Matches any character
    BEGIN_LINE = 6, // Matches empty string at end of line
    END_LINE = 7, // Matches empty string at end of line
    BEGIN_TEXT = 8, // Matches empty string at beginning of text
    END_TEXT = 9, // Matches empty string at end of text
    WORD_BOUNDARY = 10, // Matches word boundary `\b`
    NO_WORD_BOUNDARY = 11, // Matches word non-boundary `\B`
    CAPTURE = 12, // Capturing subexpr with index cap, optional name name
    STAR = 13, // Matches subs[0] zero or more times.
    PLUS = 14, // Matches subs[0] one or more times.
    QUEST = 15, // Matches subs[0] zero or one times.
    REPEAT = 16, // Matches subs[0] [min, max] times; max=-1 => no limit.
    CONCAT = 17, // Matches concatenation of subs[]
    ALTERNATE = 18, // Matches union of subs[]

    // Pseudo ops, used internally by Parser for parsing stack:
    LEFT_PAREN,
    VERTICAL_BAR

  }

  public static bool isPseudo(Op op) => op >= Op.LEFT_PAREN;

  public static Regexp[] EMPTY_SUBS = { };

  public Op op; // operator
  public int flags; // bitmap of parse flags

  public Regexp[] subs; // subexpressions, if any.  Never null.

  // subs[0] is used as the freelist.
  public int[] runes; // matched runes, for LITERAL, CHAR_CLASS
  public int min, max; // min, max for REPEAT
  public int cap; // capturing index, for CAPTURE

  public string name; // capturing name, for CAPTURE
  // Do update copy ctor when adding new fields!

  public Regexp(Op op)
  {
    this.op = op;
  }

  // Shallow copy constructor.
  public Regexp(Regexp that)
  {
    this.op = that.op;
    this.flags = that.flags;
    this.subs = that.subs;
    this.runes = that.runes;
    this.min = that.min;
    this.max = that.max;
    this.cap = that.cap;
    this.name = that.name;
  }

  public void reinit()
  {
    this.flags = 0;
    subs = EMPTY_SUBS;
    runes = null;
    cap = min = max = 0;
    name = null;
  }

  public override string ToString()
  {
    var @out = new StringBuilder();
    appendTo(@out);
    return @out.ToString();
  }

  private static void quoteIfHyphen(StringBuilder @out, int rune)
  {
    if (rune == '-')
    {
      @out.Append('\\');
    }
  }

  // appendTo() appends the Perl syntax for |this| regular expression to |out|.
  private void appendTo(StringBuilder @out)
  {
    switch (op)
    {
      case Op.NO_MATCH:
        @out.Append("[^\\x00-\\x{10FFFF}]");
        break;
      case Op.EMPTY_MATCH:
        @out.Append("(?:)");
        break;
      case Op.STAR:
      case Op.PLUS:
      case Op.QUEST:
      case Op.REPEAT:
        {
          Regexp sub = subs[0];
          if (sub.op > Op.CAPTURE
              || (sub.op == Op.LITERAL && sub.runes.Length > 1))
          {
            @out.Append("(?:");
            sub.appendTo(@out);
            @out.Append(')');
          }
          else
          {
            sub.appendTo(@out);
          }

          switch (op)
          {
            case Op.STAR:
              @out.Append('*');
              break;
            case Op.PLUS:
              @out.Append('+');
              break;
            case Op.QUEST:
              @out.Append('?');
              break;
            case Op.REPEAT:
              @out.Append('{').Append(min);
              if (min != max)
              {
                @out.Append(',');
                if (max >= 0)
                {
                  @out.Append(max);
                }
              }

              @out.Append('}');
              break;
          }

          if ((flags & RE2.NON_GREEDY) != 0)
          {
            @out.Append('?');
          }

          break;
        }
      case Op.CONCAT:
        foreach (Regexp sub in subs)
        {
          if (sub.op == Op.ALTERNATE)
          {
            @out.Append("(?:");
            sub.appendTo(@out);
            @out.Append(')');
          }
          else
          {
            sub.appendTo(@out);
          }
        }

        break;
      case Op.ALTERNATE:
        {
          String sep = "";
          foreach (Regexp sub in subs)
          {
            @out.Append(sep);
            sep = "|";
            sub.appendTo(@out);
          }

          break;
        }
      case Op.LITERAL:
        if ((flags & RE2.FOLD_CASE) != 0)
        {
          @out.Append("(?i:");
        }

        foreach (int rune in runes)
        {
          Utils.escapeRune(@out, rune);
        }
        if ((flags & RE2.FOLD_CASE) != 0)
        {
          @out.Append(')');
        }

        break;
      case Op.ANY_CHAR_NOT_NL:
        @out.Append("(?-s:.)");
        break;
      case Op.ANY_CHAR:
        @out.Append("(?s:.)");
        break;
      case Op.CAPTURE:
        if (name == null || name.Length == 0)
        {
          @out.Append('(');
        }
        else
        {
          @out.Append("(?P<");
          @out.Append(name);
          @out.Append(">");
        }

        if (subs[0].op != Op.EMPTY_MATCH)
        {
          subs[0].appendTo(@out);
        }

        @out.Append(')');
        break;
      case Op.BEGIN_TEXT:
        @out.Append("\\A");
        break;
      case Op.END_TEXT:
        if ((flags & RE2.WAS_DOLLAR) != 0)
        {
          @out.Append("(?-m:$)");
        }
        else
        {
          @out.Append("\\z");
        }

        break;
      case Op.BEGIN_LINE:
        @out.Append('^');
        break;
      case Op.END_LINE:
        @out.Append('$');
        break;
      case Op.WORD_BOUNDARY:
        @out.Append("\\b");
        break;
      case Op.NO_WORD_BOUNDARY:
        @out.Append("\\B");
        break;
      case Op.CHAR_CLASS:
        if (runes.Length % 2 != 0)
        {
          @out.Append("[invalid char class]");
          break;
        }

        @out.Append('[');
        if (runes.Length == 0)
        {
          @out.Append("^\\x00-\\x{10FFFF}");
        }
        else if (runes[0] == 0 && runes[runes.Length - 1] == Unicode.MAX_RUNE)
        {
          // Contains 0 and MAX_RUNE.  Probably a negated class.
          // Print the gaps.
          @out.Append('^');
          for (int i = 1; i < runes.Length - 1; i += 2)
          {
            int lo = runes[i] + 1;
            int hi = runes[i + 1] - 1;
            quoteIfHyphen(@out, lo);
            Utils.escapeRune(@out, lo);
            if (lo != hi)
            {
              @out.Append('-');
              quoteIfHyphen(@out, hi);
              Utils.escapeRune(@out, hi);
            }
          }
        }
        else
        {
          for (int i = 0; i < runes.Length; i += 2)
          {
            int lo = runes[i];
            int hi = runes[i + 1];
            quoteIfHyphen(@out, lo);
            Utils.escapeRune(@out, lo);
            if (lo != hi)
            {
              @out.Append('-');
              quoteIfHyphen(@out, hi);
              Utils.escapeRune(@out, hi);
            }
          }
        }

        @out.Append(']');
        break;
      default: // incl. pseudos
        @out.Append(op);
        break;
    }
  }

  // maxCap() walks the regexp to find the maximum capture index.
  public int maxCap()
  {
    int m = 0;
    if (op == Op.CAPTURE)
    {
      m = cap;
    }

    if (subs != null)
    {
      foreach (Regexp sub in subs)
      {
        int n = sub.maxCap();
        if (m < n)
        {
          m = n;
        }
      }
    }

    return m;
  }


  public override int GetHashCode()
  {
    int hashcode = op.GetHashCode();
    switch (op)
    {
      case Op.END_TEXT:
        hashcode += 31 * (flags & RE2.WAS_DOLLAR);
        break;
      case Op.LITERAL:
      case Op.CHAR_CLASS:
        hashcode += 31 * runes.GetHashCode();
        break;
      case Op.ALTERNATE:
      case Op.CONCAT:
        hashcode += 31 * subs.GetHashCode();
        break;
      case Op.STAR:
      case Op.PLUS:
      case Op.QUEST:
        hashcode += 31 * (flags & RE2.NON_GREEDY) + 31 * subs[0].GetHashCode();
        break;
      case Op.REPEAT:
        hashcode += 31 * min + 31 * max + 31 * subs[0].GetHashCode();
        break;
      case Op.CAPTURE:
        hashcode += 31 * cap + 31 * (name != null ? name.GetHashCode() : 0) + 31 * subs[0].GetHashCode();
        break;
    }

    return hashcode;
  }

  // equals() returns true if this and that have identical structure.
  public override bool Equals(Object that)
  {
    if (that as Regexp == null)
    {
      return false;
    }

    Regexp x = this;
    var y = (Regexp)that;
    if (x.op != y.op)
    {
      return false;
    }

    switch (x.op)
    {
      case Op.END_TEXT:
        // The parse flags remember whether this is \z or \Z.
        if ((x.flags & RE2.WAS_DOLLAR) != (y.flags & RE2.WAS_DOLLAR))
        {
          return false;
        }

        break;
      case Op.LITERAL:
      case Op.CHAR_CLASS:
        if (!Array.Equals(x.runes, y.runes))
        {
          return false;
        }

        break;
      case Op.ALTERNATE:
      case Op.CONCAT:
        if (x.subs.Length != y.subs.Length)
        {
          return false;
        }

        for (int i = 0; i < x.subs.Length; ++i)
        {
          if (!x.subs[i].Equals(y.subs[i]))
          {
            return false;
          }
        }

        break;
      case Op.STAR:
      case Op.PLUS:
      case Op.QUEST:
        if ((x.flags & RE2.NON_GREEDY) != (y.flags & RE2.NON_GREEDY)
            || !x.subs[0].Equals(y.subs[0]))
        {
          return false;
        }

        break;
      case Op.REPEAT:
        if ((x.flags & RE2.NON_GREEDY) != (y.flags & RE2.NON_GREEDY)
            || x.min != y.min
            || x.max != y.max
            || !x.subs[0].Equals(y.subs[0]))
        {
          return false;
        }

        break;
      case Op.CAPTURE:
        if (x.cap != y.cap
            || (x.name == null ? y.name != null : !x.name.Equals(y.name))
            || !x.subs[0].Equals(y.subs[0]))
        {
          return false;
        }

        break;
    }

    return true;
  }
}
