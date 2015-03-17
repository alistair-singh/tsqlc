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
          return new SelectStatement { ColumnList = new List<Column>() { new ExpressionColumn { Expression = BooleanExpression() } } };
      }
    }

    private SelectStatement Select()
    {
      Match(TokenType.K_SELECT);

      Expression top = null;
      if (Current.Type == TokenType.K_TOP)
        top = Expression();

      var columns = ColumnList();

      ICollection<From> froms = null;
      if (Current.Type == TokenType.K_FROM)
        froms = FromList();

      BooleanExpression whereClause = null;
      if (Current.Type == TokenType.K_WHERE)
      {
        Consume();
        whereClause = BooleanExpression();
      }

      return new SelectStatement
      {
        ColumnList = columns,
        WhereClause = whereClause,
        FromList = froms,
        TopExpression = top
      };
    }

    private ICollection<From> FromList()
    {
      Match(TokenType.K_FROM);
      var froms = new List<From>();
      froms.Add(From(JoinType.PRIMARY));

      return froms;
    }

    private From From(JoinType type)
    {
      if (Current != null && Current.Type == TokenType.OpenBracket)
      {
        Match(TokenType.OpenBracket);
        var subquery = Select();
        Match(TokenType.CloseBracket);
        var alias = Alias();
        return new SubqueryFrom { Join = type, Subquery = subquery, Alias = alias };
      }
      else
      {
        var reference = ReferenceExpression();
        var alias = Alias();
        if (Current != null && Current.Type == TokenType.K_WITH)
        {
          Consume();
          if (Current == null || Current.Type != TokenType.OpenBracket)
            throw Unexpected(Current);
        }

        //TODO: parser must break when no locking hints are specified
        //      ... or should it...
        var hints = new List<TableHint>();
        if (Current != null && Current.Type == TokenType.OpenBracket)
        {
          Match(TokenType.OpenBracket);
          while(Current != null && (Current.Type == TokenType.Identifier || Current.Type == TokenType.K_HOLDLOCK))
          {
            TableHint hint;
            if (TableHintMap.TryLookUp(Current.Character, out hint))
              hints.Add(hint);
            else
              throw Unexpected(Current);

            Consume();

            if (Current != null && Current.Type == TokenType.Comma)
            {
              Consume();
              continue;
            }
            else
              break;
          }
          Match(TokenType.CloseBracket);
        }

        return new ReferenceFrom { Join = type, Name = reference, Alias = alias, Hints = hints };
      }
    }

    private string Alias()
    {
      string alias = string.Empty;
      if (Current != null && Current.Type == TokenType.K_AS)
      {
        Consume();
        if (Current == null || Current.Type != TokenType.Identifier)
          throw Unexpected(Current);
      }

      if (Current != null && Current.Type == TokenType.Identifier)
      {
        alias = Current.Character;
        Consume();
      }
      return alias;
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

    private BooleanExpression BooleanExpression()
    {
      var exp = PrimaryBooleanExpression();
      return BooleanExpression(exp, 1);
    }

    private BooleanExpression BooleanExpression(BooleanExpression left, int minPrecedence)
    {
      while (IsBooleanOp())
      {
        var type = BooleanOperator();
        var precidence = (int)type / 100;
        var right = PrimaryBooleanExpression();
        if (minPrecedence >= precidence)
          right = BooleanExpression(right, precidence);

        left = new BooleanBinaryExpression { Left = left, Type = type, Right = right };
      }
      return left;
    }

    private BooleanExpression PrimaryBooleanExpression()
    {
      if (Current.Type == TokenType.K_NOT)
      {
        Consume();
        var booleanExpression = PrimaryBooleanExpression();
        return new BooleanNotExpresison { Right = booleanExpression };
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
      Match(TokenType.K_EXISTS);
      Match(TokenType.OpenBracket);
      var subquery = Select();
      Match(TokenType.CloseBracket);
      return new BooleanExistsExpression { Subquery = subquery };
    }

    private BooleanRangeExpression BooleanRange(Expression left, BooleanOperatorType op)
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

    private Expression PrimaryExpression()
    {
      if (IsConstant())
        return Constant();
      else if (IsUnaryOp())
        return UnaryOp();
      else if (IsReference())
      {
        var reference = ReferenceExpression();
        if (Current != null && Current.Type == TokenType.OpenBracket)
          return FunctionCall(reference);
        else
          return reference;
      }
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

    private ReferenceExpression ReferenceExpression()
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

      return new ReferenceExpression { IdentifierParts = parts };
    }

    private FunctionCallExpression FunctionCall(ReferenceExpression reference)
    {
      Match(TokenType.OpenBracket);
      var parameters = new List<Expression>();
      if (Current.Type == TokenType.CloseBracket)
        return new FunctionCallExpression { Function = reference, Parameters = parameters };

      parameters.Add(Expression());

      while (Current.Type == TokenType.Comma)
      {
        Consume();
        parameters.Add(Expression());
      }

      Match(TokenType.CloseBracket);
      return new FunctionCallExpression { Function = reference, Parameters = parameters };
    }

    private UnaryExpression UnaryOp()
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

    private ConstantExpression Constant()
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

    private BooleanOperatorType BooleanOperator()
    {
      BooleanOperatorType type;
      switch (Current.Type)
      {
        case TokenType.K_LIKE:
          type = BooleanOperatorType.Like;
          break;
        case TokenType.AssignOp:
          type = BooleanOperatorType.Equals;
          break;
        case TokenType.LessThanOp:
          type = BooleanOperatorType.LessThan;
          break;
        case TokenType.GreaterThanOp:
          type = BooleanOperatorType.GreaterThan;
          break;
        case TokenType.LessThanOrEqualOp:
          type = BooleanOperatorType.LessThanOrEqual;
          break;
        case TokenType.GreaterThanOrEqualOp:
          type = BooleanOperatorType.GreaterThanOrEqual;
          break;
        case TokenType.AnsiNotEqualOp:
        case TokenType.MsNotEqualOp:
          type = BooleanOperatorType.NotEqual;
          break;
        case TokenType.NotLessThanOp:
          type = BooleanOperatorType.NotLessThan;
          break;
        case TokenType.NotGreaterThanOp:
          type = BooleanOperatorType.NotGreaterThan;
          break;
        case TokenType.K_OR:
          type = BooleanOperatorType.Or;
          break;
        case TokenType.K_AND:
          type = BooleanOperatorType.And;
          break;
        default:
          throw Unexpected(Current);
      }
      Consume();
      return type;
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

    private bool IsBooleanOp()
    {
      return Current != null && (Current.Type == TokenType.K_AND || Current.Type == TokenType.K_OR);
    }

    private Exception Unexpected(Token token)
    {
      return new Exception(string.Format("`{0}` unexpected at line {1} char {2}.", token.Type, token.Line, token.Column));
    }

    private Exception Expected(string expected)
    {
      return new Exception(string.Format("'{0}' expected at line {2} char {3}. Found `{1}`", expected, Current.Type, Current.Line, Current.Column));
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