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
  public class SqlWriter
  {
    public TextWriter _writer;
    int _indentLevel = 0;

    public SqlWriter(TextWriter writer)
    {
      _writer = writer;
    }

    public void Append(Statement statement)
    {
      if (statement is SelectStatement)
      {
        Write(statement as SelectStatement);
        return;
      }

      throw new Exception(string.Format("Cannot handle type {0}", statement.GetType()));
    }

    public void AppendRange(IEnumerable<Statement> statements)
    {
      foreach (var statement in statements)
        Append(statement);
    }

    private void Write(SelectStatement statement)
    {
      Write("SELECT  ");
      if (statement.TopExpression != null)
      {
        Write("TOP ");
        Write(statement.TopExpression);
        _indentLevel += 8;
        WriteLine();
        _indentLevel -= 8;
      }

      Write(statement.ColumnList.First());
      _indentLevel += 8;
      WriteLine();
      foreach (var column in statement.ColumnList.Skip(1))
      {
        Write(",");
        Write(column);
        WriteLine();
      }
      _indentLevel -= 8;
    }

    private void Write(Column column)
    {
      if (column is StarColumn)
        Write(column as StarColumn);
      else if (column is ExpressionColumn)
        Write(column as ExpressionColumn);
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
      Write("*");
    }

    public void Write(string format, params object[] arg0)
    {
      _writer.Write(format, arg0);
    }

    public void WriteLine(string format = "", params object[] arg0)
    {
      _writer.WriteLine(format, arg0);
      _writer.Write(string.Empty.PadRight(_indentLevel));
    }

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

      if (expression.Parameters.Any())
      {
        Write(expression.Parameters.First());
        foreach (var parameter in expression.Parameters.Skip(1))
          Write(parameter);
      }
      Write(")");
    }

    public void Write(SelectStatementExpression expression)
    {
      Write("(");
      _indentLevel += 8;
      WriteLine();
      Write(expression.Statement);
      _indentLevel -= 8;
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

    #endregion
  }
}
