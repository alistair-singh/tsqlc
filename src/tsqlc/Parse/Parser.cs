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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using tsqlc.AST;

namespace tsqlc.Parse
{
  public class Parser : IEnumerable<Statement>
  {
    private const int ReferenceListInitializationSize = 4;

    private readonly IEnumerator<Token> _tokens;

    private Token Current { get { return _tokens.Current; } }

    public Parser(IEnumerable<Token> tokens)
    {
      // Not interested in comments
      _tokens = tokens.Where(x => x.Type != TokenType.LineComment &&
        x.Type != TokenType.BlockComment).GetEnumerator();
    }

    public Statement NextStatement()
    {
      if (Current == null && !_tokens.MoveNext())
        return null;

      switch (Current.Type)
      {
        case TokenType.K_SELECT:
          return Select();
        default:
          return new SelectStatement { Columns = new List<Column>() { new ExpressionColumn { Expression = BooleanExpression() } } };
      }
    }

    private SelectStatement Select()
    {
      Match(TokenType.K_SELECT);

      Expression top = null;
      if (Current.Type == TokenType.K_TOP)
        top = Expression();

      var columns = ColumnList();

      List<From> froms = null;
      if (Current.Type == TokenType.K_FROM)
        froms = FromList();

      ComparisonExpression whereClause = null;
      if (Current.Type == TokenType.K_WHERE)
      {
        Consume();
        var booleanExpression = BooleanExpression();
      }

      return new SelectStatement
      {
        Columns = columns,
        WhereClause = whereClause,
        FromList = froms,
        TopExpression = top
      };
    }

    private BooleanExpression BooleanExpression()
    {
      if (Current.Type == TokenType.K_NOT)
      {
        Consume();
        var booleanExpression = BooleanExpression();
        return new BooleanNotExpresison { Left = booleanExpression };
      }

      if (Current.Type == TokenType.K_EXISTS)
        return BooleanExists();

      var left = Expression();

      if (Current.Type == TokenType.K_BETWEEN)
        return BooleanBetween(left);

      if (Current.Type == TokenType.K_IN)
        return BooleanIn(left);

      if (Current.Type == TokenType.K_NOT)
      {
        Consume();
        switch (Current.Type)
        {
          case TokenType.K_IN:
            return BooleanIn(left, true);
          case TokenType.K_BETWEEN:
            return BooleanBetween(left, true);
          default:
            throw Unexpected(Current);
        }
      }

      var op = BooleanOperator();
      switch (Current.Type)
      {
        case TokenType.K_ALL:
        case TokenType.K_ANY:
        case TokenType.K_SOME:
          return BooleanRange(left, op);
      }

      var right = Expression();
      return new ComparisonExpression { Left = left, Operator = op, Right = right };
    }

    private BooleanExpression BooleanExists()
    {
      throw new NotImplementedException();
    }

    private BooleanRangeExpression BooleanRange(Expression left, BooleanBinaryType op)
    {
      var rangeOp = RangeOperator();
      Match(TokenType.OpenBracket);
      var subquery = Select();
      Match(TokenType.CloseBracket);
      return new BooleanRangeExpression
      {
        Left = left,
        BooleanOperator = op,
        RangeOperator = rangeOp,
        Subquery = subquery
      };
    }

    private RangeOperatorType RangeOperator()
    {
      RangeOperatorType type;
      switch (Current.Type)
      {
        case TokenType.K_ALL:
          type = RangeOperatorType.All;
          break;
        case TokenType.K_ANY:
          type = RangeOperatorType.Any;
          break;
        case TokenType.K_SOME:
          type = RangeOperatorType.Some;
          break;
        default:
          throw Unexpected(Current);
      }
      Consume();
      return type;
    }

    private BooleanInExpression BooleanIn(Expression left, bool not = false)
    {
      Match(TokenType.K_IN);
      Match(TokenType.OpenBracket);
      if (Current.Type == TokenType.K_SELECT)
      {
        var subquery = Select();
        Match(TokenType.CloseBracket);
        return new BooleanInSubqueryExpression { Not = not, Left = left, Subquery = subquery };
      }

      var list = new List<Expression>();
      list.Add(Expression());
      while (Current != null && Current.Type == TokenType.Comma)
      {
        Consume();
        list.Add(Expression());
      }
      Match(TokenType.CloseBracket);
      return new BooleanInListExpression { Not = not, Left = left, List = list };
    }

    private BooleanBetweenExpression BooleanBetween(Expression left, bool not = false)
    {
      Match(TokenType.K_BETWEEN);
      var first = Expression();
      Match(TokenType.K_AND);
      var second = Expression();
      return new BooleanBetweenExpression { Left = left, Not = not, First = first, Second = second };
    }

