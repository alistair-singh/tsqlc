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
    void visit(UpdateStatement updateStatement);
    void visit(SelectStatement selectStatement);

    void visit(SelectInsertStatement selectInsertStatement);

    void visit(ValuesInsertStatement valuesInsertStatement);

    void Visit(EmptyStatement emptyStatement);

    void Visit(DeleteStatement deleteStatement);
  }

  public interface ITreeVisitable
  {
    void Accept(ITreeVisitor visitor);
  }
}
