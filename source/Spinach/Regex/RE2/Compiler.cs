// Copyright 2010 The Go Authors. All rights reserved.
// Use of this source code is governed by a BSD-style
// license that can be found in the LICENSE file.

// Original Go source here:
// http://code.google.com/p/go/source/browse/src/pkg/regexp/syntax/compile.go

namespace Spinach.Regex;

/**
     * Compiler from {@code Regexp} (RE2 abstract syntax) to {@code RE2} (compiled regular expression).
     *
     * The only entry point is {@link #compileRegexp}.
     */
class Compiler
{

  /**
         * A fragment of a compiled regular expression program.
         *
         * @see http://swtch.com/~rsc/regexp/regexp1.html
         */
  private class Frag
  {
    public int i; // an instruction address (pc).
    public int @out; // a patch list; see explanation in Prog.java

    public Frag()
    {
      i = 0;
      @out = 0;
    }

    public Frag(int i)
    {
      this.i = i;
      @out = 0;
    }

    public Frag(int i, int @out)
    {
      this.i = i;
      this.@out = @out;
    }
  }

  private Prog prog = new Prog(); // Program being built

  private Compiler()
  {
    NewInst(Inst.InstOp.FAIL); // always the first instruction
  }

  public static Prog CompileRegexp(Regexp re)
  {
    var c = new Compiler();
    Frag f = c.Compile(re);
    c.prog.patch(f.@out, c.NewInst(Inst.InstOp.MATCH).i);
    c.prog.start = f.i;
    return c.prog;
  }

  private Frag NewInst(Inst.InstOp op)
  {
    // TODO(rsc): impose length limit.
    prog.addInst(op);
    return new Frag(prog.numInst() - 1);
  }

  // Returns a no-op fragment.  Sometimes unavoidable.
  private Frag Nop()
  {
    Frag f = NewInst(Inst.InstOp.NOP);
    f.@out = f.i << 1;
    return f;
  }

  private static Frag fail() => new Frag();

  // Given fragment a, returns (a) capturing as \n.
  // Given a fragment a, returns a fragment with capturing parens around a.
  private Frag Cap(int arg)
  {
    Frag f = NewInst(Inst.InstOp.CAPTURE);
    f.@out = f.i << 1;
    prog.getInst(f.i).arg = arg;
    if (prog.numCap < arg + 1)
    {
      prog.numCap = arg + 1;
    }

    return f;
  }

  // Given fragments a and b, returns ab; a|b
  private Frag Cat(Frag f1, Frag f2)
  {
    // concat of failure is failure
    if (f1.i == 0 || f2.i == 0)
    {
      return fail();
    }

    // TODO(rsc): elide nop
    prog.patch(f1.@out, f2.i);
    return new Frag(f1.i, f2.@out);
  }

  // Given fragments for a and b, returns fragment for a|b.
  private Frag Alt(Frag f1, Frag f2)
  {
    // alt of failure is other
    if (f1.i == 0)
    {
      return f2;
    }

    if (f2.i == 0)
    {
      return f1;
    }

    Frag f = NewInst(Inst.InstOp.ALT);
    Inst i = prog.getInst(f.i);
    i.@out = f1.i;
    i.arg = f2.i;
    f.@out = prog.append(f1.@out, f2.@out);
    return f;
  }

  // Given a fragment for a, returns a fragment for a? or a?? (if nongreedy)
  private Frag Quest(Frag f1, bool nongreedy)
  {
    Frag f = NewInst(Inst.InstOp.ALT);
    Inst i = prog.getInst(f.i);
    if (nongreedy)
    {
      i.arg = f1.i;
      f.@out = f.i << 1;
    }
    else
    {
      i.@out = f1.i;
      f.@out = f.i << 1 | 1;
    }

    f.@out = prog.append(f.@out, f1.@out);
    return f;
  }

  // Given a fragment a, returns a fragment for a* or a*? (if nongreedy)
  private Frag Star(Frag f1, bool nongreedy)
  {
    Frag f = NewInst(Inst.InstOp.ALT);
    Inst i = prog.getInst(f.i);
    if (nongreedy)
    {
      i.arg = f1.i;
      f.@out = f.i << 1;
    }
    else
    {
      i.@out = f1.i;
      f.@out = f.i << 1 | 1;
    }

    prog.patch(f1.@out, f.i);
    return f;
  }

  // Given a fragment for a, returns a fragment for a+ or a+? (if nongreedy)
  private Frag Plus(Frag f1, bool nongreedy) => new Frag(f1.i, Star(f1, nongreedy).@out);

  // op is a bitmask of EMPTY_* flags.
  private Frag Empty(int op)
  {
    Frag f = NewInst(Inst.InstOp.EMPTY_WIDTH);
    prog.getInst(f.i).arg = op;
    f.@out = f.i << 1;
    return f;
  }