    private BooleanBinaryType BooleanOperator()
    {
      BooleanBinaryType type;
      switch (Current.Type)
      {
        case TokenType.K_LIKE:
          type = BooleanBinaryType.Like;
          break;
        case TokenType.AssignOp:
          type = BooleanBinaryType.Equals;
          break;
        case TokenType.LessThanOp:
          type = BooleanBinaryType.LessThan;
          break;
        case TokenType.GreaterThanOp:
          type = BooleanBinaryType.GreaterThan;
          break;
        case TokenType.LessThanOrEqualOp:
          type = BooleanBinaryType.LessThanOrEqual;
          break;
        case TokenType.GreaterThanOrEqualOp:
          type = BooleanBinaryType.GreaterThanOrEqual;
          break;
        case TokenType.AnsiNotEqualOp:
        case TokenType.MsNotEqualOp:
          type = BooleanBinaryType.NotEqual;
          break;
        case TokenType.NotLessThanOp:
          type = BooleanBinaryType.NotLessThan;
          break;
        case TokenType.NotGreaterThanOp:
          type = BooleanBinaryType.NotGreaterThan;
          break;
        default:
          throw Unexpected(Current);
      }
      Consume();
      return type;
    }

    private List<From> FromList()
    {
      Match(TokenType.K_FROM);
      throw new NotImplementedException();
    }

    private ICollection<Column> ColumnList()
    {
      var columns = new List<Column>();
      columns.Add(Column());
      while (Current != null && Current.Type == TokenType.Comma)
      {
        Match(TokenType.Comma);
        columns.Add(Column());
      }
      return columns;
    }

    private Column Column()
    {
      if (Current.Type == TokenType.StarOp)
      {
        Consume();
        return new StarColumn { };
      }

      if (Current.Type == TokenType.Identifier || Current.Type == TokenType.VarcharConstant)
      {
        var token = Current;
        Consume();
        if (Current.Type == TokenType.AssignOp)
        {
          var column = new ExpressionColumn { Alias = token.Character };
          Consume();
          column.Expression = Expression();
          return column;
        }
      }

      var expression = Expression();
      var optional = true;
      if (Current != null && Current.Type == TokenType.K_AS)
      {
        Match(TokenType.K_AS);
        optional = false;
      }

      if (Current != null && (Current.Type == TokenType.Identifier || Current.Type == TokenType.VarcharConstant))
        return new ExpressionColumn { Expression = expression, Alias = Current.Character };
      else if (!optional)
        throw Expected("Identifier");

      return new ExpressionColumn { Expression = expression };
    }

    private Expression PrimaryExpression()
    {
      if (IsConstant())
        return Constant();
      else if (IsUnaryOp())
        return UnaryOp();
      else if (IsReference())
        return ReferenceExpression();
      else if (Current.Type == TokenType.OpenBracket)
      {
        Consume();
        Expression expression;
        if (Current.Type == TokenType.K_SELECT)
          expression = new SelectStatementExpression { Statement = Select() };
        else
          expression = Expression();

        if (Current.Type != TokenType.CloseBracket)
          throw Expected(")");
        Consume();
        return expression;
      }
      else throw Unexpected(Current);
    }

    private Expression Expression()
    {
      var exp = PrimaryExpression();
      return Expression(exp, 1);
    }

    private Expression Expression(Expression left, int minPrecedence)
    {
      while (IsBinaryOp())
      {
        var type = BinaryOperator();
        var precidence = (int)type / 100;
        var right = PrimaryExpression();
        if (minPrecedence >= precidence)
          right = Expression(right, precidence);

        left = new BinaryOperationExpression { Left = left, Type = type, Right = right };
      }
      return left;
    }

    private BinaryType BinaryOperator()
    {
      BinaryType type;
      switch (Current.Type)
      {
        case TokenType.DivideOp:
          type = BinaryType.Division;
          break;
        case TokenType.StarOp:
          type = BinaryType.Multiply;
          break;
        case TokenType.ModuloOp:
          type = BinaryType.Modulus;
          break;
        case TokenType.AddOp:
          type = BinaryType.Addition;
          break;
        case TokenType.SubtractOp:
          type = BinaryType.Subtraction;
          break;
        case TokenType.BitwiseAndOp:
          type = BinaryType.BitwiseAnd;
          break;
        case TokenType.BitwiseXorOp:
          type = BinaryType.BitwiseXor;
          break;
        case TokenType.BitwiseOrOp:
          type = BinaryType.BitwiseOr;
          break;
        default:
          throw Unexpected(Current);
      }
      Consume();
      return type;
    }

    private Expression ReferenceExpression()
    {
      var parts = new List<string>(ReferenceListInitializationSize);

      TokenType previous = TokenType.ReferenceOp;
      while (IsReference() && previous != TokenType.Identifier)
      {
        if (Current.Type == TokenType.ReferenceOp && previous == TokenType.ReferenceOp)
        {
          parts.Add(string.Empty);
          Consume();
        }

        if (Current.Type == TokenType.Identifier)
        {
          parts.Add(Current.Character);
          Consume();
        }

        previous = Current.Type;
      }

      var reference = new ReferenceExpression { IdentifierParts = parts };
      if (Current != null && Current.Type == TokenType.OpenBracket)
        return FunctionCall(reference);
      return reference;
    }

