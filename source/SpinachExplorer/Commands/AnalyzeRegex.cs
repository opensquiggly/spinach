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

    var normalized = RegexParser.Parse(regex);

    Console.WriteLine();
    Console.WriteLine("Normalized Regex Structure");
    Console.WriteLine("--------------------------");
    RegexPrinter.Print(normalized);
    
    var queryNode = LiteralQueryBuilder.BuildQuery(normalized);
    Console.WriteLine();
    Console.WriteLine("Candidate Documents Query");
    Console.WriteLine("-------------------------");
    LiteralQueryBuilder.Print(queryNode);
    
    // RegexPrinter.Print(normalized);

    Pause();
  }
}