  private Frag Rune(int r, int flags) => rune(new int[] { r }, flags);

  // flags : parser flags
  private Frag rune(int[] runes, int flags)
  {
    Frag f = NewInst(Inst.InstOp.RUNE);
    Inst i = prog.getInst(f.i);
    i.runes = runes;
    flags &= RE2.FOLD_CASE; // only relevant flag is FoldCase
    if (runes.Length != 1 || Unicode.simpleFold(runes[0]) == runes[0])
    {
      flags &= ~RE2.FOLD_CASE; // and sometimes not even that
    }

    i.arg = flags;
    f.@out = f.i << 1;
    // Special cases for exec machine.
    if (((flags & RE2.FOLD_CASE) == 0 && runes.Length == 1)
        || (runes.Length == 2 && runes[0] == runes[1]))
    {
      i.op = Inst.InstOp.RUNE1;
    }
    else if (runes.Length == 2 && runes[0] == 0 && runes[1] == Unicode.MAX_RUNE)
    {
      i.op = Inst.InstOp.RUNE_ANY;
    }
    else if (runes.Length == 4
             && runes[0] == 0
             && runes[1] == '\n' - 1
             && runes[2] == '\n' + 1
             && runes[3] == Unicode.MAX_RUNE)
    {
      i.op = Inst.InstOp.RUNE_ANY_NOT_NL;
    }

    return f;
  }

  private static int[] ANY_RUNE_NOT_NL = { 0, '\n' - 1, '\n' + 1, Unicode.MAX_RUNE };
  private static int[] ANY_RUNE = { 0, Unicode.MAX_RUNE };

  private Frag Compile(Regexp re)
  {
    switch (re.op)
    {
      case Regexp.Op.NO_MATCH:
        return fail();
      case Regexp.Op.EMPTY_MATCH:
        return Nop();
      case Regexp.Op.LITERAL:
        if (re.runes.Length == 0)
        {
          return Nop();
        }
        else
        {
          Frag f = null;
          foreach (int r in re.runes)
          {
            Frag f1 = Rune(r, re.flags);
            f = (f == null) ? f1 : Cat(f, f1);
          }
          return f;
        }
      case Regexp.Op.CHAR_CLASS:
        return rune(re.runes, re.flags);
      case Regexp.Op.ANY_CHAR_NOT_NL:
        return rune(ANY_RUNE_NOT_NL, 0);
      case Regexp.Op.ANY_CHAR:
        return rune(ANY_RUNE, 0);
      case Regexp.Op.BEGIN_LINE:
        return Empty(Utils.EMPTY_BEGIN_LINE);
      case Regexp.Op.END_LINE:
        return Empty(Utils.EMPTY_END_LINE);
      case Regexp.Op.BEGIN_TEXT:
        return Empty(Utils.EMPTY_BEGIN_TEXT);
      case Regexp.Op.END_TEXT:
        return Empty(Utils.EMPTY_END_TEXT);
      case Regexp.Op.WORD_BOUNDARY:
        return Empty(Utils.EMPTY_WORD_BOUNDARY);
      case Regexp.Op.NO_WORD_BOUNDARY:
        return Empty(Utils.EMPTY_NO_WORD_BOUNDARY);
      case Regexp.Op.CAPTURE:
        {
          Frag bra = Cap(re.cap << 1), sub = Compile(re.subs[0]), ket = Cap(re.cap << 1 | 1);
          return Cat(Cat(bra, sub), ket);
        }
      case Regexp.Op.STAR:
        return Star(Compile(re.subs[0]), (re.flags & RE2.NON_GREEDY) != 0);
      case Regexp.Op.PLUS:
        return Plus(Compile(re.subs[0]), (re.flags & RE2.NON_GREEDY) != 0);
      case Regexp.Op.QUEST:
        return Quest(Compile(re.subs[0]), (re.flags & RE2.NON_GREEDY) != 0);
      case Regexp.Op.CONCAT:
        if (re.subs.Length == 0)
        {
          return Nop();
        }
        else
        {
          Frag f = null;
          foreach (Regexp sub in re.subs)
          {
            Frag f1 = Compile(sub);
            f = (f == null) ? f1 : Cat(f, f1);
          }

          return f;
        }
      case Regexp.Op.ALTERNATE:
        {
          if (re.subs.Length == 0)
          {
            return Nop();
          }
          else
          {
            Frag f = null;
            foreach (Regexp sub in re.subs)
            {
              Frag f1 = Compile(sub);
              f = (f == null) ? f1 : Alt(f, f1);
            }

            return f;
          }
        }
      default:
        throw new IllegalStateException("regexp: unhandled case in compile");
    }
  }
}