    private Expression FunctionCall(ReferenceExpression reference)
    {
      Match(TokenType.OpenBracket);

      var parameters = new List<Expression>();
      if (Current.Type == TokenType.CloseBracket)
        return new FunctionCallExpression { FunctionName = reference, Parameters = parameters };

      parameters.Add(Expression());

      while (Current.Type == TokenType.Comma)
      {
        Consume();

        parameters.Add(Expression());
      }

      Match(TokenType.CloseBracket);
      return new FunctionCallExpression { FunctionName = reference, Parameters = parameters };
    }

    private Expression UnaryOp()
    {
      var op = Current;
      UnaryType type;
      switch (op.Type)
      {
        case TokenType.AddOp:
          type = UnaryType.Positive;
          break;
        case TokenType.SubtractOp:
          type = UnaryType.Negative;
          break;
        case TokenType.BitwiseNotOp:
          type = UnaryType.BitwiseNot;
          break;
        default:
          throw Unexpected(op);
      }
      Consume();
      return new UnaryExpression { Type = type, Right = PrimaryExpression() };
    }

    private Expression Constant()
    {
      //TODO: Record precision and scale of numeric, length of character data
      var constant = Current;
      Consume();
      switch (constant.Type)
      {
        case TokenType.IntConstant:
          return new ConstantExpression { Type = SqlType.Int, Value = constant.Int };
        case TokenType.BigIntConstant:
          return new ConstantExpression { Type = SqlType.BigInt, Value = constant.BigInt };
        case TokenType.FloatConstant:
          return new ConstantExpression { Type = SqlType.Float, Value = constant.Real };
        case TokenType.NumericConstant:
          return new ConstantExpression { Type = SqlType.Numeric, Value = constant.Numeric };
        case TokenType.NvarcharConstant:
          return new ConstantExpression { Type = SqlType.NVarchar, Value = constant.Character };
        case TokenType.RealConstant:
          return new ConstantExpression { Type = SqlType.Real, Value = constant.Real };
        case TokenType.VarcharConstant:
          return new ConstantExpression { Type = SqlType.Varchar, Value = constant.Character };
        default:
          throw Expected("Constant");
      }
    }

    private Exception Unexpected(Token token)
    {
      return new Exception(string.Format("`{0}` unexpected at line {1} char {2}.", token.Type, token.Line, token.Column));
    }

    private Exception Expected(string expected)
    {
      return new Exception(string.Format("'{0}' expected at line {2} char {3}. Found `{1}`", expected, Current.Type, Current.Line, Current.Column));
    }

    private void Next()
    {
      if (!_tokens.MoveNext())
        throw Unexpected(new Token { Type = TokenType.EndOfFile });
    }

    private void Match(TokenType type)
    {
      if (Current == null || Current.Type != type)
        throw Expected(type.ToString());
      Consume();
    }

    private void TerminateIfEndOfFile(TokenType type)
    {
      if (Current == null)
        throw Expected(type.ToString());
    }

    private bool Consume()
    {
      return _tokens.MoveNext();
    }

    private bool IsConstant()
    {
      var type = Current.Type;
      return type == TokenType.IntConstant ||
        type == TokenType.BigIntConstant ||
        type == TokenType.FloatConstant ||
        type == TokenType.NumericConstant ||
        type == TokenType.NvarcharConstant ||
        type == TokenType.RealConstant ||
        type == TokenType.VarcharConstant;
    }

    private bool IsUnaryOp()
    {
      var type = _tokens.Current.Type;
      return type == TokenType.AddOp ||
        type == TokenType.SubtractOp ||
        type == TokenType.BitwiseNotOp;
    }

    private bool IsReference()
    {
      if (Current == null)
        return false;
      var type = Current.Type;
      return type == TokenType.Identifier || type == TokenType.ReferenceOp;
    }

    private bool IsBinaryOp()
    {
      return Current != null && (Current.Type == TokenType.DivideOp || Current.Type == TokenType.StarOp ||
        Current.Type == TokenType.ModuloOp || Current.Type == TokenType.AddOp ||
        Current.Type == TokenType.SubtractOp || Current.Type == TokenType.BitwiseAndOp ||
        Current.Type == TokenType.BitwiseXorOp || Current.Type == TokenType.BitwiseOrOp);
    }

    #region IEnumerator<Statement>

    public IEnumerator<Statement> GetEnumerator()
    {
      return new Parser.StatementEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    #region StatementEnumerator

    private class StatementEnumerator : IEnumerator<Statement>
    {
      private Parser _parser;

      public StatementEnumerator(Parser parser)
      {
        _parser = parser;
      }

      public Statement Current { get; private set; }

      public void Dispose() { }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      public bool MoveNext()
      {
        return (Current = _parser.NextStatement()) != null;
      }

      public void Reset() { }
    }

    #endregion
  }
}
