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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc.AST;

namespace tsqlc.Util
{
  public interface ITreeVisitor
  {
    void Visit(UnaryExpression unaryExpression);
    void Visit(GroupedExpression groupedExpression);
    void Visit(NullExpression nullExpression);
    void Visit(ConstantExpression constantExpression);
    void Visit(SelectStatementExpression selectStatementExpression);
    void Visit(ReferenceExpression referenceExpression);
    void Visit(FunctionCallExpression functionCallExpression);
    void Visit(BinaryOperationExpression binaryOperationExpression);
    void Visit(BooleanInSubqueryExpression booleanInSubqueryExpression);
    void Visit(BooleanNotExpresison booleanNotExpresison);
    void Visit(BooleanInListExpression booleanInListExpression);
    void Visit(BooleanBetweenExpression booleanBetweenExpression);
    void Visit(GroupedBooleanExpression groupedBooleanExpression);
    void Visit(BooleanRangeExpression booleanRangeExpression);
    void Visit(BooleanExistsExpression booleanExistsExpression);
    void Visit(BooleanNullCheckExpression booleanNullCheckExpression);
    void Visit(BooleanComparisonExpression booleanComparisonExpression);
    void Visit(BooleanBinaryExpression booleanBinaryExpression);
    void Visit(SetExpressionColumn setExpressionColumn);
    void Visit(StarColumn starColumn);
    void Visit(ExpressionColumn expressionColumn);
    void Visit(ReferenceFrom referenceFrom);
    void Visit(SubqueryFrom subqueryFrom);
    void Visit(WhileStatement whileStatement);
    void Visit(IfStatement ifStatement);
    void Visit(BlockStatement blockStatement);
    void Visit(UpdateStatement updateStatement);
    void Visit(SelectStatement selectStatement);
    void Visit(SelectInsertStatement selectInsertStatement);
    void Visit(ValuesInsertStatement valuesInsertStatement);
    void Visit(EmptyStatement emptyStatement);
    void Visit(DeleteStatement deleteStatement);
    void Visit(ValuesRow valuesRow);
    void Visit(Values values);
  }

  public interface ITreeVisitable
  {
    void Accept(ITreeVisitor visitor);
  }

  public class TreeVisitor : ITreeVisitor
  {
    private readonly ITreeVisitor _innerVisitor;

    public TreeVisitor()
    {
      _innerVisitor = null;
    }

    public TreeVisitor(ITreeVisitor visitor)
    {
      _innerVisitor = visitor;
    }

    public void Visit(UnaryExpression unaryExpression)
    {
      if (_innerVisitor != null)
        unaryExpression.Accept(_innerVisitor);
      unaryExpression.Right.Accept(this);
    }

    public void Visit(GroupedExpression groupedExpression)
    {
      if (_innerVisitor != null)
        groupedExpression.Accept(_innerVisitor);
      groupedExpression.Group.Accept(this);
    }

    public void Visit(NullExpression nullExpression)
    {
      if (_innerVisitor != null)
        nullExpression.Accept(_innerVisitor);
    }

    public void Visit(ConstantExpression constantExpression)
    {
      if (_innerVisitor != null)
        constantExpression.Accept(_innerVisitor);
    }

    public void Visit(SelectStatementExpression selectStatementExpression)
    {
      if (_innerVisitor != null)
        selectStatementExpression.Accept(_innerVisitor);
      selectStatementExpression.Statement.Accept(this);
    }

    public void Visit(ReferenceExpression referenceExpression)
    {
      if (_innerVisitor != null)
        referenceExpression.Accept(_innerVisitor);
    }

    public void Visit(FunctionCallExpression functionCallExpression)
    {
      if (_innerVisitor != null)
        functionCallExpression.Accept(_innerVisitor);
      foreach (var p in functionCallExpression.Parameters)
        p.Accept(this);
    }

    public void Visit(BinaryOperationExpression binaryOperationExpression)
    {
      if (_innerVisitor != null)
        binaryOperationExpression.Accept(_innerVisitor);
      binaryOperationExpression.Left.Accept(this);
      binaryOperationExpression.Right.Accept(this);
    }

