namespace Spinach.Regex.Analyzers;

public class RegexParser
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private static NormalizedRegex BuildNormalizedRegex(Regexp re)
  {
    switch (re.op)
    {
      case Regexp.Op.NO_MATCH:
        return new NormalizedRegex { Op = NormalizedOpTypes.NoMatch };

      case Regexp.Op.EMPTY_MATCH:
      case Regexp.Op.BEGIN_LINE:
      case Regexp.Op.END_LINE:
      case Regexp.Op.BEGIN_TEXT:
      case Regexp.Op.END_TEXT:
      case Regexp.Op.WORD_BOUNDARY:
      case Regexp.Op.NO_WORD_BOUNDARY:
        return new NormalizedRegex { Op = NormalizedOpTypes.EmptyString };

      case Regexp.Op.LITERAL:
        var list = new List<NormalizedRegex>();
        foreach (int r in re.runes)
        {
          if ((re.flags & RE2.FOLD_CASE) != 0)
          {
            var folds = new List<NormalizedRegex>
            {
              new NormalizedRegex {Op = NormalizedOpTypes.Literal, LitBegin = r, LitEnd = r}
            };
            for (int f = Unicode.simpleFold(r); f != r; f = Unicode.simpleFold(f))
            {
              folds.Add(
                new NormalizedRegex
                {
                  Op = NormalizedOpTypes.Literal,
                  LitBegin = f,
                  LitEnd = f
                }
              );
            }

            list.Add(new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = folds });
          }
          else
          {
            list.Add(new NormalizedRegex { Op = NormalizedOpTypes.Literal, LitBegin = r, LitEnd = r });
          }
        }

        return new NormalizedRegex { Op = NormalizedOpTypes.Concatenate, Subs = list };

      case Regexp.Op.ANY_CHAR_NOT_NL:
        {
          var beforeNL = new NormalizedRegex { Op = NormalizedOpTypes.Literal, LitBegin = 0, LitEnd = '\n' };
          var afterNL = new NormalizedRegex
          { Op = NormalizedOpTypes.Literal, LitBegin = '\n', LitEnd = Unicode.MAX_RUNE };
          var subs = new List<NormalizedRegex>()
          {
            beforeNL,
            afterNL
          };
          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs };
        }

      case Regexp.Op.ANY_CHAR:
        return new NormalizedRegex { Op = NormalizedOpTypes.Literal, LitBegin = 0, LitEnd = Unicode.MAX_RUNE };

      case Regexp.Op.CAPTURE:
        return BuildNormalizedRegex(re.subs[0]);

      case Regexp.Op.CONCAT:
        {
          var subs = new List<NormalizedRegex>();
          subs.AddRange(re.subs.Select(BuildNormalizedRegex));
          return new NormalizedRegex { Op = NormalizedOpTypes.Concatenate, Subs = subs };
        }

      case Regexp.Op.ALTERNATE:
        {
          var subs = new List<NormalizedRegex>();
          subs.AddRange(re.subs.Select(BuildNormalizedRegex));
          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs };
        }

      case Regexp.Op.QUEST:
        {
          var subs = new List<NormalizedRegex>
          {
            BuildNormalizedRegex(re.subs[0]),
            new NormalizedRegex {Op = NormalizedOpTypes.EmptyString}
          };
          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs };
        }

      case Regexp.Op.STAR:
        {
          var subs = new List<NormalizedRegex>
          {
            BuildNormalizedRegex(re.subs[0]),
          };
          return new NormalizedRegex { Op = NormalizedOpTypes.KleeneStar, Subs = subs };
        }

      case Regexp.Op.REPEAT:
        {
          var args = new List<NormalizedRegex>();
          NormalizedRegex sub = BuildNormalizedRegex(re.subs[0]);
          for (int i = 0; i < re.min; i++)
          {
            args.Add(sub);
          }

          for (int i = re.min; i < re.max; i++)
          {
            var subs = new List<NormalizedRegex>
            {
              sub,
              new NormalizedRegex {Op = NormalizedOpTypes.EmptyString}
            };
            args.Add(new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = subs });
          }

          return new NormalizedRegex { Op = NormalizedOpTypes.Concatenate, Subs = args };
        }

      case Regexp.Op.PLUS:
        {
          NormalizedRegex parsed = BuildNormalizedRegex(re.subs[0]);
          return new NormalizedRegex
          {
            Op = NormalizedOpTypes.Concatenate,
            Subs = new List<NormalizedRegex>{
            parsed, new NormalizedRegex
            {
              Op = NormalizedOpTypes.KleeneStar,
              Subs = new List<NormalizedRegex>{ parsed }
            }}
          };
        }

      case Regexp.Op.CHAR_CLASS:
        {
          var args = new List<NormalizedRegex>();
          for (int i = 0; i < re.runes.Length - 1; i += 2)
          {
            args.Add(new NormalizedRegex
            {
              Op = NormalizedOpTypes.Literal,
              LitBegin = re.runes[i],
              LitEnd = re.runes[i + 1]
            });
          }

          return new NormalizedRegex { Op = NormalizedOpTypes.Alternate, Subs = args };
        }
    }

    throw new Exception($"Unknown regexp operation: {re.op}");
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Static Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public static NormalizedRegex Parse(string expr)
  {
    Regexp re = Parser.Parse(expr, RE2.PERL | RE2.PERL_X);
    Regexp sre = Simply.Simplify(re);
    return BuildNormalizedRegex(re);
  }
}
