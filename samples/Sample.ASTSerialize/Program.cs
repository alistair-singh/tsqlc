using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc;
using tsqlc.AST;
using tsqlc.Util;

namespace Sample.ASTSerialize
{
  public class Program
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

          string json;
          using (Benchmark.Start("serialize"))
            json = (JsonConvert.SerializeObject(ast, Formatting.Indented,
                    new JsonConverter[] { new StringEnumConverter() }));

          using (Benchmark.Start("write"))
            File.WriteAllText("ast.out.json", json);
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
  }
}
