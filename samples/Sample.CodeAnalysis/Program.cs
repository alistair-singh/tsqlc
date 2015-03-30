using System;
using System.Collections.Generic;
using System.Linq;
using tsqlc;
using tsqlc.AST;
using tsqlc.Util;

namespace Sample.CodeAnalysis
{
  class Program
  {
    static void Main(string[] args)
    {
      Benchmark.Begin += (name, id) =>
      {
        using (ConsoleUtil.Color(fg: ConsoleColor.Green))
          Console.WriteLine("{0}:{1}->{2}", id, name, DateTime.Now);
      };

      Benchmark.End += (name, id, elapse) =>
      {
        using (ConsoleUtil.Color(fg: ConsoleColor.Green))
          Console.WriteLine("{0}:{1}->{2}ms, {3}", id, name, elapse, DateTime.Now);
      };

      using (Benchmark.Start("*"))
      {
        try
        {
          List<IStatement> ast;
          using (Benchmark.Start("parse"))
            ast = TSql.ParseFile("Test.sql").ToList();

          Analyze(ast);
        }
        catch (Exception e)
        {
          using (ConsoleUtil.Color(ConsoleColor.Red))
            Console.WriteLine(e.Message);
        }
      }

      Console.WriteLine("Press any key to quit...");
      Console.ReadKey(true);
    }

    private static void Analyze(IEnumerable<IStatement> ast)
    {
      foreach (var statement in ast)
      {
        if (statement is SelectStatement)
        {
          var select = statement as SelectStatement;
          if (select.FromList != null && select.FromList.Any())
          {
            var leftJoinAlias = select.FromList
              .OfType<ReferenceFrom>()
              .Where(x => x.Join == JoinType.LEFT)
              .Select(x => x.Alias ?? x.Name.Identifier)
              .ToList();
          }
        }
      }
    }
  }
}