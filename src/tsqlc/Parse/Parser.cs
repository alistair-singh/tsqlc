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
      if(Current ==  null && !_tokens.MoveNext())
        return null;

      switch (Current.Type)
      {
        default:
          return new SelectStatement { Columns = new List<Column>() { new ExpressionColumn { Expression = Expression() } } };
      }
    }

    private Expression Expression()
    {
      if (IsConstant())
        return Constant();
      if (IsUnaryOp())
        return UnaryOp();
      if (IsReference())
        return ReferenceExpression();
      if(Current.Type == TokenType.OpenBracket)
      {
        Next();
        var expression = Expression();
        if (Current.Type != TokenType.CloseBracket)
          throw Expected(")");
        Next();
        return expression;
      }

      return new Expression();
    }
    private Expression ReferenceExpression()
    {
      var reference = new ReferenceExpression();
      while(IsReference())
      {
        if (Current.Type == TokenType.ReferenceOp)
          reference.IdentifierParts.Add(string.Empty);
        if (Current.Type == TokenType.Identifier)
          reference.IdentifierParts.Add(Current.Character);
        Next();
      }
      return reference;
    }

    //private Expression BinaryOp()
    //{
    //  var left = Expression();
    //  Next();
    //  var op = _tokens.Current;
    //  Next();
    //  var right = Expression();
    //}

    private Expression UnaryOp()
    {
      var op = Current;
      UnaryType type;
      switch(op.Type)
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
      Next();
      return new UnaryExpression { Type = type, Right = Expression() };
    }

    private Expression Constant()
    {
      //TODO: Record precision and scale of numeric, length of character data
      var constant = Current;
      Next();
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

    private bool IsReference()
    {
      var type = Current.Type;
      return type == TokenType.Identifier || type == TokenType.ReferenceOp;
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
        Current = _parser.NextStatement();
        return Current != null;
      }

      public void Reset() { }
    }

    #endregion
  }
}
