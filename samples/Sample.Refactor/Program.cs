using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc;
using tsqlc.AST;
using tsqlc.Util;

namespace Sample.Refactor
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


          using (Benchmark.Start("refactor"))
            ast.Visit(new TreeVisitor(new RefactorVisitor()));

          using (Benchmark.Start("write"))
          using (ConsoleUtil.Color(ConsoleColor.Yellow))
            ast.Write(Console.Out);
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

    public class RefactorVisitor : ITreeVisitor
    {
      public void Visit(UnaryExpression unaryExpression) { }
      public void Visit(GroupedExpression groupedExpression) { }
      public void Visit(NullExpression nullExpression) { }
      public void Visit(ConstantExpression constantExpression) { }
      public void Visit(SelectStatementExpression selectStatementExpression) { }
      public void Visit(ReferenceExpression referenceExpression) { }
      public void Visit(FunctionCallExpression functionCallExpression) { }
      public void Visit(BinaryOperationExpression binaryOperationExpression) { }
      public void Visit(BooleanInSubqueryExpression booleanInSubqueryExpression) { }
      public void Visit(BooleanNotExpresison booleanNotExpresison) { }
      public void Visit(BooleanInListExpression booleanInListExpression) { }
      public void Visit(BooleanBetweenExpression booleanBetweenExpression) { }
      public void Visit(GroupedBooleanExpression groupedBooleanExpression) { }
      public void Visit(BooleanRangeExpression booleanRangeExpression) { }
      public void Visit(BooleanExistsExpression booleanExistsExpression) { }
      public void Visit(BooleanNullCheckExpression booleanNullCheckExpression) { }
      public void Visit(BooleanComparisonExpression booleanComparisonExpression) { }
      public void Visit(BooleanBinaryExpression booleanBinaryExpression) { }
      public void Visit(SetExpressionColumn setExpressionColumn) { }
      public void Visit(StarColumn starColumn) { }
      public void Visit(ExpressionColumn expressionColumn) { }
      public void Visit(ReferenceFrom referenceFrom) { }
      public void Visit(SubqueryFrom subqueryFrom) { }
      public void Visit(WhileStatement whileStatement) { }
      public void Visit(IfStatement ifStatement) { }
      public void Visit(BlockStatement blockStatement) { }

      public void Visit(UpdateStatement updateStatement)
      {
        var froms = (updateStatement.FromList ?? Enumerable.Empty<IFrom>()).OfType<ReferenceFrom>();

        foreach (var from in froms)
        {
          if (from.Name.IdentifierParts.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 1)
          {
            from.Name.IdentifierParts = new List<string>() { "dbo", from.Name.IdentifierParts.Last() };
          }
        }

        var target = updateStatement.Target as ReferenceFrom;
        if (target == null)
          return;

        if (!froms.Any(x => (x.Alias ?? x.Name.Identifier) == target.Name.Identifier))
        {
          target.Name.IdentifierParts = new List<string>() { "dbo", target.Name.IdentifierParts.Last() };
        }
      }

      public void Visit(SelectStatement selectStatement)
      {
        var froms = selectStatement.FromList;
        if (froms == null)
          return;
        foreach (var from in froms.OfType<ReferenceFrom>())
        {
          if (from.Name.IdentifierParts.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 1)
          {
            from.Name.IdentifierParts = new List<string>() { "dbo", from.Name.IdentifierParts.Last() };
          }
        }
      }

      public void Visit(DeleteStatement deleteStatement)
      {
        var froms = (deleteStatement.FromList ?? Enumerable.Empty<IFrom>()).OfType<ReferenceFrom>();

        foreach (var from in froms)
        {
          if (from.Name.IdentifierParts.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 1)
          {
            from.Name.IdentifierParts = new List<string>() { "dbo", from.Name.IdentifierParts.Last() };
          }
        }

        var target = deleteStatement.Target as ReferenceFrom;
        if (target == null)
          return;

        if (!froms.Any(x => (x.Alias ?? x.Name.Identifier) == target.Name.Identifier))
        {
          target.Name.IdentifierParts = new List<string>() { "dbo", target.Name.IdentifierParts.Last() };
        }
      }

      public void Visit(SelectInsertStatement selectInsertStatement)
      {
        var from = selectInsertStatement.Target as ReferenceFrom;
        if (from == null)
          return;
        if (from.Name.IdentifierParts.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 1)
        {
          from.Name.IdentifierParts = new List<string>() { "dbo", from.Name.IdentifierParts.Last() };
        }
      }

      public void Visit(ValuesInsertStatement valuesInsertStatement)
      {
        var from = valuesInsertStatement.Target as ReferenceFrom;
        if (from == null)
          return;
        if (from.Name.IdentifierParts.Where(x => !string.IsNullOrWhiteSpace(x)).Count() == 1)
        {
          from.Name.IdentifierParts = new List<string>() { "dbo", from.Name.IdentifierParts.Last() };
        }
      }

      public void Visit(EmptyStatement emptyStatement) { }
      public void Visit(ValuesRow valuesRow) { }
      public void Visit(Values values) { }
    }
  }
}