    public void Visit(BooleanInSubqueryExpression booleanInSubqueryExpression)
    {
      if (_innerVisitor != null)
        booleanInSubqueryExpression.Accept(_innerVisitor);
      booleanInSubqueryExpression.Left.Accept(this);
      booleanInSubqueryExpression.Subquery.Accept(this);
    }

    public void Visit(BooleanNotExpresison booleanNotExpresison)
    {
      if (_innerVisitor != null)
        booleanNotExpresison.Accept(_innerVisitor);
      booleanNotExpresison.Right.Accept(this);
    }

    public void Visit(BooleanInListExpression booleanInListExpression)
    {
      if (_innerVisitor != null)
        booleanInListExpression.Accept(_innerVisitor);
      booleanInListExpression.Left.Accept(this);
      foreach (var exp in booleanInListExpression.List)
        exp.Accept(this);
    }

    public void Visit(BooleanBetweenExpression booleanBetweenExpression)
    {
      if (_innerVisitor != null)
        booleanBetweenExpression.Accept(_innerVisitor);
      booleanBetweenExpression.Left.Accept(this);
      booleanBetweenExpression.First.Accept(this);
      booleanBetweenExpression.Second.Accept(this);
    }

    public void Visit(GroupedBooleanExpression groupedBooleanExpression)
    {
      if (_innerVisitor != null)
        groupedBooleanExpression.Accept(_innerVisitor);
      groupedBooleanExpression.Group.Accept(this);
    }

    public void Visit(BooleanRangeExpression booleanRangeExpression)
    {
      if (_innerVisitor != null)
        booleanRangeExpression.Accept(_innerVisitor);
      booleanRangeExpression.Left.Accept(this);
      booleanRangeExpression.Subquery.Accept(this);
    }

    public void Visit(BooleanExistsExpression booleanExistsExpression)
    {
      if (_innerVisitor != null)
        booleanExistsExpression.Accept(_innerVisitor);
      booleanExistsExpression.Subquery.Accept(this);
    }

    public void Visit(BooleanNullCheckExpression booleanNullCheckExpression)
    {
      if (_innerVisitor != null)
        booleanNullCheckExpression.Accept(_innerVisitor);
      booleanNullCheckExpression.Left.Accept(this);
    }

    public void Visit(BooleanComparisonExpression booleanComparisonExpression)
    {
      if (_innerVisitor != null)
        booleanComparisonExpression.Accept(_innerVisitor);
      booleanComparisonExpression.Left.Accept(this);
      booleanComparisonExpression.Right.Accept(this);
    }

    public void Visit(BooleanBinaryExpression booleanBinaryExpression)
    {
      if (_innerVisitor != null)
        booleanBinaryExpression.Accept(_innerVisitor);
      booleanBinaryExpression.Left.Accept(this);
      booleanBinaryExpression.Right.Accept(this);
    }

    public void Visit(SetExpressionColumn setExpressionColumn)
    {
      if (_innerVisitor != null)
        setExpressionColumn.Accept(_innerVisitor);
      setExpressionColumn.Reference.Accept(this);
      setExpressionColumn.Expression.Accept(this);
    }

    public void Visit(StarColumn starColumn)
    {
      if (_innerVisitor != null)
        starColumn.Accept(_innerVisitor);
    }

    public void Visit(ExpressionColumn expressionColumn)
    {
      if (_innerVisitor != null)
        expressionColumn.Accept(_innerVisitor);
      expressionColumn.Expression.Accept(this);
    }

    public void Visit(ReferenceFrom referenceFrom)
    {
      if (_innerVisitor != null)
        referenceFrom.Accept(_innerVisitor);
    }

    public void Visit(SubqueryFrom subqueryFrom)
    {
      if (_innerVisitor != null)
        subqueryFrom.Accept(_innerVisitor);
      subqueryFrom.Subquery.Accept(this);
      subqueryFrom.OnClause.Accept(this);
    }

