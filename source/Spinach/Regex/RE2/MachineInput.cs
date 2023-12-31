// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/regexp.go

namespace Spinach.Regex;

/**
     * MachineInput abstracts different representations of the input text supplied to the Machine. It
     * provides one-character lookahead.
     */
public abstract class MachineInput
{

  public static int EOF = (-1 << 3) | 0;

  public static MachineInput fromUTF8(byte[] b) => new UTF8Input(b);

  public static MachineInput fromUTF8(byte[] b, int start, int end) => new UTF8Input(b, start, end);

  public static MachineInput fromUTF16(string s) => new UTF16Input(s, 0, s.Length);

  public static MachineInput fromUTF16(string s, int start, int end) => new UTF16Input(s, start, end);

  //// Interface

  // Returns the rune at the specified index; the units are
  // unspecified, but could be UTF-8 byte, UTF-16 char, or rune
  // indices.  Returns the width (in the same units) of the rune in
  // the lower 3 bits, and the rune (Unicode code point) in the high
  // bits.  Never negative, except for EOF which is represented as -1
  // << 3 | 0.
  public abstract int step(int pos);

  // can we look ahead without losing info?
  public abstract bool canCheckPrefix();

  // Returns the index relative to |pos| at which |re2.prefix| is found
  // in this input stream, or a negative value if not found.
  public abstract int index(RE2 re2, int pos);

  // Returns a bitmask of EMPTY_* flags.
  public abstract int context(int pos);

  // Returns the end position in the same units as step().
  public abstract int endPos();

  //// Implementations

  // An implementation of MachineInput for UTF-8 byte arrays.
  // |pos| and |width| are byte indices.
  private class UTF8Input : MachineInput
  {

    byte[] b;
    int start;
    int end;

    public UTF8Input(byte[] b)
    {
      this.b = b;
      start = 0;
      end = b.Length;
    }

    public UTF8Input(byte[] b, int start, int end)
    {
      if (end > b.Length)
      {
        throw new System.IndexOutOfRangeException(
          "end is greater than length: " + end + " > " + b.Length);
      }

      this.b = b;
      this.start = start;
      this.end = end;
    }

    public override int step(int i)
    {
      i += start;
      if (i >= end)
      {
        return EOF;
      }

      // UTF-8.  RFC 3629 in five lines:
      //
      // Unicode code points            UTF-8 encoding (binary)
      //         00-7F  (7 bits)   0tuvwxyz
      //     0080-07FF (11 bits)   110pqrst 10uvwxyz
      //     0800-FFFF (16 bits)   1110jklm 10npqrst 10uvwxyz
      // 010000-10FFFF (21 bits)   11110efg 10hijklm 10npqrst 10uvwxyz
      int x = b[i++] & 0xff; // zero extend
      if ((x & 0x80) == 0)
      {
        return x << 3 | 1;
      }
      else if ((x & 0xE0) == 0xC0)
      {
        // 110xxxxx
        x = x & 0x1F;
        if (i >= end)
        {
          return EOF;
        }

        x = x << 6 | (b[i++] & 0x3F);
        return x << 3 | 2;
      }
      else if ((x & 0xF0) == 0xE0)
      {
        // 1110xxxx
        x = x & 0x0F;
        if (i + 1 >= end)
        {
          return EOF;
        }

        x = x << 6 | (b[i++] & 0x3F);
        x = x << 6 | (b[i++] & 0x3F);
        return x << 3 | 3;
      }
      else
      {
        // 11110xxx
        x = x & 0x07;
        if (i + 2 >= end)
        {
          return EOF;
        }

        x = x << 6 | (b[i++] & 0x3F);
        x = x << 6 | (b[i++] & 0x3F);
        x = x << 6 | (b[i++] & 0x3F);
        return x << 3 | 4;
      }
    }

    public override bool canCheckPrefix() => true;

    public override int index(RE2 re2, int pos)
    {
      pos += start;
      int i = Utils.indexOf(b, re2.prefixUTF8, pos);
      return i < 0 ? i : i - pos;
    }

    public override int context(int pos)
    {
      pos += this.start;
      int r1 = -1;
      if (pos > this.start && pos <= this.end)
      {
        int start = pos - 1;
        r1 = b[start--];
        if (r1 >= 0x80)
        {
          // decode UTF-8
          // Find start, up to 4 bytes earlier.
          int lim = pos - 4;
          if (lim < this.start)
          {
            lim = this.start;
          }

          while (start >= lim && (b[start] & 0xC0) == 0x80)
          {
            // 10xxxxxx
            start--;
          }

          if (start < this.start)
          {
            start = this.start;
          }

          r1 = step(start) >> 3;
        }
      }

      int r2 = pos < this.end ? (step(pos) >> 3) : -1;
      return Utils.emptyOpContext(r1, r2);
    }

    public override int endPos() => end;
  }

  // |pos| and |width| are in Java "char" units.
  private class UTF16Input : MachineInput
  {
    string str;
    int start;
    int end;

    public UTF16Input(string str, int start, int end)
    {
      this.str = str;
      this.start = start;
      this.end = end;
    }

    static int CharCount(int codePoint) =>
      codePoint >= 0x10000 ? 2 : 1;

    public override int step(int pos)
    {
      pos += start;
      if (pos < end)
      {
        int rune = Char.ConvertToUtf32(str, pos);
        return rune << 3 | CharCount(rune);
      }
      else
      {
        return EOF;
      }
    }

    public override bool canCheckPrefix() => true;

    public override int index(RE2 re2, int pos)
    {
      pos += start;
      int i = indexOf(str, re2.prefix, pos);
      return i < 0 ? i : i - pos;
    }

    public override int context(int pos)
    {
      int before = pos;
      pos += start;
      //               int r1 = pos > start && pos <= end ? Character.codePointBefore(str, pos) : -1;
      //               int r2 = pos < end ? Character.codePointAt(str, pos) : -1;

      int r1 = pos > start && pos < end ? Char.ConvertToUtf32(str, before) : -1;
      int r2 = pos < end ? Char.ConvertToUtf32(str, pos) : -1;

      return Utils.emptyOpContext(r1, r2);
    }

    public override int endPos() => end;

    private int indexOf(string hayStack, String needle, int pos) =>
      //if (hayStack as String != null) {
      //    return ((String) hayStack).IndexOf(needle, pos);
      //}
      //if (hayStack as StringBuilder != null) {
      //    return ((StringBuilder) hayStack).indexOf(needle, pos);
      //}
      indexOfFallback(hayStack, needle, pos);

    // Modified version of {@link String#indexOf(String) that allows a CharSequence.
    private int indexOfFallback(string hayStack, String needle, int fromIndex)
    {
      if (fromIndex >= hayStack.Length)
      {
        return needle.Length == 0 ? 0 : -1;
      }

      if (fromIndex < 0)
      {
        fromIndex = 0;
      }

      if (needle.Length == 0)
      {
        return fromIndex;
      }

      char first = needle[0];
      int max = hayStack.Length - needle.Length;

      for (int i = fromIndex; i <= max; i++)
      {
        /* Look for first character. */
        if (hayStack[i] != first)
        {
          while (++i <= max && hayStack[i] != first)
          {
          }
        }

        /* Found first character, now look at the rest of v2 */
        if (i <= max)
        {
          int j = i + 1;
          int end = j + needle.Length - 1;
          for (int k = 1; j < end && hayStack[j] == needle[k]; j++, k++)
          {
          }

          if (j == end)
          {
            /* Found whole string. */
            return i;
          }
        }
      }

      return -1;
    }
  }
}
