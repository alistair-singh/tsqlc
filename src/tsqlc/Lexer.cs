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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tsqlc
{
  public class Lexer : IEnumerable<Token>
  {
    private char _look;
    private char _peek;
    private int _col;
    private int _line;
    private StringBuilder _builder;
    private StreamReader _reader;

    private int _tokenLine;
    private int _tokenCol;

    public Lexer(StreamReader reader)
    {
      _col = 0;
      _line = 1;
      _builder = new StringBuilder();
      _reader = reader;
      _peek = '\0';

      Next();
    }

    private Token NextToken()
    {
      SkipWhitespace();
      _tokenLine = _line;
      _tokenCol = _col;

      if (_look == '.')
      {
        if (char.IsDigit(Peek()))
          return Number();
        else
          return Operator();
      }
      else if (char.IsDigit(_look))
        return Number();
      else if (_look == 'N')
      {
        if (Peek() == '\'')
          return String();
        else
          return Identifier();
      }
      else if (_look == '\'')
        return String();
      else if (char.IsLetter(_look) || _look == '@' || _look == '#' || _look == '_' || _look == '[' || _look == '"')
        return Identifier();
      else if (_look == '\0')
        return new Token { Type = TokenType.EndOfFile, Line = _tokenLine, Column = _tokenCol };
      else if (_look == '/' || _look == '-')
      {
        var peek = Peek();
        if ((_look == '/' && peek == '*') || (_look == '-' && peek == '-'))
          return Comment();
        return Operator();
      }
      else
        return Operator();

      throw new Exception("Unexpect char " + _look);
    }

    private void Next()
    {
      if (_peek != '\0')
      {
        _look = _peek;
        _peek = '\0';
      }
      else
      {
        var val = _reader.Read();
        if (val == -1)
        {
          _look = '\0';
          return;
        }
        _look = (char)val;
      }

      if (_look == '\n')
      {
        _line++;
        _col = 0;
      }
      else if (_look != '\r')
        _col++;
    }

    private char Peek()
    {
      Debug.Assert(_peek == '\0', "Double peek detected");

      var val = _reader.Read();
      if (val == -1)
        return '\0';

      _peek = (char)val;
      return _peek;
    }

    private void SkipWhitespace()
    {
      while (char.IsWhiteSpace(_look))
        Next();
    }

    private Token Number()
    {
      if (!char.IsDigit(_look) && _look != '.')
        throw new Exception("Number Expected");

      var pointFound = false;
      var eFound = false;
      while (char.IsDigit(_look) ||
        (_look == '.' && !pointFound) ||
        ((_look == 'e' || _look == 'E') && !eFound))
      {
        if (_look == '.')
          pointFound = true;

        if (_look == 'e' || _look == 'E')
        {
          eFound = true;
          pointFound = true;
          Next();
          if (_look != '+' && _look != '-')
            break;

          _builder.Append('E');
        }

        StashLook();
      }

      if (eFound)
        while (char.IsDigit(_look))
          StashLook();

      var value = CollectStash();
      if (eFound)
        return Real(value);
      else if (pointFound)
        return Numeric(value);
      else
        return Integer(value);
    }

    private Token Identifier()
    {
      if (!char.IsLetter(_look) && _look != '_' && _look != '@' && _look != '#' && _look != '[' && _look != '"')
        throw new Exception("Identifier Expected");

      if (_look == '[' || _look == '"')
      {
        var terminator = _look == '[' ? ']' : _look;
        StashLook();

        while (_look != terminator)
          StashLook();

        StashLook();
      }
      else
      {
        while (char.IsLetterOrDigit(_look) || _look == '_' || _look == '@' || _look == '#' || _look == '$')
          StashLook();
      }

      var identifier = CollectStash();
      var type = Keyword.LookUp(identifier);
      return MakeToken(type).Character(identifier);
    }

    private Token String()
    {
      bool isNvarchar = false;
      if (_look != 'N' && _look != '\'')
      {
        if (_look != '\'')
          throw MakeError('\'');
        else
          throw MakeError('N');
      }
      else
      {
        if (_look == 'N')
        {
          isNvarchar = true;
          Next();
        }
      }

      Next();

      while (true)
      {
        if (_look == '\'' && Peek() == '\'')
          Next();
        else if(_look == '\'')
          break;

        StashLook();
      }

      Next();

      return MakeToken(isNvarchar ? TokenType.NvarcharConstant : TokenType.VarcharConstant).Character(CollectStash());
    }

    private Token Comment()
    {
      if (_look == '-')
      {
        if (_look != '-')
          throw MakeError('-');
        Next();

        while (_look != '\r' && _look != '\n')
          StashLook();

        Next();

        if (_look == '\n')
          Next();

        var comment = CollectStash();
        return MakeToken(TokenType.LineComment).Character(comment);
      }
      else if (_look == '/')
      {
        Next();
        if (_look != '*')
          throw MakeError('*');

        Next();

        while (true)
        {
          if (_look == '*' && Peek() == '/')
            break;

          StashLook();
        }

        Next();
        if (_look == '/')
          Next();

        var comment = CollectStash();
        return MakeToken(TokenType.BlockComment).Character(comment);
      }

      throw new Exception("Comment expected.");
    }

    private Token Operator()
    {
      switch (_look)
      {
        case '.':
          Next();
          return MakeToken(TokenType.ReferenceOp);
        case ':':
          Next();
          if (_look != ':')
            throw MakeError(':');
          Next();
          return MakeToken(TokenType.ScopeReferenceOp);
        case '+':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.AddAssignOp);
          return MakeToken(TokenType.AddOp);
        case '-':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.SubtractAssignOp);
          return MakeToken(TokenType.SubtractOp);
        case '*':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.StarAssignOp);
          return MakeToken(TokenType.StarOp);
        case '/':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.DivideAssignOp);
          return MakeToken(TokenType.DivideOp);
        case '%':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.ModuloAssignOp);
          return MakeToken(TokenType.ModuloOp);
        case '&':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.BitwiseAndAssignOp);
          return MakeToken(TokenType.BitwiseAndOp);
        case '^':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.BitwiseNotAssignOp);
          return MakeToken(TokenType.BitwiseNotOp);
        case '|':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.BitwiseOrAssignOp);
          return MakeToken(TokenType.BitwiseOrOp);
        case '<':
          Next();
          if (_look == '>')
            return MakeToken(TokenType.AnsiNotEqualOp);
          if (_look == '=')
            return MakeToken(TokenType.LessThanOrEqualOp);
          return MakeToken(TokenType.LessThanOp);
        case '>':
          Next();
          if (_look == '=')
            return MakeToken(TokenType.GreaterThanOrEqualOp);
          return MakeToken(TokenType.GreaterThanOp);
        case '!':
          Next();
          if (_look == '<')
            return MakeToken(TokenType.NotLessThanOp);
          if (_look == '>')
            return MakeToken(TokenType.NotGreaterThanOp);
          if (_look == '=')
            return MakeToken(TokenType.MsNotEqualOp);
          throw MakeError('=');
        case '=':
          Next();
          return MakeToken(TokenType.AssignOp);
        case ',':
          Next();
          return MakeToken(TokenType.Comma);
        case '(':
          Next();
          return MakeToken(TokenType.OpenBracket);
        case ')':
          Next();
          return MakeToken(TokenType.CloseBracket);
        case ';':
          Next();
          return MakeToken(TokenType.SemiColon);
        default:
          throw new Exception("Unexpected operator " + _look);
      }
    }

    private Token Numeric(string value)
    {
      decimal val;
      if (decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out val))
        return MakeToken(TokenType.NumericConstant).Numeric(val);

      //TODO: Custom Exception here
      throw new Exception("Decimal Error");
    }

    private Token Integer(string value)
    {
      int intVal;
      if (int.TryParse(value, out intVal))
        return MakeToken(TokenType.IntConstant).Int(intVal);
      else
      {
        long bigIntVal;
        if (long.TryParse(value, out bigIntVal))
          return MakeToken(TokenType.BigIntConstant).BigInt(bigIntVal);
        else
          return Numeric(value);
      }

      throw new Exception("Integer Error");
    }

    private Token Real(string value)
    {
      double realVal;
      if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out realVal))
        return MakeToken(TokenType.RealConstant).Real(value, realVal);

      throw new Exception("Real Error");
    }

    private void StashLook()
    {
      _builder.Append(_look);
      Next();
    }

    private string CollectStash()
    {
      var value = _builder.ToString();
      _builder.Clear();
      return value;
    }

    private Token MakeToken(TokenType type)
    {
      return new Token { Type = type, Line = _tokenLine, Column = _tokenCol };
    }

    private Exception MakeError<T>(T expected)
    {
      //TODO: Custom Exception here
      return new Exception(string.Format("`{0}` expected at line {1} char {2}.", expected, _line, _col));
    }

    #region IEnumerator<Token>

    public IEnumerator<Token> GetEnumerator()
    {
      return new TokenEnumerator(this);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }

    #endregion

    #region Lexer.TokenEnumerator

    private class TokenEnumerator : IEnumerator<Token>
    {
      private Lexer _lexer;

      public TokenEnumerator(Lexer lexer)
      {
        _lexer = lexer;
      }

      public Token Current { get; private set; }

      public void Dispose() { }

      object IEnumerator.Current
      {
        get { return Current; }
      }

      public bool MoveNext()
      {
        Current = _lexer.NextToken();
        return Current.Type != TokenType.EndOfFile;
      }

      public void Reset() { }
    }

    #endregion
  }
}
