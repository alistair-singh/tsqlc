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
}
