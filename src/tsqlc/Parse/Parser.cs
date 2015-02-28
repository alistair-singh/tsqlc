using System;
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
  public class Parser
  {
    private readonly IEnumerator<Token> _tokens;

    public Parser(IEnumerable<Token> tokens)
    {
      _tokens = tokens.GetEnumerator();
    }

    public IEnumerable<Statement> Parse()
    {
      while(_tokens.MoveNext())
      {
        var token = _tokens.Current;

        if(token.Type == TokenType.K_SELECT)
        {
          yield return Select();
          continue;
        }

        throw new Exception(string.Format("`{0}` unexpected at line {1} char {2}.", token.Type, token.Line, token.Column));
      }
    }

    private Statement Select()
    {
      return new SelectStatement();
    }
  }
}