    public void Visit(WhileStatement whileStatement)
    {
      if (_innerVisitor != null)
        whileStatement.Accept(_innerVisitor);
      whileStatement.Test.Accept(this);
      whileStatement.Body.Accept(this);
    }

    public void Visit(IfStatement ifStatement)
    {
      if (_innerVisitor != null)
        ifStatement.Accept(_innerVisitor);
      ifStatement.Test.Accept(this);
      ifStatement.TrueBody.Accept(this);
      if (ifStatement.FalseBody != null)
        ifStatement.FalseBody.Accept(this);
    }

    public void Visit(BlockStatement blockStatement)
    {
      if (_innerVisitor != null)
        blockStatement.Accept(_innerVisitor);
      foreach (var statement in blockStatement.Body)
        statement.Accept(this);
    }

    public void Visit(UpdateStatement updateStatement)
    {
      if (_innerVisitor != null)
        updateStatement.Accept(_innerVisitor);
      if (updateStatement.TopExpression != null)
        updateStatement.TopExpression.Accept(this);

      updateStatement.Target.Accept(this);

      if (updateStatement.SetColumnList != null)
        foreach (var column in updateStatement.SetColumnList)
          column.Accept(this);

      if (updateStatement.FromList != null)
        foreach (var from in updateStatement.FromList)
          from.Accept(this);

      if (updateStatement.WhereClause != null)
        updateStatement.WhereClause.Accept(this);
    }

    public void Visit(SelectStatement selectStatement)
    {
      if (_innerVisitor != null)
        selectStatement.Accept(_innerVisitor);
      if (selectStatement.TopExpression != null)
        selectStatement.TopExpression.Accept(this);

      if (selectStatement.ColumnList != null)
        foreach (var column in selectStatement.ColumnList)
          column.Accept(this);

      if (selectStatement.FromList != null)
        foreach (var from in selectStatement.FromList)
          from.Accept(this);

      if (selectStatement.WhereClause != null)
        selectStatement.WhereClause.Accept(this);
    }

    public void Visit(SelectInsertStatement selectInsertStatement)
    {
      if (_innerVisitor != null)
        selectInsertStatement.Accept(_innerVisitor);
      if (selectInsertStatement.TopExpression != null)
        selectInsertStatement.TopExpression.Accept(this);

      selectInsertStatement.Target.Accept(this);

      if (selectInsertStatement.ColumnSpecification != null)
        foreach (var column in selectInsertStatement.ColumnSpecification)
          column.Accept(this);

      selectInsertStatement.SelectStatement.Accept(this);
    }

    public void Visit(ValuesInsertStatement valuesInsertStatement)
    {
      if (_innerVisitor != null)
        valuesInsertStatement.Accept(_innerVisitor);
      if (valuesInsertStatement.TopExpression != null)
        valuesInsertStatement.TopExpression.Accept(this);

      valuesInsertStatement.Target.Accept(this);

      if (valuesInsertStatement.ColumnSpecification != null)
        foreach (var column in valuesInsertStatement.ColumnSpecification)
          column.Accept(this);

      valuesInsertStatement.Values.Accept(this);
    }

    public void Visit(EmptyStatement emptyStatement)
    {
      if (_innerVisitor != null)
        emptyStatement.Accept(_innerVisitor);
    }

    public void Visit(DeleteStatement deleteStatement)
    {
      if (_innerVisitor != null)
        deleteStatement.Accept(_innerVisitor);
      if (deleteStatement.TopExpression != null)
        deleteStatement.TopExpression.Accept(this);

      deleteStatement.Target.Accept(this);

      if (deleteStatement.FromList != null)
        foreach (var from in deleteStatement.FromList)
          from.Accept(this);

      if (deleteStatement.WhereClause != null)
        deleteStatement.WhereClause.Accept(this);
    }

    public void Visit(ValuesRow valuesRow)
    {
      if (_innerVisitor != null)
        valuesRow.Accept(_innerVisitor);
      foreach (var values in valuesRow.Expressions)
        values.Accept(this);
    }

    public void Visit(Values values)
    {
      if (_innerVisitor != null)
        values.Accept(_innerVisitor);
    }
  }
}
