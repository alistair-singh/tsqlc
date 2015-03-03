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
      catch(ArgumentNullException e)
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
    public void LexVarcharConstant() { Assert.Fail("not implemented yet"); }

    [TestMethod]
    public void LexNVarcharConstant() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexIntegerConstant() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexBigIntegerConstant() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexRealConstant() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexNumericConstant() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexDoubleQuotedIdentifier() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexSquareBracketIdentifier() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexLineComment() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexBlockComment() { Assert.Fail("not implemented yet");  }

    [TestMethod]
    public void LexStringUnrecognizedToken() { Assert.Fail("not implemented yet"); }

    [TestMethod]
    public void LexStringExpectedToken() { Assert.Fail("not implemented yet"); }

    [TestMethod]
    public void LexOperatorToken() { Assert.Fail("not implemented yet"); }

    [TestMethod]
    public void LexKeywords() { Assert.Fail("not implemented yet"); }

    [TestMethod]
    public void LexAcidTest() { Assert.Fail("not implemented yet"); }
  }
}
