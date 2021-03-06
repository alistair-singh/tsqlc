﻿/*
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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using tsqlc.AST;
using tsqlc.Parse;
using tsqlc.Util;

namespace tsqlc
{
  class Program
  {
    static void Main(string[] args)
    {
      Benchmark.Begin += (name, id) =>
      {
        using (ConsoleUtil.Color(fg: ConsoleColor.Green))
          Console.WriteLine("{0}:{1}->{2}", id, name, DateTime.Now);
      };

      Benchmark.End += (name, id, elapse) =>
      {
        using (ConsoleUtil.Color(fg: ConsoleColor.Green))
          Console.WriteLine("{0}:{1}->{2}ms, {3}", id, name, elapse, DateTime.Now);
      };

      using (Benchmark.Start("total"))
      {
        const string testFilePath = @"Test.txt";
        string text;

        using (Benchmark.Start("io"))
          text = File.ReadAllText(testFilePath);

        try
        {
          var tokens = new Token[0];
          Console.WriteLine("=== token ===");
          using (Benchmark.Start("lexer"))
          {
            using (Benchmark.Start("lex"))
              tokens = text.Lex().ToArray();

            using (Benchmark.Start("print"))
              foreach (var token in tokens)
                Console.WriteLine(token);
          }

          Console.WriteLine("=== statements ===");
          using (Benchmark.Start("parser"))
          {
            IEnumerable<IStatement> statements;
            using (Benchmark.Start("parse"))
              statements = tokens.Parse().ToArray();

            using (Benchmark.Start("print"))
              statements.Write(Console.Out);
          }
        }
        catch (Exception ex)
        {
          using (ConsoleUtil.Color(fg: ConsoleColor.Red))
            Console.WriteLine(ex.Message);
        }
      }
      Console.ReadKey(true);
    }
  }
}
