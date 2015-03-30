using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using tsqlc;
using tsqlc.AST;
using tsqlc.Util;

namespace Sample.Hashing
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


          using (HashAlgorithm hash = MD5.Create())
          using (var stream = new MemoryStream())
          using (var writer = new StreamWriter(stream))
          {
            using (Benchmark.Start("format"))
            {
              ast.Write(writer);
              writer.Flush();
            }

            stream.Seek(0, SeekOrigin.Begin);

            byte[] hashBytes;
            using (Benchmark.Start("hash"))
              hashBytes = hash.ComputeHash(stream);

            Console.WriteLine("Hash is '{0}'", Format(hashBytes));
          }
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

    private static string Format(byte[] hash)
    {
      var builder = new StringBuilder();
      foreach (var b in hash)
        builder.AppendFormat("{0:X2}", b);

      return builder.ToString();
    }
  }
}
