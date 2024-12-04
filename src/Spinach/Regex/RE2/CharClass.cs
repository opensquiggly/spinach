// From https://github.com/google/re2j
// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.
//
// Translation to C# by Ken Domino. Copyright 2018 Ken Domino.

namespace Spinach.Regex;

/**
     * A "builder"-style helper class for manipulating character classes represented as an array of
     * pairs of runes [lo, hi], each denoting an inclusive interval.
     *
     * All methods mutate the internal state and return {@code this}, allowing operations to be chained.
     */
public class CharClass
{
  private int[] _r; // inclusive ranges, pairs of [lo,hi].  r.length is even.
  private int _len; // prefix of |r| that is defined.  Even.

  // Constructs a CharClass with initial ranges |r|.
  // The right to mutate |r| is passed to the callee.
  public CharClass(int[] r)
  {
    this._r = r;
    this._len = r.Length;
  }

  // Constructs an empty CharClass.
  public CharClass()
  {
    this._r = Utils.EMPTY_INTS;
    this._len = 0;
  }

  // After a call to ensureCapacity(), |r.length| is at least |newLen|.
  private void EnsureCapacity(int newLen)
  {
    if (_r.Length < newLen)
    {
      // Expand by at least doubling, except when len == 0.
      // TODO(adonovan): opt: perhaps it would be better to allocate exactly
      // newLen, since the number of expansions is typically very small?
      if (newLen < _len * 2)
      {
        newLen = _len * 2;
      }

      int[] r2 = new int[newLen];
      System.Array.Copy(_r, 0, r2, 0, _len);
      _r = r2;
    }
  }

  // Returns the character class as an int array.  Subsequent CharClass
  // operations may mutate this array, so typically this is the last operation
  // performed on a given CharClass instance.
  public int[] ToArray()
  {
    if (this._len == _r.Length)
    {
      return _r;
    }
    else
    {
      int[] r2 = new int[_len];
      System.Array.Copy(_r, 0, r2, 0, _len);
      return r2;
    }
  }

  // cleanClass() sorts the ranges (pairs of elements) of this CharClass,
  // merges them, and eliminates duplicates.
  public CharClass CleanClass()
  {
    if (_len < 4)
    {
      return this;
    }

    // Sort by lo increasing, hi decreasing to break ties.
    QsortIntPair(_r, 0, _len - 2);

    // Merge abutting, overlapping.
    int w = 2; // write index
    for (int i = 2; i < _len; i += 2)
    {
      int lo = _r[i];
      int hi = _r[i + 1];
      if (lo <= _r[w - 1] + 1)
      {
        // merge with previous range
        if (hi > _r[w - 1])
        {
          _r[w - 1] = hi;
        }

        continue;
      }

      // new disjoint range
      _r[w] = lo;
      _r[w + 1] = hi;
      w += 2;
    }

    _len = w;

    return this;
  }

  // appendLiteral() appends the literal |x| to this CharClass.
  public CharClass AppendLiteral(int x, int flags) => ((flags & RE2.FOLD_CASE) != 0) ? AppendFoldedRange(x, x) : AppendRange(x, x);

  // appendRange() appends the range [lo-hi] (inclusive) to this CharClass.
  public CharClass AppendRange(int lo, int hi)
  {
    // Expand last range or next to last range if it overlaps or abuts.
    // Checking two ranges helps when appending case-folded
    // alphabets, so that one range can be expanding A-Z and the
    // other expanding a-z.
    if (_len > 0)
    {
      for (int i = 2; i <= 4; i += 2)
      {
        // twice, using i=2, i=4
        if (_len >= i)
        {
          int rlo = _r[_len - i];
          int rhi = _r[_len - i + 1];
          if (lo <= rhi + 1 && rlo <= hi + 1)
          {
            if (lo < rlo)
            {
              _r[_len - i] = lo;
            }

            if (hi > rhi)
            {
              _r[_len - i + 1] = hi;
            }

            return this;
          }
        }
      }
    }

    // Can't coalesce; append.   Expand capacity by doubling as needed.
    EnsureCapacity(_len + 2);
    _r[_len++] = lo;
    _r[_len++] = hi;
    return this;
  }

