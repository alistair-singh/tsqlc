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

          using (Benchmark.Start("analyze"))
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
          Test(select.FromList.OfType<ReferenceFrom>(), select.WhereClause);
        }
        if (statement is SelectInsertStatement)
        {
          var select = (statement as SelectInsertStatement).SelectStatement;
          Test(select.FromList.OfType<ReferenceFrom>(), select.WhereClause);
        }
        if (statement is DeleteStatement)
        {
          var select = statement as DeleteStatement;
          Test(select.FromList.OfType<ReferenceFrom>(), select.WhereClause);
        }
      }
    }

    private static void Test(IEnumerable<ReferenceFrom> from, IBooleanExpression whereClause)
    {
      if (from == null || !from.Any())
        return;

      var leftJoinAlias = from
        .OfType<ReferenceFrom>()
        .Where(x => x.Join == JoinType.LEFT)
        .Select(x => x.Alias ?? x.Name.Identifier)
        .ToList();

      TreeVisitor visitor = new TreeVisitor(new AnalyzeVisitor(x =>
      {
        if (leftJoinAlias.Any(y => x.Identifier.StartsWith(y)))
        {
          Console.WriteLine("Where clause excluding left join rows '{0}', line {1} col {2}",
            x.Identifier, x.Token.Line, x.Token.Column);
        }
      }));
      if (whereClause != null)
        whereClause.Accept(visitor);

    }

    class AnalyzeVisitor : ITreeVisitor
    {
      private Action<ReferenceExpression> _action;
      public AnalyzeVisitor(Action<ReferenceExpression> action)
      {
        _action = action;
      }

      public void Visit(UnaryExpression unaryExpression) { }
      public void Visit(GroupedExpression groupedExpression) { }
      public void Visit(NullExpression nullExpression) { }
      public void Visit(ConstantExpression constantExpression) { }
      public void Visit(SelectStatementExpression selectStatementExpression) { }
      public void Visit(ReferenceExpression referenceExpression) { _action(referenceExpression); }
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
      public void Visit(UpdateStatement updateStatement) { }
      public void Visit(SelectStatement selectStatement) { }
      public void Visit(SelectInsertStatement selectInsertStatement) { }
      public void Visit(ValuesInsertStatement valuesInsertStatement) { }
      public void Visit(EmptyStatement emptyStatement) { }
      public void Visit(DeleteStatement deleteStatement) { }
      public void Visit(ValuesRow valuesRow) { }
      public void Visit(Values values) { }
    }
  }
}