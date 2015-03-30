using System;
using System.Collections.Generic;
using System.Linq;
using tsqlc;
using tsqlc.AST;
using tsqlc.Util;

namespace Sample.Search
{
  class Program
  {
    public static IEnumerable<T> SoftZip<T, L, R>(IEnumerable<L> left, IEnumerable<R> right, Func<L, R, T> func)
    {
      var l = left.GetEnumerator();
      var r = right.GetEnumerator();

      var hasL = true;
      var hasR = true;
      while (!(!hasL && !hasR))
      {
        hasL = l.MoveNext();
        hasR = r.MoveNext();
        yield return func(hasL ? l.Current : default(L), hasR ? r.Current : default(R));
      }
    }

    public static bool CompareIdentifiers(IEnumerable<string> left, IEnumerable<string> right)
    {
      var result = SoftZip(left.Reverse(), right.Reverse(), (l, r) => new { l, r });
      return result.All(r => string.Equals(r.l, r.r, StringComparison.OrdinalIgnoreCase) ||
          (string.IsNullOrWhiteSpace(r.r) && r.l.Equals("dbo", StringComparison.OrdinalIgnoreCase)) ||
          (string.IsNullOrWhiteSpace(r.l) && r.r.Equals("dbo", StringComparison.OrdinalIgnoreCase)));
    }

    static void Main(string[] args)
    {
      var identifier = "tb_car";
      var type = "table";
      var context = "update,insert,delete,select".ToLower();

      //-------------------------------------------------------

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
        List<IStatement> syntaxTree;

        using (Benchmark.Start("parse"))
          syntaxTree = TSql.ParseFile("Test.sql").ToList();

        using (Benchmark.Start("analyze"))
        {
          var identifierParts = identifier.Split('.');
          foreach (var statement in syntaxTree)
          {
            if (statement is SelectStatement && context.Contains("select"))
            {
              var select = statement as SelectStatement;
              if (select.FromList != null && select.FromList.OfType<ReferenceFrom>().Any())
                foreach (var from in select.FromList.OfType<ReferenceFrom>())
                  if (CompareIdentifiers(from.Name.IdentifierParts, identifierParts))
                    Console.WriteLine("{0} found at line {1} column {2}.", "SELECT", from.Token.Line, from.Token.Column);
            }

            if (statement is DeleteStatement && context.Contains("select"))
            {
              var select = statement as DeleteStatement;
              var target = select.Target as ReferenceFrom;
              if (select.FromList != null && select.FromList.OfType<ReferenceFrom>().Any())
                foreach (var from in select.FromList.OfType<ReferenceFrom>())
                  if (CompareIdentifiers(from.Name.IdentifierParts, identifierParts))
                    if (!from.Alias.Equals(target.Name.Identifier, StringComparison.OrdinalIgnoreCase))
                      Console.WriteLine("{0} found at line {1} column {2}.", "SELECT-IN-DELETE", from.Token.Line, from.Token.Column);
            }

            if (statement is SelectInsertStatement && context.Contains("select"))
            {
              var select = (statement as SelectInsertStatement).SelectStatement;
              if (select.FromList != null && select.FromList.OfType<ReferenceFrom>().Any())
                foreach (var from in select.FromList.OfType<ReferenceFrom>())
                  if (CompareIdentifiers(from.Name.IdentifierParts, identifierParts))
                    Console.WriteLine("{0} found at line {1} column {2}.", "SELECT-IN-INSERT", from.Token.Line, from.Token.Column);
            }

            if (statement is UpdateStatement && context.Contains("select"))
            {
              var select = statement as UpdateStatement;
              var target = select.Target as ReferenceFrom;
              if (select.FromList != null && select.FromList.OfType<ReferenceFrom>().Any())
                foreach (var from in select.FromList.OfType<ReferenceFrom>())
                  if (CompareIdentifiers(from.Name.IdentifierParts, identifierParts))
                    if (!from.Alias.Equals(target.Name.Identifier, StringComparison.OrdinalIgnoreCase))
                      Console.WriteLine("{0} found at line {1} column {2}.", "SELECT-IN-UPDATE", from.Token.Line, from.Token.Column);
            }

            if (statement is DeleteStatement && context.Contains("delete"))
            {
              var delete = statement as DeleteStatement;
              var target = delete.Target as ReferenceFrom;
              if (target != null)
                if (CompareIdentifiers(target.Name.IdentifierParts, identifierParts))
                  Console.WriteLine("{0} found at line {1} column {2}.", "DELETE", target.Token.Line, target.Token.Column);

              if (delete.FromList != null && delete.FromList.OfType<ReferenceFrom>().Any())
                foreach (var from in delete.FromList.OfType<ReferenceFrom>())
                  if (CompareIdentifiers(from.Name.IdentifierParts, identifierParts))
                    if (from.Alias.Equals(target.Name.Identifier, StringComparison.OrdinalIgnoreCase))
                      Console.WriteLine("{0} found at line {1} column {2}.", "DELETE", from.Token.Line, from.Token.Column);
            }

            if (statement is UpdateStatement && context.Contains("update"))
            {
              var update = statement as UpdateStatement;
              var target = update.Target as ReferenceFrom;
              if (target != null)
                if (CompareIdentifiers(target.Name.IdentifierParts, identifierParts))
                  Console.WriteLine("{0} found at line {1} column {2}.", "UPDATE", target.Token.Line, target.Token.Column);

              if (update.FromList != null && update.FromList.OfType<ReferenceFrom>().Any())
                foreach (var from in update.FromList.OfType<ReferenceFrom>())
                  if (CompareIdentifiers(from.Name.IdentifierParts, identifierParts))
                    if (from.Alias.Equals(target.Name.Identifier, StringComparison.OrdinalIgnoreCase))
                      Console.WriteLine("{0} found at line {1} column {2}.", "UPDATE", from.Token.Line, from.Token.Column);
            }

            if (statement is IInsertStatement && context.Contains("insert"))
            {
              var insert = statement as IInsertStatement;
              var target = insert.Target as ReferenceFrom;
              if (target != null)
                if (CompareIdentifiers(target.Name.IdentifierParts, identifierParts))
                  Console.WriteLine("{0} found at line {1} column {2}.", "INSERT", target.Token.Line, target.Token.Column);
            }
          }
        }
      }
      Console.WriteLine("Press any key to quit...");
      Console.ReadKey(true);
    }
  }
}