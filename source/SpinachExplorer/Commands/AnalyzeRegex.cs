namespace SpinachExplorer;

using Spinach.Regex.Analyzers;

internal static partial class Program
{
  private static void AnalyzeRegex()
  {
    Console.WriteLine();
    Console.Write("Enter regex: ");
    string regex = Console.ReadLine();

    // var tqb = new TrigramsQueryBuilder();
    // var trigramQuery = tqb.MakeQuery(regex);
    //
    // Console.WriteLine(trigramQuery.ToQueryString());

    // var builder = new LiteralsQueryBuilder();
    // var nfa = builder.CompileToNFA(regex);
    // builder.PrintNFA(nfa);  

    // var builder = new LiteralsQueryBuilder();
    // var literalsQuery = builder.MakeQuery(regex);
    //
    // Console.WriteLine(literalsQuery.ToQueryString());    

    Spinach.Regex.Types.NormalizedRegex normalized = RegexParser.Parse(regex);

    Console.WriteLine();
    Console.WriteLine("Normalized Regex Structure");
    Console.WriteLine("--------------------------");
    RegexPrinter.Print(normalized);

    Spinach.Regex.Types.LiteralQueryNode queryNode = LiteralQueryBuilder.BuildQuery(normalized);
    Console.WriteLine();
    Console.WriteLine("Candidate Documents Query");
    Console.WriteLine("-------------------------");
    LiteralQueryBuilder.Print(queryNode);

    // RegexPrinter.Print(normalized);

    Pause();

    IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int> queryEnumerable = LiteralQueryBuilder.BuildEnumerable(TextSearchIndex, queryNode);

    try
    {
      foreach (TrigramFileInfo tfi in queryEnumerable)
      {
        long nameAddress = TextSearchIndex.InternalFileIdTree.Find(tfi.FileId);
        DiskImmutableString nameString = TextSearchIndex.DiskBlockManager.ImmutableStringFactory.LoadExisting(nameAddress);
        Console.WriteLine($"Match on FileId = {tfi.FileId} at Position {tfi.Position} : {nameString.GetValue()}");
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
    }

    Pause();
  }
}
