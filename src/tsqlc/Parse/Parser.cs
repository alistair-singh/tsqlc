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

      TerminatedStatement statement;
      switch (Current.Type)
      {
        case TokenType.K_SELECT:
          statement = Select();
          break;
        case TokenType.K_DELETE:
          statement = Delete();
          break;
        case TokenType.K_UPDATE:
          statement = Update();
          break;
        case TokenType.K_INSERT:
          statement = Insert();
          break;
        case TokenType.K_IF:
          return If();
          break;
        case TokenType.K_BEGIN:
          return Block();
          break;
        case TokenType.K_WHILE:
          return While();
          break;
        case TokenType.SemiColon:
          statement = new TerminatedStatement();
          break;
        default:
          throw Unexpected();
      }

      statement.HasTerminator = Consume(TokenType.SemiColon);
      return statement;
    }

    private InsertStatement Insert()
    {
      Match(TokenType.K_INSERT);
      var top = Top();
      Consume(TokenType.K_INTO);
      var target = From(allowAlias: false, allowOnClause: false);
      var columnSpecifier = ColumnListSpecification();

      if (CurrentTypeIs(TokenType.K_VALUES))
      {
        var values = Values();
        return new ValuesInsertStatement
        {
          TopExpression = top,
          Target = target,
          ColumnSpecification = columnSpecifier,
          Values = values
        };
      }
      else if (CurrentTypeIs(TokenType.K_SELECT))
      {
        var select = Select();
        return new SelectInsertStatement
        {
          TopExpression = top,
          Target = target,
          ColumnSpecification = columnSpecifier,
          SelectStatement = select
        };
      }
      throw Unexpected();
    }

    private Values Values()
    {
      Match(TokenType.K_VALUES);
      var rows = new List<ValuesRow>();
      do
        rows.Add(ValuesRow());
      while (Consume(TokenType.Comma));

      return new Values
      {
        Rows = rows
      };
    }

    private ValuesRow ValuesRow()
    {
      Match(TokenType.OpenBracket);

      var expression = new List<Expression>();
      do
        expression.Add(Expression());
      while (Consume(TokenType.Comma));

      Match(TokenType.CloseBracket);
      return new ValuesRow
      {
        Expressions = expression
      };
    }

    private ICollection<ReferenceExpression> ColumnListSpecification()
    {
      var list = new List<ReferenceExpression>();
      if (!Consume(TokenType.OpenBracket))
        return list;

      do
        list.Add(ReferenceExpression());
      while (Consume(TokenType.Comma));

      Match(TokenType.CloseBracket);
      return list;
    }

    private UpdateStatement Update()
    {
      Match(TokenType.K_UPDATE);
      var top = Top();

      var target = From(allowAlias: false, allowOnClause: false);

      Match(TokenType.K_SET);

      var setColumns = SetColumnList();
      var froms = FromList();
      var where = Where();

      return new UpdateStatement
      {
        TopExpression = top,
        Target = target,
        FromList = froms,
        SetColumnList = setColumns,
        WhereClause = where
      };
    }

    private ICollection<SetExpressionColumn> SetColumnList()
    {
      var columns = new List<SetExpressionColumn>();
      do
      {
        var reference = ReferenceExpression();
        Match(TokenType.AssignOp);
        var expression = Expression();
        columns.Add(new SetExpressionColumn
        {
          Reference = reference,
          Expression = expression
        });
      }
      while (Consume(TokenType.Comma));
      return columns;
    }

    private WhileStatement While()
    {
      Match(TokenType.K_WHILE);
      var test = BooleanExpression();
      var body = NextStatement();
      return new WhileStatement { Test = test, Body = body };
    }

    private DeleteStatement Delete()
    {
      Match(TokenType.K_DELETE);
      var top = Top();

      Consume(TokenType.K_FROM);

      var target = From(allowAlias: false, allowOnClause: false);
      var fromList = FromList();
      var where = Where();

      return new DeleteStatement
      {
        TopExpression = top,
        Target = target,
        FromList = fromList,
        WhereClause = where
      };
    }

    private BlockStatement Block()
    {
      Match(TokenType.K_BEGIN);
      var statements = new List<Statement>();
      while (!CurrentTypeIs(TokenType.K_END))
        statements.Add(NextStatement());
      Match(TokenType.K_END);
      return new BlockStatement { Statements = statements };
    }

    private IfStatement If()
    {
      Match(TokenType.K_IF);
      var test = BooleanExpression();
      var trueBody = NextStatement();

      Statement falseBody = null;
      if (Consume(TokenType.K_ELSE))
        falseBody = NextStatement();

      return new IfStatement
      {
        Test = test,
        TrueBody = trueBody,
        FalseBody = falseBody
      };
    }

    private SelectStatement Select()
    {
      Match(TokenType.K_SELECT);

      var top = Top();
      var columns = ColumnList();
      var froms = FromList();
      var whereClause = Where();

      return new SelectStatement
      {
        ColumnList = columns,
        WhereClause = whereClause,
        FromList = froms,
        TopExpression = top
      };
    }

    private BooleanExpression Where()
    {
      if (Consume(TokenType.K_WHERE))
        return BooleanExpression();

      return null;
    }

    private Expression Top()
    {
      if (Consume(TokenType.K_TOP))
        return PrimaryExpression();

      return null;
    }

    private ICollection<From> FromList()
    {
      var froms = new List<From>();
      if (!Consume(TokenType.K_FROM))
        return froms;

      froms.Add(From());
      while (IsJoin())
      {
        switch (Current.Type)
        {
          case TokenType.K_INNER:
            Match(TokenType.K_INNER);
            Match(TokenType.K_JOIN);
            froms.Add(From(JoinType.INNER));
            break;
          case TokenType.K_JOIN:
            Match(TokenType.K_JOIN);
            froms.Add(From(JoinType.INNER));
            break;
          case TokenType.K_LEFT:
            Match(TokenType.K_LEFT);
            Consume(TokenType.K_OUTER);
            Match(TokenType.K_JOIN);
            froms.Add(From(JoinType.LEFT));
            break;
          case TokenType.K_RIGHT:
            Match(TokenType.K_RIGHT);
            Consume(TokenType.K_OUTER);
            Match(TokenType.K_JOIN);
            froms.Add(From(JoinType.RIGHT));
            break;
          case TokenType.K_FULL:
            Match(TokenType.K_FULL);
            Consume(TokenType.K_OUTER);
            Match(TokenType.K_JOIN);
            froms.Add(From(JoinType.OUTER_JOIN));
            break;
        }
      }

      return froms;
    }

    private From From(JoinType type = JoinType.PRIMARY, bool allowAlias = true, bool allowOnClause = true)
    {
      if (Current != null && Current.Type == TokenType.OpenBracket)
      {
        Match(TokenType.OpenBracket);
        var subquery = Select();
        Match(TokenType.CloseBracket);
        var alias = allowAlias ? TableAlias() : string.Empty;
        return new SubqueryFrom
        {
          Join = type,
          Subquery = subquery,
          Alias = alias,
          OnClause = OnClause(type)
        };
      }
      else
      {
        var reference = ReferenceExpression();
        var alias = allowAlias ? TableAlias() : string.Empty;
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
          //TODO: Refactor here to use Consume
          while (Current != null && (Current.Type == TokenType.Identifier || Current.Type == TokenType.K_HOLDLOCK))
          {
            TableHint hint;
            if (TableHintMap.TryLookUp(Current.Character, out hint))
              hints.Add(hint);
            else
              throw Unexpected(Current);

            Consume();

            if (Consume(TokenType.Comma))
              continue;
            else
              break;
          }
          Match(TokenType.CloseBracket);
        }

        var onClause = allowOnClause ? OnClause(type) : null;
        return new ReferenceFrom
        {
          Join = type,
          Name = reference,
          Alias = alias,
          Hints = hints,
          OnClause = onClause
        };
      }
    }

    private BooleanExpression OnClause(JoinType type)
    {
      switch (type)
      {
        case JoinType.INNER:
        case JoinType.LEFT:
        case JoinType.OUTER_JOIN:
        case JoinType.RIGHT:
          Match(TokenType.K_ON);
          return BooleanExpression();
      }
      return null;
    }

    private string TableAlias()
    {
      string alias = string.Empty;
      if (Consume(TokenType.K_AS))
      {
        if (!CurrentTypeIs(TokenType.Identifier))
          throw Unexpected();
      }

      if (CurrentTypeIs(TokenType.Identifier))
      {
        alias = Current.Character;
        Consume();
      }
      return alias;
    }

    private ICollection<Column> ColumnList()
    {
      var columns = new List<Column>();
      do
        columns.Add(Column());
      while (Consume(TokenType.Comma));

      return columns;
    }

    private Column Column()
    {
      if (Consume(TokenType.StarOp))
        return new StarColumn { };

      //TODO: buggy code dont support it for now, might need to implement back tracking
      //if (Current != null && Current.Type == TokenType.Identifier || Current.Type == TokenType.VarcharConstant)
      //{
      //  var token = Current;
      //  Consume();
      //  if (Current != null && Current.Type == TokenType.AssignOp)
      //  {
      //    var column = new ExpressionColumn { Alias = token.Character };
      //    Consume();
      //    column.Expression = Expression();
      //    return column;
      //  }
      //}

      var expression = Expression();
      var optional = !Consume(TokenType.K_AS);

      if (CurrentIs(TokenType.Identifier, TokenType.VarcharConstant).HasValue)
      {
        var column = new ExpressionColumn { Expression = expression, Alias = Current.Character };
        Consume();
        return column;
      }
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
      if (Consume(TokenType.K_NOT))
        return new BooleanNotExpresison
        {
          Right = PrimaryBooleanExpression()
        };

      if (CurrentTypeIs(TokenType.K_EXISTS))
        return BooleanExists();

      var left = Expression();

      if (CurrentTypeIs(TokenType.K_BETWEEN))
        return BooleanBetween(left);

      if (CurrentTypeIs(TokenType.K_IN))
        return BooleanIn(left);

      if (CurrentTypeIs(TokenType.K_IS))
        return IsNullOrNotNull(left);

      if (Consume(TokenType.K_NOT))
      {
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
      return new ComparisonExpression { Left = left, Type = op, Right = right };
    }

    private BooleanExpression IsNullOrNotNull(Expression left)
    {
      Match(TokenType.K_IS);
      var isNull = !Consume(TokenType.K_NOT);
      Match(TokenType.K_NULL);
      return new NullComparisonExpression { IsNull = isNull, Left = left };
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
      if (CurrentTypeIs(TokenType.K_SELECT))
      {
        var subquery = Select();
        Match(TokenType.CloseBracket);
        return new BooleanInSubqueryExpression { Not = not, Left = left, Subquery = subquery };
      }

      var list = new List<Expression>();
      do
        list.Add(Expression());
      while (Consume(TokenType.Comma));

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
      else if (CurrentTypeIs(TokenType.K_NULL))
        return NullExpression();
      else if (IsReference())
      {
        var reference = ReferenceExpression();
        if (CurrentTypeIs(TokenType.OpenBracket))
          return FunctionCall(reference);
        else
          return reference;
      }
      else if (Consume(TokenType.OpenBracket))
      {
        Expression expression;
        if (CurrentTypeIs(TokenType.K_SELECT))
          expression = new SelectStatementExpression { Statement = Select() };
        else
          expression = Expression();

        Match(TokenType.CloseBracket);
        return expression;
      }
      else throw Unexpected();
    }

    private NullExpression NullExpression()
    {
      Match(TokenType.K_NULL);
      return new NullExpression();
    }

    private Expression Expression()
    {
      return Expression(PrimaryExpression(), 1);
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
      do
      {
        while (CurrentTypeIs(TokenType.ReferenceOp))
        {
          parts.Add(string.Empty);
          Consume();
        }

        if (CurrentTypeIs(TokenType.Identifier))
        {
          parts.Add(Current.Character);
          Consume();
        }
        else
          throw Unexpected();
      } while (Consume(TokenType.ReferenceOp));
      return new ReferenceExpression { IdentifierParts = parts };
    }

    private FunctionCallExpression FunctionCall(ReferenceExpression reference)
    {
      Match(TokenType.OpenBracket);
      var parameters = new List<Expression>();
      if (CurrentTypeIs(TokenType.CloseBracket))
        return new FunctionCallExpression { Function = reference, Parameters = parameters };

      do
        parameters.Add(Expression());
      while (Consume(TokenType.Comma));

      Match(TokenType.CloseBracket);
      return new FunctionCallExpression { Function = reference, Parameters = parameters };
    }

    private UnaryExpression UnaryOp()
    {
      UnaryType type;
      var tokenType = CurrentIs(TokenType.AddOp, TokenType.SubtractOp, TokenType.BitwiseNotOp);

      if (!tokenType.HasValue)
        throw Unexpected();

      switch (tokenType)
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
          throw Unexpected();
      }
      Consume();
      return new UnaryExpression { Type = type, Right = Expression() };
    }

    private ConstantExpression Constant()
    {
      //TODO: Record precision and scale of numeric, length of character data
      var type = CurrentIs(TokenType.IntConstant, TokenType.BigIntConstant, TokenType.FloatConstant,
        TokenType.NumericConstant, TokenType.NvarcharConstant, TokenType.RealConstant, TokenType.VarcharConstant);

      if (!type.HasValue)
        throw Unexpected();

      ConstantExpression expression;
      switch (type)
      {
        case TokenType.IntConstant:
          expression = new ConstantExpression { Type = SqlType.Int, Value = Current.Int };
          break;
        case TokenType.BigIntConstant:
          expression = new ConstantExpression { Type = SqlType.BigInt, Value = Current.BigInt };
          break;
        case TokenType.FloatConstant:
          expression = new ConstantExpression { Type = SqlType.Float, Value = Current.Real };
          break;
        case TokenType.NumericConstant:
          expression = new ConstantExpression { Type = SqlType.Numeric, Value = Current.Numeric };
          break;
        case TokenType.NvarcharConstant:
          expression = new ConstantExpression { Type = SqlType.NVarchar, Value = Current.Character };
          break;
        case TokenType.RealConstant:
          expression = new ConstantExpression { Type = SqlType.Real, Value = Current.Real };
          break;
        case TokenType.VarcharConstant:
          expression = new ConstantExpression { Type = SqlType.Varchar, Value = Current.Character };
          break;
        default:
          throw Unexpected();
      }

      Consume();
      return expression;
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

    private void Match(TokenType type)
    {
      if (!Consume(type))
        throw Expected(type.ToString());
    }

    private bool Consume(TokenType? type = null)
    {
      if (type.HasValue)
      {
        if (CurrentTypeIs(type.Value))
        {
          _tokens.MoveNext();
          return true;
        }
        return false;
      }
      return _tokens.MoveNext();
    }

    private bool CurrentTypeIs(TokenType type)
    {
      return CurrentIs(type).HasValue;
    }

    private TokenType? CurrentIs(IEnumerable<TokenType> types)
    {
      if (Current == null)
        return null;

      if (types.Any(x => x == Current.Type))
        return Current.Type;

      return null;
    }

    private TokenType? CurrentIs(params TokenType[] types)
    {
      return CurrentIs(types.AsEnumerable());
    }

    private bool IsJoin()
    {
      return CurrentIs(TokenType.K_LEFT, TokenType.K_RIGHT,
        TokenType.K_INNER, TokenType.K_JOIN, TokenType.K_FULL).HasValue;
    }

    private bool IsConstant()
    {
      return CurrentIs(TokenType.IntConstant, TokenType.BigIntConstant,
        TokenType.FloatConstant, TokenType.NumericConstant, TokenType.NvarcharConstant,
        TokenType.RealConstant, TokenType.VarcharConstant).HasValue;
    }

    private bool IsUnaryOp()
    {
      return CurrentIs(TokenType.AddOp, TokenType.SubtractOp, TokenType.BitwiseNotOp).HasValue;
    }

    private bool IsReference()
    {
      return CurrentIs(TokenType.Identifier, TokenType.ReferenceOp).HasValue;
    }

    private bool IsBinaryOp()
    {
      return CurrentIs(TokenType.DivideOp, TokenType.StarOp, TokenType.ModuloOp,
        TokenType.AddOp, TokenType.SubtractOp, TokenType.BitwiseAndOp, TokenType.BitwiseXorOp,
        TokenType.BitwiseOrOp).HasValue;
    }

    private bool IsBooleanOp()
    {
      return CurrentIs(TokenType.K_AND, TokenType.K_OR).HasValue;
    }

    private Exception Unexpected(Token token = null)
    {
      if (token == null)
        token = Current;

      if (token == null)
        return new Exception("Unexpected end of file.");

      return new Exception(string.Format("`{0}` unexpected at line {1} char {2}.", token.Type, token.Line,
        token.Column));
    }

    private Exception Expected(string expected)
    {
      return new Exception(string.Format("'{0}' expected at line {2} char {3}. Found `{1}`", expected, Current.Type,
        Current.Line, Current.Column));
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