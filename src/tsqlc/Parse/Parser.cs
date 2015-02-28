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

    public Parser(IEnumerable<Token> tokens)
    {
      _tokens = tokens.GetEnumerator();
    }

    private Token Next()
    {
      if (_tokens.MoveNext())
        return _tokens.Current;
      return new Token { Type = TokenType.EndOfFile };
    }

    private Token Last()
    {
      return _tokens.Current;
    }

    public Statement NextStatement()
    {
      var token = Next();
      switch (token.Type)
      {
        case TokenType.EndOfFile:
          return null;
        case TokenType.K_SELECT:
          return Select();
        default:
          throw Unexpected(token);
      }
    }

    private Statement Select()
    {
      var columns = new List<Column>();
      Token token;
      do
      {
        token = Next();
        switch (token.Type)
        {
          case TokenType.StarOp:
            columns.Add(new StarColumn { TableAlias = "*" });
            token = Next();
            break;
          case TokenType.VarcharConstant:
          case TokenType.Identifier:
            var first = token;
            token = Next();
            if (token.Type == TokenType.AssignOp)
            {
              columns.Add(ColumnExpression(first.Character));
              token = Next();
            }
            else if (token.Type == TokenType.Comma)
            {
              columns.Add(ColumnExpression(first.Character));
              continue;
            }
            else
            {
              columns.Add(ColumnExpression(first));
              token = Next();
            }
            break;
          default:
            throw Unexpected(token);
        }
      } while (token.Type == TokenType.Comma);

      return new SelectStatement { Columns = columns };
    }

    private Column ColumnExpression(string alias)
    {
      return new ExpressionColumn { Alias = alias, Expression = Expression() };
    }

    private Column ColumnExpression(Token first)
    {
      var column = new ExpressionColumn { Expression = Expression(first) };
      var token = Last();
      if (token.Type == TokenType.K_AS)
        token = Next();

      switch (token.Type)
      {
        case TokenType.VarcharConstant:
        case TokenType.Identifier:
          column.Alias = token.Character;
          break;
        default:
          throw Unexpected(token);
      }
      return column;
    }

    private Expression Expression(Token first = null)
    {
      //if (first == null)
      //  first = Next();

      return new Expression();
    }

    private Exception Unexpected(Token token)
    {
      return new Exception(string.Format("`{0}` unexpected at line {1} char {2}.", token.Type, token.Line, token.Column));
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
        var statement = _parser.NextStatement();
        if (statement != null)
        {
          Current = statement;
          return true;
        }
        return false;
      }

      public void Reset() { }
    }

    #endregion
  }
}
