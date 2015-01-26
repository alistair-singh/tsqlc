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

namespace tsqlc
{
  public class Keyword
  {
    public static TokenType LookUp(string lookup)
    {
      TokenType type;
      if(_keywords.TryGetValue(lookup.ToUpperInvariant(), out type))
        return type;

      return TokenType.Identifier; 
    }

    private static Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
    {
      {"ADD",TokenType.K_ADD},
      {"ALL",TokenType.K_ALL},
      {"ALTER",TokenType.K_ALTER},
      {"AND",TokenType.K_AND},
      {"ANY",TokenType.K_ANY},
      {"AS",TokenType.K_AS},
      {"ASC",TokenType.K_ASC},
      {"AUTHORIZATION",TokenType.K_AUTHORIZATION},
      {"BACKUP",TokenType.K_BACKUP},
      {"BEGIN",TokenType.K_BEGIN},
      {"BETWEEN",TokenType.K_BETWEEN},
      {"BREAK",TokenType.K_BREAK},
      {"BROWSE",TokenType.K_BROWSE},
      {"BULK",TokenType.K_BULK},
      {"BY",TokenType.K_BY},
      {"CASCADE",TokenType.K_CASCADE},
      {"CASE",TokenType.K_CASE},
      {"CHECK",TokenType.K_CHECK},
      {"CHECKPOINT",TokenType.K_CHECKPOINT},
      {"CLOSE",TokenType.K_CLOSE},
      {"CLUSTERED",TokenType.K_CLUSTERED},
      {"COALESCE",TokenType.K_COALESCE},
      {"COLLATE",TokenType.K_COLLATE},
      {"COLUMN",TokenType.K_COLUMN},
      {"COMMIT",TokenType.K_COMMIT},
      {"COMPUTE",TokenType.K_COMPUTE},
      {"CONSTRAINT",TokenType.K_CONSTRAINT},
      {"CONTAINS",TokenType.K_CONTAINS},
      {"CONTAINSTABLE",TokenType.K_CONTAINSTABLE},
      {"CONTINUE",TokenType.K_CONTINUE},
      {"CONVERT",TokenType.K_CONVERT},
      {"CREATE",TokenType.K_CREATE},
      {"CROSS",TokenType.K_CROSS},
      {"CURRENT",TokenType.K_CURRENT},
      {"CURRENT_DATE",TokenType.K_CURRENT_DATE},
      {"CURRENT_TIME",TokenType.K_CURRENT_TIME},
      {"CURRENT_TIMESTAMP",TokenType.K_CURRENT_TIMESTAMP},
      {"CURRENT_USER",TokenType.K_CURRENT_USER},
      {"CURSOR",TokenType.K_CURSOR},
      {"DATABASE",TokenType.K_DATABASE},
      {"DBCC",TokenType.K_DBCC},
      {"DEALLOCATE",TokenType.K_DEALLOCATE},
      {"DECLARE",TokenType.K_DECLARE},
      {"DEFAULT",TokenType.K_DEFAULT},
      {"DELETE",TokenType.K_DELETE},
      {"DENY",TokenType.K_DENY},
      {"DESC",TokenType.K_DESC},
      {"DISK",TokenType.K_DISK},
      {"DISTINCT",TokenType.K_DISTINCT},
      {"DISTRIBUTED",TokenType.K_DISTRIBUTED},
      {"DOUBLE",TokenType.K_DOUBLE},
      {"DROP",TokenType.K_DROP},
      {"DUMP",TokenType.K_DUMP},
      {"ELSE",TokenType.K_ELSE},
      {"END",TokenType.K_END},
      {"ERRLVL",TokenType.K_ERRLVL},
      {"ESCAPE",TokenType.K_ESCAPE},
      {"EXCEPT",TokenType.K_EXCEPT},
      {"EXEC",TokenType.K_EXEC},
      {"EXECUTE",TokenType.K_EXECUTE},
      {"EXISTS",TokenType.K_EXISTS},
      {"EXIT",TokenType.K_EXIT},
      {"EXTERNAL",TokenType.K_EXTERNAL},
      {"FETCH",TokenType.K_FETCH},
      {"FILE",TokenType.K_FILE},
      {"FILLFACTOR",TokenType.K_FILLFACTOR},
      {"FOR",TokenType.K_FOR},
      {"FOREIGN",TokenType.K_FOREIGN},
      {"FREETEXT",TokenType.K_FREETEXT},
      {"FREETEXTTABLE",TokenType.K_FREETEXTTABLE},
      {"FROM",TokenType.K_FROM},
      {"FULL",TokenType.K_FULL},
      {"FUNCTION",TokenType.K_FUNCTION},
      {"GOTO",TokenType.K_GOTO},
      {"GRANT",TokenType.K_GRANT},
      {"GROUP",TokenType.K_GROUP},
      {"HAVING",TokenType.K_HAVING},
      {"HOLDLOCK",TokenType.K_HOLDLOCK},
      {"IDENTITY",TokenType.K_IDENTITY},
      {"IDENTITY_INSERT",TokenType.K_IDENTITY_INSERT},
      {"IDENTITYCOL",TokenType.K_IDENTITYCOL},
      {"IF",TokenType.K_IF},
      {"IN",TokenType.K_IN},
      {"INDEX",TokenType.K_INDEX},
      {"INNER",TokenType.K_INNER},
      {"INSERT",TokenType.K_INSERT},
      {"INTERSECT",TokenType.K_INTERSECT},
      {"INTO",TokenType.K_INTO},
      {"IS",TokenType.K_IS},
      {"JOIN",TokenType.K_JOIN},
      {"KEY",TokenType.K_KEY},
      {"KILL",TokenType.K_KILL},
      {"LEFT",TokenType.K_LEFT},
      {"LIKE",TokenType.K_LIKE},
      {"LINENO",TokenType.K_LINENO},
      {"LOAD",TokenType.K_LOAD},
      {"MERGE",TokenType.K_MERGE},
      {"NATIONAL",TokenType.K_NATIONAL},
      {"NOCHECK",TokenType.K_NOCHECK},
      {"NONCLUSTERED",TokenType.K_NONCLUSTERED},
      {"NOT",TokenType.K_NOT},
      {"NULL",TokenType.K_NULL},
      {"NULLIF",TokenType.K_NULLIF},
      {"OF",TokenType.K_OF},
      {"OFF",TokenType.K_OFF},
      {"OFFSETS",TokenType.K_OFFSETS},
      {"ON",TokenType.K_ON},
      {"OPEN",TokenType.K_OPEN},
      {"OPENDATASOURCE",TokenType.K_OPENDATASOURCE},
      {"OPENQUERY",TokenType.K_OPENQUERY},
      {"OPENROWSET",TokenType.K_OPENROWSET},
      {"OPENXML",TokenType.K_OPENXML},
      {"OPTION",TokenType.K_OPTION},
      {"OR",TokenType.K_OR},
      {"ORDER",TokenType.K_ORDER},
      {"OUTER",TokenType.K_OUTER},
      {"OVER",TokenType.K_OVER},
      {"PERCENT",TokenType.K_PERCENT},
      {"PIVOT",TokenType.K_PIVOT},
      {"PLAN",TokenType.K_PLAN},
      {"PRECISION",TokenType.K_PRECISION},
      {"PRIMARY",TokenType.K_PRIMARY},
      {"PRINT",TokenType.K_PRINT},
      {"PROC",TokenType.K_PROC},
      {"PROCEDURE",TokenType.K_PROCEDURE},
      {"PUBLIC",TokenType.K_PUBLIC},
      {"RAISERROR",TokenType.K_RAISERROR},
      {"READ",TokenType.K_READ},
      {"READTEXT",TokenType.K_READTEXT},
      {"RECONFIGURE",TokenType.K_RECONFIGURE},
      {"REFERENCES",TokenType.K_REFERENCES},
      {"REPLICATION",TokenType.K_REPLICATION},
      {"RESTORE",TokenType.K_RESTORE},
      {"RESTRICT",TokenType.K_RESTRICT},
      {"RETURN",TokenType.K_RETURN},
      {"REVERT",TokenType.K_REVERT},
      {"REVOKE",TokenType.K_REVOKE},
      {"RIGHT",TokenType.K_RIGHT},
      {"ROLLBACK",TokenType.K_ROLLBACK},
      {"ROWCOUNT",TokenType.K_ROWCOUNT},
      {"ROWGUIDCOL",TokenType.K_ROWGUIDCOL},
      {"RULE",TokenType.K_RULE},
      {"SAVE",TokenType.K_SAVE},
      {"SCHEMA",TokenType.K_SCHEMA},
      {"SECURITYAUDIT",TokenType.K_SECURITYAUDIT},
      {"SELECT",TokenType.K_SELECT},
      {"SEMANTICKEYPHRASETABLE",TokenType.K_SEMANTICKEYPHRASETABLE},
      {"SEMANTICSIMILARITYDETAILSTABLE",TokenType.K_SEMANTICSIMILARITYDETAILSTABLE},
      {"SEMANTICSIMILARITYTABLE",TokenType.K_SEMANTICSIMILARITYTABLE},
      {"SESSION_USER",TokenType.K_SESSION_USER},
      {"SET",TokenType.K_SET},
      {"SETUSER",TokenType.K_SETUSER},
      {"SHUTDOWN",TokenType.K_SHUTDOWN},
      {"SOME",TokenType.K_SOME},
      {"STATISTICS",TokenType.K_STATISTICS},
      {"SYSTEM_USER",TokenType.K_SYSTEM_USER},
      {"TABLE",TokenType.K_TABLE},
      {"TABLESAMPLE",TokenType.K_TABLESAMPLE},
      {"TEXTSIZE",TokenType.K_TEXTSIZE},
      {"THEN",TokenType.K_THEN},
      {"TO",TokenType.K_TO},
      {"TOP",TokenType.K_TOP},
      {"TRAN",TokenType.K_TRAN},
      {"TRANSACTION",TokenType.K_TRANSACTION},
      {"TRIGGER",TokenType.K_TRIGGER},
      {"TRUNCATE",TokenType.K_TRUNCATE},
      {"TRY_CONVERT",TokenType.K_TRY_CONVERT},
      {"TSEQUAL",TokenType.K_TSEQUAL},
      {"UNION",TokenType.K_UNION},
      {"UNIQUE",TokenType.K_UNIQUE},
      {"UNPIVOT",TokenType.K_UNPIVOT},
      {"UPDATE",TokenType.K_UPDATE},
      {"UPDATETEXT",TokenType.K_UPDATETEXT},
      {"USE",TokenType.K_USE},
      {"USER",TokenType.K_USER},
      {"VALUES",TokenType.K_VALUES },
      {"VARYING",TokenType.K_VARYING },
      {"VIEW",TokenType.K_VIEW },
      {"WAITFOR",TokenType.K_WAITFOR },
      {"WHEN",TokenType.K_WHEN },
      {"WHERE",TokenType.K_WHERE },
      {"WHILE",TokenType.K_WHILE },
      {"WITH",TokenType.K_WITH },
      {"WITHIN GROUP",TokenType.K_WITHIN },
      {"WRITETEXT",TokenType.K_WRITETEXT }
    };
  }
}