  // appendFoldedRange() appends the range [lo-hi] and its case
  // folding-equivalent runes to this CharClass.
  public CharClass AppendFoldedRange(int lo, int hi)
  {
    // Optimizations.
    if (lo <= Unicode.MIN_FOLD && hi >= Unicode.MAX_FOLD)
    {
      // Range is full: folding can't add more.
      return AppendRange(lo, hi);
    }

    if (hi < Unicode.MIN_FOLD || lo > Unicode.MAX_FOLD)
    {
      // Range is outside folding possibilities.
      return AppendRange(lo, hi);
    }

    if (lo < Unicode.MIN_FOLD)
    {
      // [lo, minFold-1] needs no folding.
      AppendRange(lo, Unicode.MIN_FOLD - 1);
      lo = Unicode.MIN_FOLD;
    }

    if (hi > Unicode.MAX_FOLD)
    {
      // [maxFold+1, hi] needs no folding.
      AppendRange(Unicode.MAX_FOLD + 1, hi);
      hi = Unicode.MAX_FOLD;
    }

    // Brute force.  Depend on appendRange to coalesce ranges on the fly.
    for (int c = lo; c <= hi; c++)
    {
      AppendRange(c, c);
      for (int f = Unicode.simpleFold(c); f != c; f = Unicode.simpleFold(f))
      {
        AppendRange(f, f);
      }
    }

    return this;
  }

  // appendClass() appends the class |x| to this CharClass.
  // It assumes |x| is clean.  Does not mutate |x|.
  public CharClass AppendClass(int[] x)
  {
    for (int i = 0; i < x.Length; i += 2)
    {
      AppendRange(x[i], x[i + 1]);
    }

    return this;
  }

  // appendFoldedClass() appends the case folding of the class |x| to this
  // CharClass.  Does not mutate |x|.
  public CharClass AppendFoldedClass(int[] x)
  {
    for (int i = 0; i < x.Length; i += 2)
    {
      AppendFoldedRange(x[i], x[i + 1]);
    }

    return this;
  }

  // appendNegatedClass() append the negation of the class |x| to this
  // CharClass.  It assumes |x| is clean.  Does not mutate |x|.
  public CharClass AppendNegatedClass(int[] x)
  {
    int nextLo = 0;
    for (int i = 0; i < x.Length; i += 2)
    {
      int lo = x[i];
      int hi = x[i + 1];
      if (nextLo <= lo - 1)
      {
        AppendRange(nextLo, lo - 1);
      }

      nextLo = hi + 1;
    }

    if (nextLo <= Unicode.MAX_RUNE)
    {
      AppendRange(nextLo, Unicode.MAX_RUNE);
    }

    return this;
  }

  // appendTable() appends the Unicode range table |table| to this CharClass.
  // Does not mutate |table|.
  public CharClass AppendTable(int[][] table)
  {
    foreach (int[] triple in table)
    {
      int lo = triple[0], hi = triple[1], stride = triple[2];
      if (stride == 1)
      {
        AppendRange(lo, hi);
        continue;
      }

      for (int c = lo; c <= hi; c += stride)
      {
        AppendRange(c, c);
      }
    }
    return this;
  }

  // appendNegatedTable() returns the result of appending the negation of range
  // table |table| to this CharClass.  Does not mutate |table|.
  public CharClass AppendNegatedTable(int[][] table)
  {
    int nextLo = 0; // lo end of next class to add
    foreach (int[] triple in table)
    {
      int lo = triple[0], hi = triple[1], stride = triple[2];
      if (stride == 1)
      {
        if (nextLo <= lo - 1)
        {
          AppendRange(nextLo, lo - 1);
        }

        nextLo = hi + 1;
        continue;
      }

      for (int c = lo; c <= hi; c += stride)
      {
        if (nextLo <= c - 1)
        {
          AppendRange(nextLo, c - 1);
        }

        nextLo = c + 1;
      }
    }
    if (nextLo <= Unicode.MAX_RUNE)
    {
      AppendRange(nextLo, Unicode.MAX_RUNE);
    }

    return this;
  }

