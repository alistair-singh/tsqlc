using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

      using(Benchmark.Start("*"))
      {
        try
        {
          //TODO: Implement
        }
        catch(Exception e)
        {
          using (ConsoleUtil.Color(ConsoleColor.Red))
            Console.WriteLine(e.Message);
        }
      }
      Console.WriteLine();
      Console.ReadKey(true);
    }
  }
}
