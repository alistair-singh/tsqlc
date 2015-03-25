/*
Copyright (c) 2015, Alistair Singh
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc.AST;

namespace tsqlc.Util
{
  //TODO: Implement with visitor pattern
  public class SqlWriter
  {
    private TextWriter _writer;
    private int _indentLevel;
    private bool _isNewLine;

    public SqlWriter(TextWriter writer)
    {
      _writer = writer;
      _indentLevel = 0;
      _isNewLine = true;
    }

    public void Append(Statement statement)
    {
      Write(statement);
      WriteLine();
      WriteLine();
    }

    public void AppendRange(IEnumerable<Statement> statements)
    {
      foreach (var statement in statements)
        Append(statement);
    }

    #region Statements

    private void Write(Statement statement)
    {
      if (statement is SelectStatement)
        Write(statement as SelectStatement);
      else if (statement is IfStatement)
        Write(statement as IfStatement);
      else if (statement is BlockStatement)
        Write(statement as BlockStatement);
      else if (statement is DeleteStatement)
        Write(statement as DeleteStatement);
      else if (statement is InsertStatement)
        Write(statement as InsertStatement);
      else if (statement is UpdateStatement)
        Write(statement as UpdateStatement);
      else if (statement is WhileStatement)
        Write(statement as WhileStatement);
      else
        throw new Exception(string.Format("Cannot handle type {0}", statement.GetType()));
    }

    private void Write(BlockStatement statement)
    {
      Write("BEGIN");
      WriteLine();

      using (Indent(2))
        DoBetween(statement.Body, Write, (p, n) =>
        {
          WriteLine();
          WriteLine();
        });

      WriteLine();
      Write("END");
    }

    private void Write(WhileStatement statement)
    {
      Write("WHILE ");
      Write(statement.Test);
      using (Indent(2))
      {
        WriteLine();
        Write(statement.Body);
      }
    }

    private void Write(DeleteStatement statement)
    {
      Write("DELETE  ");
      WriteTop(statement.TopExpression);

      using (Indent(8))
        Write(statement.Target);

      Write(statement.FromList);
      WriteWhereClause(statement.WhereClause);
    }

    private void Write(UpdateStatement statement)
    {
      Write("UPDATE  ");
      WriteTop(statement.TopExpression);

      using (Indent(8))
        Write(statement.Target);

      WriteLine();
      Write("SET     ");
      using (Indent(8))
        WriteColumnList(statement.SetColumnList);

      Write(statement.FromList);
      WriteWhereClause(statement.WhereClause);
    }

    private void Write(InsertStatement statement)
    {
      Write("INSERT  ");
      WriteTop(statement.TopExpression);

      using (Indent(8))
        Write(statement.Target);

      WriteColumnSpecification(statement.ColumnSpecification);
      WriteLine();

      if (statement is SelectInsertStatement)
        Write(statement as SelectInsertStatement);
      else if (statement is ValuesInsertStatement)
        Write(statement as ValuesInsertStatement);
      else
        throw new NotSupportedException();
    }

    private void Write(SelectInsertStatement statement)
    {
      Write(statement.SelectStatement);
    }

    private void Write(ValuesInsertStatement statement)
    {
      Write("VALUES  ");
      using (Indent(8))
        Write(statement.Values);
    }

    private void Write(IfStatement statement)
    {
      Write("IF ");
      Write(statement.Test);
      WriteLine();
      WriteBody(statement.TrueBody);
      WriteLine();
      if (statement.FalseBody != null)
      {
        Write("ELSE ");
        WriteLine();
        WriteBody(statement.FalseBody);
      }
    }

    private void Write(SelectStatement statement)
    {
      Write("SELECT  ");
      WriteTop(statement.TopExpression);
      WriteColumnList(statement.ColumnList);
      Write(statement.FromList);
      WriteWhereClause(statement.WhereClause);

      if (statement.HasTerminator)
        Write(";");
    }

    private void Write(Values values)
    {
      DoBetween(values.Rows, Write, (n, p) =>
      {
        WriteLine();
        Write(",");
      });
    }

    private void Write(ValuesRow row)
    {
      Write("(");
      DoBetween(row.Expressions, Write, (n, p) => Write(","));
      Write(")");
    }

    private void WriteBody(Statement statement)
    {
      if (statement is BlockStatement)
        Write(statement);
      else
        using (Indent(2))
          Write(statement);
    }

    private void WriteTop(Expression expression)
    {
      if (expression == null)
        return;

      Write("TOP ");
      Write(expression);
      WriteLine();
    }

    private void WriteWhereClause(BooleanExpression expression)
    {
      if (expression == null)
        return;

      WriteLine();
      Write("WHERE   ");
      using (Indent(8))
        Write(expression);
    }

    private void WriteColumnList(IEnumerable<IColumn> columns)
    {
      using (Indent(8))
        DoBetween(columns, Write, (n, p) =>
        {
          WriteLine();
          Write(",");
        });
    }

    private void WriteColumnSpecification(IEnumerable<ReferenceExpression> columns)
    {
      if (columns == null || !columns.Any())
        return;

      using (Indent(8))
      {
        Write(" (");
        WriteLine();
        using (Indent(2))
        {
          DoBetween(columns, Write, (n, p) =>
          {
            WriteLine();
            Write(",");
          });
        }
        WriteLine();
        Write(")");
      }
    }

    #endregion

    #region Columns

    private void Write(IColumn column)
    {
      if (column is StarColumn)
        Write(column as StarColumn);
      else if (column is ExpressionColumn)
        Write(column as ExpressionColumn);
      else if (column is SetExpressionColumn)
        Write(column as SetExpressionColumn);
      else
        throw new Exception(string.Format("Cannot handle type {0}", column.GetType()));
    }

    private void Write(ExpressionColumn column)
    {
      Write(column.Expression);
      if (!string.IsNullOrWhiteSpace(column.Alias))
        Write(" AS {0}", column.Alias);
    }

    private void Write(StarColumn column)
    {
      if (!string.IsNullOrWhiteSpace(column.TableAlias))
        Write("{0}.", column.TableAlias);
      Write("*");
    }

    private void Write(SetExpressionColumn column)
    {
      Write(column.Reference.Identifier);
      Write(" = ");
      Write(column.Expression);
    }

    #endregion

    #region Froms

    private void Write(ICollection<From> froms)
    {
      if (froms != null && froms.Any())
      {
        WriteLine();
        Write("FROM    ");
        using (Indent(8))
          DoBetween(froms, Write, (n, p) => WriteLine());
      }
    }

    private void Write(From from)
    {
      if (from is SubqueryFrom)
        Write(from as SubqueryFrom);
      else if (from is ReferenceFrom)
        Write(from as ReferenceFrom);
      else
        throw new NotSupportedException();
    }

    private void Write(SubqueryFrom from)
    {
      Write(GetJoinType(from.Join));
      Write("(");
      using (Indent(2))
      {
        WriteLine();
        Write(from.Subquery);
      }
      WriteLine();
      Write(")");
      WriteOnClause(from.OnClause);
    }

    private void Write(ReferenceFrom from)
    {
      Write(GetJoinType(from.Join));
      Write(from.Name);
      WriteAlias(from.Alias);
      WriteTableHints(from.Hints);
      WriteOnClause(from.OnClause);
    }

    private void WriteTableHints(ICollection<TableHint> hints)
    {
      if (hints == null || !hints.Any())
        return;
      Write(" WITH (");
      Write(string.Join(", ", hints));
      Write(")");
    }

    private void WriteOnClause(BooleanExpression expression)
    {
      if (expression == null)
        return;

      using (Indent(2))
      {
        WriteLine();
        Write("ON ");
        using (Indent(2))
          Write(expression);
      }
    }

    private void WriteAlias(string alias)
    {
      if (string.IsNullOrEmpty(alias))
        return;

      Write(" AS ");
      Write(alias);
    }

    private string GetJoinType(JoinType type)
    {
      switch (type)
      {
        case JoinType.PRIMARY:
          return string.Empty;
        case JoinType.CROSS_APPLY:
          return "CROSS APPLY ";
        case JoinType.CROSS_JOIN:
          return "CROSS JOIN ";
        case JoinType.INNER:
          return "INNER JOIN ";
        case JoinType.LEFT:
          return "LEFT JOIN ";
        case JoinType.OUTER_APPLY:
          return "OUTER APPLY ";
        case JoinType.OUTER_JOIN:
          return "FULL OUTER JOIN ";
        case JoinType.RIGHT:
          return "RIGHT JOIN ";
        default:
          throw new NotSupportedException();
      }
    }

    #endregion

    #region Boolean Expressions

    private void Write(BooleanExpression expression)
    {
      if (expression is BooleanBetweenExpression)
        Write(expression as BooleanBetweenExpression);
      else if (expression is BooleanBinaryExpression)
        Write(expression as BooleanBinaryExpression);
      else if (expression is BooleanExistsExpression)
        Write(expression as BooleanExistsExpression);
      else if (expression is BooleanInListExpression)
        Write(expression as BooleanInListExpression);
      else if (expression is BooleanInSubqueryExpression)
        Write(expression as BooleanInSubqueryExpression);
      else if (expression is BooleanNotExpresison)
        Write(expression as BooleanNotExpresison);
      else if (expression is BooleanRangeExpression)
        Write(expression as BooleanRangeExpression);
      else if (expression is BooleanComparisonExpression)
        Write(expression as BooleanComparisonExpression);
      else if (expression is BooleanNullCheckExpression)
        Write(expression as BooleanNullCheckExpression);
      else if (expression is GroupedBooleanExpression)
        Write(expression as GroupedBooleanExpression);
      else
        throw new NotSupportedException();
    }

    public void Write(GroupedBooleanExpression expression)
    {
      Write("(");
      using (Indent(2))
      {
        WriteLine();
        Write(expression.Group);
        WriteLine();
      }
      Write(")");
    }

    public void Write(BooleanBetweenExpression expression)
    {
      Write(expression.Left);
      Write(" BETWEEN ");
      Write(expression.First);
      Write(" AND ");
      Write(expression.Second);
    }

    public void Write(BooleanBinaryExpression expression)
    {
      string op;
      switch (expression.Type)
      {
        case BooleanOperatorType.And:
          op = "AND "; break;
        case BooleanOperatorType.Or:
          op = "OR "; break;
        default:
          throw new NotSupportedException();
      }
      Write(expression.Left);
      WriteLine();
      Write(op);
      Write(expression.Right);
    }

    public void Write(BooleanExistsExpression expression)
    {
      Write("EXISTS (");
      WriteLine();
      using (Indent(2))
        Write(expression.Subquery);
      WriteLine();
      Write(")");
    }

    public void Write(BooleanInListExpression expression)
    {
      Write(expression.Left);
      Write(expression.Not ? " NOT IN (" : " IN (");
      WriteLine();

      using (Indent(2))
        DoBetween(expression.List, Write, (n, p) =>
        {
          WriteLine();
          Write(",");
        });

      WriteLine();
      Write(")");
    }

    public void Write(BooleanInSubqueryExpression expression)
    {
      Write(expression.Left);
      Write(expression.Not ? " NOT IN (" : " IN (");
      WriteLine();
      using (Indent(2))
        Write(expression.Subquery);
      WriteLine();
      Write(")");
    }

    public void Write(BooleanNotExpresison expression)
    {
      Write("NOT ");
      Write(expression.Right);
    }

    public void Write(BooleanRangeExpression expression)
    {
      string op;
      switch (expression.RangeType)
      {
        case RangeOperatorType.All:
          op = " ALL"; break;
        case RangeOperatorType.Any:
          op = " ANY"; break;
        case RangeOperatorType.Some:
          op = " SOME"; break;
        default:
          throw new NotSupportedException();
      }

      Write(expression.Left);
      Write(op);
      Write(GetBooleanOperator(expression.Type));
      Write("(");
      WriteLine();
      using (Indent(2))
        Write(expression.Subquery);
      WriteLine();
      Write(")");
    }

    public void Write(BooleanComparisonExpression expression)
    {
      Write(expression.Left);
      Write(GetBooleanOperator(expression.Type));
      Write(expression.Right);
    }

    public void Write(BooleanNullCheckExpression expression)
    {
      Write(expression.Left);
      Write(expression.IsNull ? " IS NULL" : " IS NOT NULL");
    }

    private string GetBooleanOperator(BooleanOperatorType type)
    {
      switch (type)
      {
        case BooleanOperatorType.Equals:
          return " = ";
        case BooleanOperatorType.GreaterThan:
          return " > ";
        case BooleanOperatorType.GreaterThanOrEqual:
          return " >= ";
        case BooleanOperatorType.LessThan:
          return " < ";
        case BooleanOperatorType.LessThanOrEqual:
          return " <= ";
        case BooleanOperatorType.Like:
          return " LIKE ";
        case BooleanOperatorType.NotEqual:
          return " <> ";
        case BooleanOperatorType.NotGreaterThan:
          return " !> ";
        case BooleanOperatorType.NotLessThan:
          return " !< ";
        default:
          throw new NotSupportedException();
      }
    }

    #endregion

    #region Expression

    public void Write(Expression expression)
    {
      if (expression is ConstantExpression)
        Write(expression as ConstantExpression);
      else if (expression is ReferenceExpression)
        Write(expression as ReferenceExpression);
      else if (expression is BinaryOperationExpression)
        Write(expression as BinaryOperationExpression);
      else if (expression is GroupedExpression)
        Write(expression as GroupedExpression);
      else if (expression is UnaryExpression)
        Write(expression as UnaryExpression);
      else if (expression is FunctionCallExpression)
        Write(expression as FunctionCallExpression);
      else if (expression is SelectStatementExpression)
        Write(expression as SelectStatementExpression);
      else if (expression is NullExpression)
        Write(expression as NullExpression);
      else
        throw new NotSupportedException();
    }

    public void Write(GroupedExpression expression)
    {
      Write("(");
      Write(expression.Group);
      Write(")");
    }

    public void Write(BinaryOperationExpression expression)
    {
      string op;
      switch (expression.Type)
      {
        case BinaryType.Addition:
          op = " + "; break;
        case BinaryType.BitwiseAnd:
          op = " & "; break;
        case BinaryType.BitwiseOr:
          op = " | "; break;
        case BinaryType.BitwiseXor:
          op = " ^ "; break;
        case BinaryType.Division:
          op = " / "; break;
        case BinaryType.Modulus:
          op = " % "; break;
        case BinaryType.Multiply:
          op = " * "; break;
        case BinaryType.Subtraction:
          op = " - "; break;
        default:
          throw new NotSupportedException();
      }
      Write(expression.Left);
      Write(op);
      Write(expression.Right);
    }

    public void Write(ReferenceExpression expression)
    {
      Write(expression.Identifier);
    }

    public void Write(UnaryExpression expression)
    {
      string op;
      switch (expression.Type)
      {
        case UnaryType.Negative:
          op = "-"; break;
        case UnaryType.Positive:
          op = "+"; break;
        case UnaryType.BitwiseNot:
          op = "~"; break;
        default:
          throw new NotSupportedException();
      }
      Write(op);
      Write(expression.Right);
    }

    public void Write(FunctionCallExpression expression)
    {
      Write(expression.Function);
      Write("(");
      DoBetween(expression.Parameters, Write, (n, p) => Write(", "));
      Write(")");
    }

    public void Write(SelectStatementExpression expression)
    {
      Write("(");
      WriteLine();
      using (Indent(2))
        Write(expression.Statement);
      WriteLine();
      Write(")");
    }

    public void Write(ConstantExpression expression)
    {
      switch (expression.Type)
      {
        case SqlType.NVarchar:
          Write("N");
          Write("\'{0}\'", expression.Value);
          break;
        case SqlType.Varchar:
          Write("\'{0}\'", expression.Value);
          break;
        default:
          Write(expression.Value.ToString());
          break;
      }
    }

    public void Write(NullExpression expression)
    {
      Write("NULL");
    }

    #endregion

    public void Write(string format, params object[] arg0)
    {
      if (_isNewLine)
      {
        _isNewLine = false;
        _writer.Write(string.Empty.PadLeft(_indentLevel));
      }
      _writer.Write(format, arg0);
    }

    public void WriteLine()
    {
      _writer.WriteLine();
      _isNewLine = true;
    }

    private static void DoBetween<T>(IEnumerable<T> collection, Action<T> action, Action<T, T> betweenAction)
    {
      var enumerator = collection.GetEnumerator();
      if (enumerator.MoveNext())
      {
        var prev = enumerator.Current;
        action(prev);
        while (enumerator.MoveNext())
        {
          var next = enumerator.Current;
          betweenAction(prev, next);
          action(next);
          prev = next;
        }
      }
    }

    private IDisposable Indent(int numOfSpaces)
    {
      _indentLevel += numOfSpaces;
      return Disposable.From(() => _indentLevel -= numOfSpaces);
    }
  }
}