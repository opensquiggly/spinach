// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/syntax/prog.go

namespace Spinach.Regex;

/**
     * A single instruction in the regular expression virtual machine.
     *
     * @see http://swtch.com/~rsc/regexp/regexp2.html
     */
public sealed class Inst
{
  public enum InstOp
  {
    ALT = 1,
    ALT_MATCH = 2,
    CAPTURE = 3,
    EMPTY_WIDTH = 4,
    FAIL = 5,
    MATCH = 6,
    NOP = 7,
    RUNE = 8,
    RUNE1 = 9,
    RUNE_ANY = 10,
    RUNE_ANY_NOT_NL = 11
  }

  public InstOp op;
  public int @out; // all but MATCH, FAIL
  public int arg; // ALT, ALT_MATCH, CAPTURE, EMPTY_WIDTH

  public int[] runes; // length==1 => exact match
  // otherwise a list of [lo,hi] pairs.  hi is *inclusive*.
  // REVIEWERS: why not half-open intervals?

  public Inst(Inst.InstOp op)
  {
    this.op = (InstOp)op;
  }

  public static bool isRuneOp(InstOp op) => InstOp.RUNE <= op && op <= InstOp.RUNE_ANY_NOT_NL;

  // MatchRune returns true if the instruction matches (and consumes) r.
  // It should only be called when op == InstRune.
  public bool matchRune(int r)
  {
    // Special case: single-rune slice is from literal string, not char
    // class.
    if (runes.Length == 1)
    {
      int r0 = runes[0];
      if (r == r0)
      {
        return true;
      }

      if ((arg & RE2.FOLD_CASE) != 0)
      {
        for (int r1 = Unicode.simpleFold(r0); r1 != r0; r1 = Unicode.simpleFold(r1))
        {
          if (r == r1)
          {
            return true;
          }
        }
      }

      return false;
    }

    // Peek at the first few pairs.
    // Should handle ASCII well.
    for (int j = 0; j < runes.Length && j <= 8; j += 2)
    {
      if (r < runes[j])
      {
        return false;
      }

      if (r <= runes[j + 1])
      {
        return true;
      }
    }

    // Otherwise binary search.
    for (int lo = 0, hi = runes.Length / 2; lo < hi;)
    {
      int m = lo + (hi - lo) / 2;
      int c = runes[2 * m];
      if (c <= r)
      {
        if (r <= runes[2 * m + 1])
        {
          return true;
        }

        lo = m + 1;
      }
      else
      {
        hi = m;
      }
    }

    return false;
  }

  public override string ToString()
  {
    switch (op)
    {
      case InstOp.ALT:
        return "alt -> " + @out + ", " + arg;
      case InstOp.ALT_MATCH:
        return "altmatch -> " + @out + ", " + arg;
      case InstOp.CAPTURE:
        return "cap " + arg + " -> " + @out;
      case InstOp.EMPTY_WIDTH:
        return "empty " + arg + " -> " + @out;
      case InstOp.MATCH:
        return "match";
      case InstOp.FAIL:
        return "fail";
      case InstOp.NOP:
        return "nop -> " + @out;
      case InstOp.RUNE:
        if (runes == null)
        {
          return "rune <null>"; // can't happen
        }

        return "rune "
               + escapeRunes(runes)
               + (((arg & RE2.FOLD_CASE) != 0) ? "/i" : "")
               + " -> "
               + @out;
      case InstOp.RUNE1:
        return "rune1 " + escapeRunes(runes) + " -> " + @out;
      case InstOp.RUNE_ANY:
        return "any -> " + @out;
      case InstOp.RUNE_ANY_NOT_NL:
        return "anynotnl -> " + @out;
      default:
        throw new IllegalStateException("unhandled case in Inst.toString");
    }
  }

  // Returns an RE2 expression matching exactly |runes|.
  private static String escapeRunes(int[] runes)
  {
    var @out = new StringBuilder();
    @out.Append('"');
    foreach (int rune in runes)
    {
      Utils.escapeRune(@out, rune);
    }

    @out.Append('"');
    return @out.ToString();
  }
}
