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
namespace tsqlc.Parse
{
  public class Token
  {
    public int Line { get; set; }
    public int Column { get; set; }
    public int CharacterIndex { get; set; }
    public TokenType Type { get; set; }
    public string Character { get; set; }
    public int Int { get; set; }
    public long BigInt { get; set; }
    public decimal Numeric { get; set; }
    public double Real { get; set; }

    public override string ToString()
    {
      object obj = null;
      switch (Type)
      {
        case TokenType.IntConstant:
          obj = Int;
          break;
        case TokenType.BigIntConstant:
          obj = BigInt;
          break;
        case TokenType.RealConstant:
        case TokenType.FloatConstant:
          obj = Real;
          break;
        case TokenType.NumericConstant:
          obj = Numeric;
          break;
        case TokenType.Identifier:
        case TokenType.BlockComment:
        case TokenType.LineComment:
          obj = Character;
          break;
        case TokenType.NvarcharConstant:
        case TokenType.VarcharConstant:
          obj = string.Format("\'{0}\'", Character.Replace("\'", "\'\'"));
          break;
      }

      var value = obj == null ? string.Empty : string.Format(" => {0}", obj);
      return string.Format("Token {0},{1} - {2}{3}", Line, Column, Type, value);
    }
  }
}
