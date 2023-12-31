// From https://github.com/google/re2j
// // Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// Translation to C# by Ken Domino. Copyright 2018 Ken Domino.

namespace Spinach.Regex;

/**
     * Utilities for dealing with Unicode better than Java does.
     *
     * @author adonovan@google.com (Alan Donovan)
     */
public class Unicode
{

  // The highest legal rune value.
  public const int MAX_RUNE = 0x10FFFF;

  // The highest legal ASCII value.
  public const int MAX_ASCII = 0x7f;

  // The highest legal Latin-1 value.
  public const int MAX_LATIN1 = 0xFF;

  // Minimum and maximum runes involved in folding.
  // Checked during test.
  public const int MIN_FOLD = 0x0041;
  public const int MAX_FOLD = 0x1044f;

  // is32 uses binary search to test whether rune is in the specified
  // slice of 32-bit ranges.
  // TODO(adonovan): opt: consider using int[n*3] instead of int[n][3].
  private static bool is32(int[][] ranges, int r)
  {
    // binary search over ranges
    for (int lo = 0, hi = ranges.Length; lo < hi;)
    {
      int m = lo + (hi - lo) / 2;
      int[] range = ranges[m]; // [lo, hi, stride]
      if (range[0] <= r && r <= range[1])
      {
        return ((r - range[0]) % range[2]) == 0;
      }

      if (r < range[0])
      {
        hi = m;
      }
      else
      {
        lo = m + 1;
      }
    }

    return false;
  }

  // is tests whether rune is in the specified table of ranges.
  private static bool @is(int[][] ranges, int r)
  {
    // common case: rune is ASCII or Latin-1, so use linear search.
    if (r <= MAX_LATIN1)
    {
      foreach (int[] range in ranges)
      {
        // range = [lo, hi, stride]
        if (r > range[1])
        {
          continue;
        }

        if (r < range[0])
        {
          return false;
        }

        return ((r - range[0]) % range[2]) == 0;
      }
      return false;
    }

    return ranges.Length > 0 && r >= ranges[0][0] && is32(ranges, r);
  }

  // isUpper reports whether the rune is an upper case letter.
  public static bool isUpper(int r)
  {
    // See comment in isGraphic.
    if (r <= MAX_LATIN1)
    {
      return System.Char.IsUpper((char)r);
    }

    return @is(UnicodeTables.Upper, r);
  }

  // isLower reports whether the rune is a lower case letter.
  public static bool isLower(int r)
  {
    // See comment in isGraphic.
    if (r <= MAX_LATIN1)
    {
      return System.Char.IsLower((char)r);
    }

    return @is(UnicodeTables.Lower, r);
  }

  // isTitle reports whether the rune is a title case letter.
  public static bool isTitle(int r)
  {
    if (r <= MAX_LATIN1)
    {
      return false;
    }

    return @is(UnicodeTables.Title, r);
  }

  // isPrint reports whether the rune is printable (Unicode L/M/N/P/S or ' ').
  public static bool isPrint(int r)
  {
    if (r <= MAX_LATIN1)
    {
      return (r >= 0x20 && r < 0x7F) || (r >= 0xA1 && r != 0xAD);
    }

    return @is(UnicodeTables.L, r)
           || @is(UnicodeTables.M, r)
           || @is(UnicodeTables.N, r)
           || @is(UnicodeTables.P, r)
           || @is(UnicodeTables.S, r);
  }

  // simpleFold iterates over Unicode code points equivalent under
  // the Unicode-defined simple case folding.  Among the code points
  // equivalent to rune (including rune itself), SimpleFold returns the
  // smallest r >= rune if one exists, or else the smallest r >= 0.
  //
  // For example:
  //      SimpleFold('A') = 'a'
  //      SimpleFold('a') = 'A'
  //
  //      SimpleFold('K') = 'k'
  //      SimpleFold('k') = '\u212A' (Kelvin symbol, K)
  //      SimpleFold('\u212A') = 'K'
  //
  //      SimpleFold('1') = '1'
  //
  // Derived from Go's unicode.SimpleFold.
  //
  public static int simpleFold(int r)
  {
    // Consult caseOrbit table for special cases.
    int lo = 0;
    int hi = UnicodeTables.CASE_ORBIT.Length;
    while (lo < hi)
    {
      int m = lo + (hi - lo) / 2;
      if (UnicodeTables.CASE_ORBIT[m][0] < r)
      {
        lo = m + 1;
      }
      else
      {
        hi = m;
      }
    }

    if (lo < UnicodeTables.CASE_ORBIT.Length && UnicodeTables.CASE_ORBIT[lo][0] == r)
    {
      return UnicodeTables.CASE_ORBIT[lo][1];
    }

    // No folding specified.  This is a one- or two-element
    // equivalence class containing rune and toLower(rune)
    // and toUpper(rune) if they are different from rune.
    int l = Characters.ToLowerCase(r);
    if (l != r)
    {
      return l;
    }

    return Characters.ToUpperCase(r);
  }

  private Unicode()
  {
  } // uninstantiable
}
