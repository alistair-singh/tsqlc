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
using tsqlc.Parse;

//TODO: implement values from clause
namespace tsqlc.AST
{
  public enum TableHint
  {
    NOLOCK = 0,
    NOEXPAND, FORCESCAN, FORCESEEK, HOLDLOCK, NOWAIT, 
    PAGLOCK, READCOMMITTED, READCOMMITTEDLOCK, READPAST, 
    READUNCOMMITTED, REPEATABLEREAD, ROWLOCK, SERIALIZABLE,
    SNAPSHOT, SPATIAL_WINDOW_MAX_CELLS, TABLOCK, TABLOCKX, 
    UPDLOCK, XLOCK 
  }

  public enum JoinType
  {
    PRIMARY = 0,
    INNER,
    LEFT,
    RIGHT,
    CROSS_JOIN,
    CROSS_APPLY,
    OUTER_JOIN,
    OUTER_APPLY
  }

  public class From
  {
    public string Alias { get; set; }
    public JoinType Join { get; set; }
  }

  public class ReferenceFrom : From
  {
    public ReferenceExpression Name { get; set; }
    public ICollection<TableHint> Hints { get; set; }
    public BooleanExpression OnClause { get; set; }

    public override string ToString()
    {
      return string.Format("(:{2} ({3}) {1}->{0} {4})", 
        Name,
        Alias, 
        Join, 
        string.Join(", ", Hints), 
        OnClause != null ? OnClause.ToString() : string.Empty);
    }
  }

  public class SubqueryFrom : From
  {
    public SelectStatement Subquery { get; set; }
    public BooleanExpression OnClause { get; set; }

    public override string ToString()
    {
      return string.Format("(:{2} {1}->{0} {4})", 
        Subquery, 
        Alias, 
        Join, 
        OnClause != null ? OnClause.ToString() : string.Empty);
    }
  }
}
