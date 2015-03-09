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
          return new SelectStatement { Columns = new List<Column>() { new ExpressionColumn { Expression = Expression() } } };
      }
    }

    private Statement Select()
    {
      Match(TokenType.K_SELECT);
      var columns = ColumnList();
      return new SelectStatement { Columns = columns };
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
        var expression = Expression();
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
      while (IsReference())
      {
        if (Current.Type == TokenType.ReferenceOp && previous == TokenType.ReferenceOp)
          parts.Add(string.Empty);

        if (Current.Type == TokenType.Identifier)
          parts.Add(Current.Character);

        previous = Current.Type;
        Consume();
      }

      if (previous == TokenType.ReferenceOp)
        parts.Add(string.Empty);

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
