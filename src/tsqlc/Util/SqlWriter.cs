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
  public class SqlWriter : ITreeVisitor
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

    #region Statements

    public void Visit(BlockStatement statement)
    {
      Write("BEGIN");
      WriteLine();

      using (Indent(2))
        statement.Body.DoBetween(n => n.Accept(this), (p, n) =>
        {
          WriteLine();
          WriteLine();
        });

      WriteLine();
      Write("END");
    }

    public void Visit(WhileStatement statement)
    {
      Write("WHILE ");
      Write(statement.Test);
      using (Indent(2))
      {
        WriteLine();
        statement.Body.Accept(this);
      }
    }

    public void Visit(DeleteStatement statement)
    {
      Write("DELETE  ");
      WriteTop(statement.TopExpression);

      using (Indent(8))
        statement.Target.Accept(this);

      Write(statement.FromList);
      WriteWhereClause(statement.WhereClause);
    }

    public void Visit(UpdateStatement statement)
    {
      Write("UPDATE  ");
      WriteTop(statement.TopExpression);

      using (Indent(8))
        statement.Target.Accept(this);

      WriteLine();
      Write("SET     ");
      using (Indent(8))
        WriteColumnList(statement.SetColumnList);

      Write(statement.FromList);
      WriteWhereClause(statement.WhereClause);
    }

    public void Visit(SelectInsertStatement statement)
    {
      WriteInsert(statement);
      statement.SelectStatement.Accept(this);
    }

    public void Visit(ValuesInsertStatement statement)
    {
      WriteInsert(statement);
      Write("VALUES  ");
      using (Indent(8))
        Visit(statement.Values);
    }

    public void Visit(IfStatement statement)
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

    public void Visit(SelectStatement statement)
    {
      Write("SELECT  ");
      WriteTop(statement.TopExpression);
      WriteColumnList(statement.ColumnList);
      Write(statement.FromList);
      WriteWhereClause(statement.WhereClause);

      if (statement.HasTerminator)
        Write(";");
    }

    public void Visit(EmptyStatement statement)
    {
      if (statement.HasTerminator)
        Write(";");
    }

    public void Visit(Values values)
    {
      values.Rows.DoBetween(Visit, (n, p) =>
      {
        WriteLine();
        Write(",");
      });
    }

    public void Visit(ValuesRow row)
    {
      Write("(");
      row.Expressions.DoBetween(Visit, (n, p) => Write(","));
      Write(")");
    }

    private void WriteInsert(IInsertStatement statement)
    {
      Write("INSERT  ");
      WriteTop(statement.TopExpression);

      using (Indent(8))
        statement.Target.Accept(this);

      WriteColumnSpecification(statement.ColumnSpecification);
      WriteLine();
    }

    private void WriteBody(IStatement statement)
    {
      if (statement is BlockStatement)
        statement.Accept(this);
      else
        using (Indent(2))
          statement.Accept(this);
    }

    private void WriteTop(IExpression expression)
    {
      if (expression == null)
        return;

      Write("TOP ");
      Visit(expression);
      WriteLine();
    }

    private void WriteWhereClause(IBooleanExpression expression)
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
        columns.DoBetween(Write, (n, p) =>
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
          columns.DoBetween(Visit, (n, p) =>
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

    public void Visit(ExpressionColumn column)
    {
      Visit(column.Expression);
      if (!string.IsNullOrWhiteSpace(column.Alias))
        Write(" AS {0}", column.Alias);
    }

    public void Visit(StarColumn column)
    {
      if (!string.IsNullOrWhiteSpace(column.TableAlias))
        Write("{0}.", column.TableAlias);
      Write("*");
    }

    public void Visit(SetExpressionColumn column)
    {
      Write(column.Reference.Identifier);
      Write(" = ");
      Visit(column.Expression);
    }

    private void Write(IColumn column)
    {
      column.Accept(this);
    }

    #endregion

    #region Froms

    public void Visit(SubqueryFrom from)
    {
      Write(GetJoinType(from.Join));
      Write("(");
      using (Indent(2))
      {
        WriteLine();
        from.Subquery.Accept(this);
      }
      WriteLine();
      Write(")");
      WriteOnClause(from.OnClause);
    }

    public void Visit(ReferenceFrom from)
    {
      Write(GetJoinType(from.Join));
      Visit(from.Name);
      WriteAlias(from.Alias);
      WriteTableHints(from.Hints);
      WriteOnClause(from.OnClause);
    }

    private void Write(ICollection<IFrom> froms)
    {
      if (froms == null || !froms.Any())
        return;
      WriteLine();
      Write("FROM    ");
      using (Indent(8))
        froms.DoBetween(n => n.Accept(this), (n, p) => WriteLine());
    }

    private void WriteTableHints(ICollection<TableHint> hints)
    {
      if (hints == null || !hints.Any())
        return;
      Write(" WITH (");
      Write(string.Join(", ", hints));
      Write(")");
    }

    private void WriteOnClause(IBooleanExpression expression)
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

    public void Visit(GroupedBooleanExpression expression)
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

    public void Visit(BooleanBetweenExpression expression)
    {
      Visit(expression.Left);
      Write(" BETWEEN ");
      Visit(expression.First);
      Write(" AND ");
      Visit(expression.Second);
    }

    public void Visit(BooleanBinaryExpression expression)
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

    public void Visit(BooleanExistsExpression expression)
    {
      Write("EXISTS (");
      WriteLine();
      using (Indent(2))
        expression.Subquery.Accept(this);
      WriteLine();
      Write(")");
    }

    public void Visit(BooleanInListExpression expression)
    {
      Visit(expression.Left);
      Write(expression.Not ? " NOT IN (" : " IN (");
      WriteLine();

      using (Indent(2))
        expression.List.DoBetween(Visit, (n, p) =>
        {
          WriteLine();
          Write(",");
        });

      WriteLine();
      Write(")");
    }

    public void Visit(BooleanInSubqueryExpression expression)
    {
      Visit(expression.Left);
      Write(expression.Not ? " NOT IN (" : " IN (");
      WriteLine();
      using (Indent(2))
        Visit(expression.Subquery);
      WriteLine();
      Write(")");
    }

    public void Visit(BooleanNotExpresison expression)
    {
      Write("NOT ");
      Write(expression.Right);
    }

    public void Visit(BooleanRangeExpression expression)
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

      Visit(expression.Left);
      Write(op);
      Write(GetBooleanOperator(expression.Type));
      Write("(");
      WriteLine();
      using (Indent(2))
        Visit(expression.Subquery);
      WriteLine();
      Write(")");
    }

    public void Visit(BooleanComparisonExpression expression)
    {
      Visit(expression.Left);
      Write(GetBooleanOperator(expression.Type));
      Visit(expression.Right);
    }

    public void Visit(BooleanNullCheckExpression expression)
    {
      Visit(expression.Left);
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

    private void Write(IBooleanExpression expression)
    {
      expression.Accept(this);
    }

    #endregion

    #region Expression

    public void Visit(GroupedExpression expression)
    {
      Write("(");
      Visit(expression.Group);
      Write(")");
    }

    public void Visit(BinaryOperationExpression expression)
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
      Visit(expression.Left);
      Write(op);
      Visit(expression.Right);
    }

    public void Visit(ReferenceExpression expression)
    {
      Write(expression.Identifier);
    }

    public void Visit(UnaryExpression expression)
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
      Visit(expression.Right);
    }

    public void Visit(FunctionCallExpression expression)
    {
      Visit(expression.Function);
      Write("(");
      expression.Parameters.DoBetween(Visit, (n, p) => Write(", "));
      Write(")");
    }

    public void Visit(SelectStatementExpression expression)
    {
      Write("(");
      WriteLine();
      using (Indent(2))
        expression.Statement.Accept(this);
      WriteLine();
      Write(")");
    }

    public void Visit(ConstantExpression expression)
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

    public void Visit(NullExpression expression)
    {
      Write("NULL");
    }

    public void Visit(IExpression expression)
    {
      expression.Accept(this);
    }

    #endregion

    public void WriteLine()
    {
      _writer.WriteLine();
      _isNewLine = true;
    }

    private void Write(string format, params object[] arg0)
    {
      if (_isNewLine)
      {
        _isNewLine = false;
        _writer.Write(string.Empty.PadLeft(_indentLevel));
      }
      _writer.Write(format, arg0);
    }

    private IDisposable Indent(int numOfSpaces)
    {
      _indentLevel += numOfSpaces;
      return Disposable.From(() => _indentLevel -= numOfSpaces);
    }
  }
}