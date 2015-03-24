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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc.AST;

namespace tsqlc.Parse
{
  public class TableHintMap
  {
    public static bool TryLookUp(string lookup, out TableHint hint)
    {
      return _keywords.TryGetValue(lookup.ToUpperInvariant(), out hint);
    }

    private static Dictionary<string, TableHint> _keywords = new Dictionary<string, TableHint>
    {
      {"NOLOCK", TableHint.NOLOCK}, 
      {"NOEXPAND", TableHint.NOEXPAND}, //TODO: Requires more than an enum
      {"FORCESCAN", TableHint.FORCESCAN}, 
      {"FORCESEEK", TableHint.FORCESEEK}, 
      {"HOLDLOCK", TableHint.HOLDLOCK}, 
      {"NOWAIT", TableHint.NOWAIT}, 
      {"PAGLOCK", TableHint.PAGLOCK}, 
      {"READCOMMITTED", TableHint.READCOMMITTED}, 
      {"READCOMMITTEDLOCK", TableHint.READCOMMITTEDLOCK}, 
      {"READPAST", TableHint.READPAST}, 
      {"READUNCOMMITTED", TableHint.READUNCOMMITTED}, 
      {"REPEATABLEREAD", TableHint.REPEATABLEREAD}, 
      {"ROWLOCK", TableHint.ROWLOCK}, 
      {"SERIALIZABLE", TableHint.SERIALIZABLE}, 
      {"SNAPSHOT", TableHint.SNAPSHOT}, 
      {"SPATIAL_WINDOW_MAX_CELLS", TableHint.SPATIAL_WINDOW_MAX_CELLS}, //TODO: Requires more than an enum
      {"TABLOCK", TableHint.TABLOCK}, 
      {"TABLOCKX", TableHint.TABLOCKX}, 
      {"UPDLOCK", TableHint.UPDLOCK}, 
      {"XLOCK", TableHint.XLOCK}
    };
  }
}
