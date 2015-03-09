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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using tsqlc.Parse;

namespace tsqlc.Tests
{
  [TestClass]
  public class LexerTests
  {
    [TestMethod]
    public void EmptyInputYieldsNoTokens()
    {
      const string input = @"";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 0, "result is not empty");
    }

    [TestMethod]
    public void NullInputThrowsArgumentNullException()
    {
      const string input = null;
      try
      {
        var result = new Lexer(input).ToArray();
        Assert.Fail("not supposed to reach here");
      }
      catch (ArgumentNullException e)
      {
        Assert.IsTrue(e.ParamName == "characters", "Argument exception for characters not thrown");
      }
    }

    [TestMethod]
    public void WhitespaceInputYieldsNoTokens()
    {
      const string input = " \t \n \r \r \v";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 0, "result is not empty");
    }

    [TestMethod]
    public void LexVarcharConstant()
    {
      const string input = @" 'hello' ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.VarcharConstant, "token type not varchar");
      Assert.IsTrue(result[0].Character == "hello", "values are not equal");
    }

    [TestMethod]
    public void LexNvarcharConstant()
    {
      const string input = @" N'hello' ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.NvarcharConstant, "token type not nvarchar");
      Assert.IsTrue(result[0].Character == "hello", "values are not equal");
    }

    [TestMethod]
    public void LexIntegerConstant()
    {
      const string input = @" 123 ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.IntConstant, "token type not int");
      Assert.IsTrue(result[0].Int == 123, "values are not equal");
    }

    [TestMethod]
    public void LexBigIntegerConstant()
    {
      const string input = @" 6000000000 ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.BigIntConstant, "token type not bigint");
      Assert.IsTrue(result[0].BigInt == 6000000000, "values are not equal");
    }

    [TestMethod]
    public void LexRealConstant()
    {
      const string input = @" 3e+4 ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.AreEqual(result[0].CharacterIndex, 2);
      Assert.IsTrue(result[0].Type == TokenType.RealConstant, "token type not real");
      Assert.IsTrue(result[0].Real == 30000, "values are not equal");
    }

    [TestMethod]
    public void LexNumericConstant()
    {
      const string input = @" 100.001 ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.NumericConstant, "token type not numeric");
      Assert.IsTrue(result[0].Numeric == new decimal(100.001), "values are not equal");
    }

    [TestMethod]
    public void LexDoubleQuotedIdentifier()
    {
      const string input = @" ""  hello"" ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.Identifier, "token type not quoted identifier");
      Assert.AreEqual("\"  hello\"", result[0].Character, "values are not equal");
    }

    [TestMethod]
    public void LexIdentifier()
    {
      const string input = @" bye ";
      var result = new Lexer(input).ToArray();
      Assert.AreEqual(1, result.Length, "length not expected");
      Assert.AreEqual(TokenType.Identifier, result[0].Type, "token type not quoted identifier");
      Assert.AreEqual("bye", result[0].Character, "values are not equal");
    }

    [TestMethod]
    public void LexSquareBracketIdentifier()
    {
      const string input = @" [  hello  ] ";
      var result = new Lexer(input).ToArray();
      Assert.IsTrue(result.Length == 1, "result is empty");
      Assert.IsTrue(result[0].Type == TokenType.Identifier, "token type not identifier");
      Assert.AreEqual("[  hello  ]", result[0].Character, "values are not equal");
    }

    [TestMethod]
    public void LexLineComment()
    {
      const string input = @"sdsd -- hello";
      var result = new Lexer(input).ToArray();
      Assert.AreEqual(2, result.Length, "length not expected");
      Assert.AreEqual(TokenType.LineComment, result[1].Type, "token type not line comment");
      Assert.AreEqual(" hello", result[1].Character, "values are not equal");
    }

    [TestMethod]
    public void LexBlockComment()
    {
      const string input = @"sdsd /* hello
todo
*/ end";
      var result = new Lexer(input).ToArray();
      Assert.AreEqual(3, result.Length, "length not expected");
      Assert.AreEqual(result[1].CharacterIndex, 6);
      Assert.AreEqual(TokenType.BlockComment, result[1].Type, "token type not block comment");
      Assert.AreEqual(@" hello
todo
", result[1].Character, "values are not equal");
    }

    [TestMethod]
    public void LexNeverEndingIdentifier()
    {
      try
      {
        const string input = @"[ sadsdsd  ";
        var result = new Lexer(input).ToArray();
        Assert.Fail("Never reach here");
      }
      catch (Exception e)
      {
        Assert.AreEqual("`]` expected at line 1 char 12.", e.Message);
      }
    }

    [TestMethod]
    public void LexNeverEndingString()
    {
      try
      {
        const string input = @"' sad sd sd ";
        var result = new Lexer(input).ToArray();
        Assert.Fail("Never reach here");
      }
      catch (Exception e)
      {
        Assert.AreEqual("`'` expected at line 1 char 13.", e.Message);
      }
    }

    [TestMethod]
    public void LexOperatorToken()
    {
      const string input = @"- += =";
      var result = new Lexer(input).ToArray();
      Assert.AreEqual(3, result.Length, "length not expected");
      Assert.AreEqual(TokenType.SubtractOp, result[0].Type, "not subtract");
      Assert.AreEqual(TokenType.AddAssignOp, result[1].Type, "not add assignment");
      Assert.AreEqual(TokenType.AssignOp, result[2].Type, "not assignment");
    }

    [TestMethod]
    public void LexKeywords()
    {
      const string input = @" SELECT FRoM delete ";

      var result = new Lexer(input).ToArray();

      Assert.AreEqual(3, result.Length, "length not expected");
      Assert.AreEqual(TokenType.K_SELECT, result[0].Type, "not keyword");
      Assert.AreEqual(@"SELECT", result[0].Character, "values are not equal");
      Assert.AreEqual(TokenType.K_FROM, result[1].Type, "not keyword");
      Assert.AreEqual(@"FRoM", result[1].Character, "values are not equal");
      Assert.AreEqual(TokenType.K_DELETE, result[2].Type, "not keyword");
      Assert.AreEqual(@"delete", result[2].Character, "values are not equal");
    }
  }
}
