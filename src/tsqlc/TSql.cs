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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc.AST;
using tsqlc.Parse;

namespace tsqlc
{
  public static class TSql
  {
    public static IEnumerable<Statement> Parse(this IEnumerable<Token> tokens)
    {
      return new Parser(tokens);
    }

    public static IEnumerable<Statement> Parse(this IEnumerable<char> tokens)
    {
      return tokens.Lex().Parse();
    }

    public static IEnumerable<Statement> Parse(Stream stream)
    {
      return Lex(stream).Parse();
    }

    public static IEnumerable<Statement> Parse(TextReader reader)
    {
      return Lex(reader).Parse();
    }

    public static IEnumerable<Statement> Parse(IEnumerable<byte> buffer)
    {
      return Lex(buffer).Parse();
    }

    public static IEnumerable<Statement> Parse(byte[] buffer)
    {
      return Lex(buffer).Parse();
    }

    public static IEnumerable<Statement> ParseFile(string path)
    {
      return LexFile(path).Parse();
    }

    public static IEnumerable<Token> Lex(this IEnumerable<char> characters)
    {
      return new Lexer(characters);
    }

    public static IEnumerable<Token> Lex(Stream stream)
    {
      return FromStream(stream).Lex();
    }

    public static IEnumerable<Token> Lex(TextReader reader)
    {
      return FromReader(reader).Lex();
    }

    public static IEnumerable<Token> Lex(IEnumerable<byte> buffer)
    {
      return FromBuffer(buffer.ToArray()).Lex();
    }

    public static IEnumerable<Token> Lex(byte[] buffer)
    {
      return FromBuffer(buffer).Lex();
    }

    public static IEnumerable<Token> LexFile(string path)
    {
      return FromBuffer(FromFile(path)).Lex();
    }

    private static IEnumerable<char> FromBuffer(byte[] buffer)
    {
      return FromStream(new MemoryStream(buffer));
    }

    private static byte[] FromFile(string path)
    {
      return File.ReadAllBytes(path);
    }

    private static IEnumerable<char> FromStream(Stream stream)
    {
      var reader = new StreamReader(stream);
      return FromReader(reader);
    }

    private static IEnumerable<char> FromReader(TextReader reader)
    {
      return reader.ReadToEnd();
    }
  }
}