  // appendTableWithSign() calls append{,Negated}Table depending on sign.
  // Does not mutate |table|.
  public CharClass AppendTableWithSign(int[][] table, int sign) => sign < 0 ? AppendNegatedTable(table) : AppendTable(table);

  // negateClass() negates this CharClass, which must already be clean.
  public CharClass NegateClass()
  {
    int nextLo = 0; // lo end of next class to add
    int w = 0; // write index
    for (int i = 0; i < _len; i += 2)
    {
      int lo = _r[i], hi = _r[i + 1];
      if (nextLo <= lo - 1)
      {
        _r[w] = nextLo;
        _r[w + 1] = lo - 1;
        w += 2;
      }

      nextLo = hi + 1;
    }

    _len = w;

    if (nextLo <= Unicode.MAX_RUNE)
    {
      // It's possible for the negation to have one more
      // range - this one - than the original class, so use append.
      EnsureCapacity(_len + 2);
      _r[_len++] = nextLo;
      _r[_len++] = Unicode.MAX_RUNE;
    }

    return this;
  }

  // appendClassWithSign() calls appendClass() if sign is +1 or
  // appendNegatedClass if sign is -1.  Does not mutate |x|.
  public CharClass AppendClassWithSign(int[] x, int sign) => sign < 0 ? AppendNegatedClass(x) : AppendClass(x);

  // appendGroup() appends CharGroup |g| to this CharClass, folding iff
  // |foldCase|.  Does not mutate |g|.
  public CharClass AppendGroup(CharGroup g, bool foldCase)
  {
    int[] cls = g.cls;
    if (foldCase)
    {
      cls = new CharClass().AppendFoldedClass(cls).CleanClass().ToArray();
    }

    return AppendClassWithSign(cls, g.sign);
  }

  // cmp() returns the ordering of the pair (a[i], a[i+1]) relative to
  // (pivotFrom, pivotTo), where the first component of the pair (lo) is
  // ordered naturally and the second component (hi) is in reverse order.
  private static int Cmp(int[] array, int i, int pivotFrom, int pivotTo)
  {
    int cmp = array[i] - pivotFrom;
    return cmp != 0 ? cmp : pivotTo - array[i + 1];
  }

  // qsortIntPair() quicksorts pairs of ints in |array| according to lt().
  // Precondition: |left|, |right|, |this.len| must all be even; |this.len > 1|.
  private static void QsortIntPair(int[] array, int left, int right)
  {
    int pivotIndex = ((left + right) / 2) & ~1;
    int pivotFrom = array[pivotIndex], pivotTo = array[pivotIndex + 1];
    int i = left, j = right;

    while (i <= j)
    {
      while (i < right && Cmp(array, i, pivotFrom, pivotTo) < 0)
      {
        i += 2;
      }

      while (j > left && Cmp(array, j, pivotFrom, pivotTo) > 0)
      {
        j -= 2;
      }

      if (i <= j)
      {
        if (i != j)
        {
          int temp = array[i];
          array[i] = array[j];
          array[j] = temp;
          temp = array[i + 1];
          array[i + 1] = array[j + 1];
          array[j + 1] = temp;
        }

        i += 2;
        j -= 2;
      }
    }

    if (left < j)
    {
      QsortIntPair(array, left, j);
    }

    if (i < right)
    {
      QsortIntPair(array, i, right);
    }
  }

  // Exposed, since useful for debugging CharGroups too.
  static string CharClassToString(int[] r, int len)
  {
    var b = new StringBuilder();
    b.Append('[');
    for (int i = 0; i < len; i += 2)
    {
      if (i > 0)
      {
        b.Append(' ');
      }

      int lo = r[i], hi = r[i + 1];
      // Avoid String.format (not available on GWT).
      // Cf. https://code.google.com/p/google-web-toolkit/issues/detail?id=3945
      if (lo == hi)
      {
        b.Append("0x");
        b.Append(lo.ToString("x"));
      }
      else
      {
        b.Append("0x");
        b.Append(lo.ToString("x"));
        b.Append("-0x");
        b.Append(hi.ToString("x"));
      }
    }

    b.Append(']');
    return b.ToString();
  }

  public override string ToString() => CharClassToString(_r, _len);
}
