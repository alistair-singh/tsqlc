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
namespace tsqlc
{
  public enum TokenType
  {
    AddAssignOp,
    AddOp,
    AnsiNotEqualOp,
    AssignOp,
    BigIntConstant,
    BitwiseAndAssignOp,
    BitwiseAndOp,
    BitwiseNotAssignOp,
    BitwiseNotOp,
    BitwiseOrAssignOp,
    BitwiseOrOp,
    DivideAssignOp,
    DivideOp,
    End,
    EndOfFile,
    FloatConstant,
    GreaterThanOp,
    GreaterThanOrEqualOp,
    Identifier,
    IntConstant,
    LessThanOp,
    LessThanOrEqualOp,
    ModuloAssignOp,
    ModuloOp,
    MsNotEqualOp,
    NotGreaterThanOp,
    NotLessThanOp,
    NumericConstant,
    RealConstant,
    ReferenceOp,
    ScopeReferenceOp,
    StarAssignOp,
    StarOp,
    SubtractAssignOp,
    SubtractOp,
    LineComment,
    BlockComment,
    NvarcharConstant,
    VarcharConstant,
    K_ADD,
    K_ALL,
    K_ALTER,
    K_AND,
    K_ANY,
    K_AS,
    K_ASC,
    K_AUTHORIZATION,
    K_BACKUP,
    K_BEGIN,
    K_BETWEEN,
    K_BREAK,
    K_BROWSE,
    K_BULK,
    K_BY,
    K_CASCADE,
    K_CASE,
    K_CHECK,
    K_CHECKPOINT,
    K_CLOSE,
    K_CLUSTERED,
    K_COALESCE,
    K_COLLATE,
    K_COLUMN,
    K_COMMIT,
    K_COMPUTE,
    K_CONSTRAINT,
    K_CONTAINS,
    K_CONTAINSTABLE,
    K_CONTINUE,
    K_CONVERT,
    K_CREATE,
    K_CROSS,
    K_CURRENT,
    K_CURRENT_DATE,
    K_CURRENT_TIME,
    K_CURRENT_TIMESTAMP,
    K_CURRENT_USER,
    K_CURSOR,
    K_DATABASE,
    K_DBCC,
    K_DEALLOCATE,
    K_DECLARE,
    K_DEFAULT,
    K_DELETE,
    K_DENY,
    K_DESC,
    K_DISK,
    K_DISTINCT,
    K_DISTRIBUTED,
    K_DOUBLE,
    K_DROP,
    K_DUMP,
    K_ELSE,
    K_END,
    K_ERRLVL,
    K_ESCAPE,
    K_EXCEPT,
    K_EXEC,
    K_EXECUTE,
    K_EXISTS,
    K_EXIT,
    K_EXTERNAL,
    K_FETCH,
    K_FILE,
    K_FILLFACTOR,
    K_FOR,
    K_FOREIGN,
    K_FREETEXT,
    K_FREETEXTTABLE,
    K_FROM,
    K_FULL,
    K_FUNCTION,
    K_GOTO,
    K_GRANT,
    K_GROUP,
    K_HAVING,
    K_HOLDLOCK,
    K_IDENTITY,
    K_IDENTITY_INSERT,
    K_IDENTITYCOL,
    K_IF,
    K_IN,
    K_INDEX,
    K_INNER,
    K_INSERT,
    K_INTERSECT,
    K_INTO,
    K_IS,
    K_JOIN,
    K_KEY,
    K_KILL,
    K_LEFT,
    K_LIKE,
    K_LINENO,
    K_LOAD,
    K_MERGE,
    K_NATIONAL,
    K_NOCHECK,
    K_NONCLUSTERED,
    K_NOT,
    K_NULL,
    K_NULLIF,
    K_OF,
    K_OFF,
    K_OFFSETS,
    K_ON,
    K_OPEN,
    K_OPENDATASOURCE,
    K_OPENQUERY,
    K_OPENROWSET,
    K_OPENXML,
    K_OPTION,
    K_OR,
    K_ORDER,
    K_OUTER,
    K_OVER,
    K_PERCENT,
    K_PIVOT,
    K_PLAN,
    K_PRECISION,
    K_PRIMARY,
    K_PRINT,
    K_PROC,
    K_PROCEDURE,
    K_PUBLIC,
    K_RAISERROR,
    K_READ,
    K_READTEXT,
    K_RECONFIGURE,
    K_REFERENCES,
    K_REPLICATION,
    K_RESTORE,
    K_RESTRICT,
    K_RETURN,
    K_REVERT,
    K_REVOKE,
    K_RIGHT,
    K_ROLLBACK,
    K_ROWCOUNT,
    K_ROWGUIDCOL,
    K_RULE,
    K_SAVE,
    K_SCHEMA,
    K_SECURITYAUDIT,
    K_SELECT,
    K_SEMANTICKEYPHRASETABLE,
    K_SEMANTICSIMILARITYDETAILSTABLE,
    K_SEMANTICSIMILARITYTABLE,
    K_SESSION_USER,
    K_SET,
    K_SETUSER,
    K_SHUTDOWN,
    K_SOME,
    K_STATISTICS,
    K_SYSTEM_USER,
    K_TABLE,
    K_TABLESAMPLE,
    K_TEXTSIZE,
    K_THEN,
    K_TO,
    K_TOP,
    K_TRAN,
    K_TRANSACTION,
    K_TRIGGER,
    K_TRUNCATE,
    K_TRY_CONVERT,
    K_TSEQUAL,
    K_UNION,
    K_UNIQUE,
    K_UNPIVOT,
    K_UPDATE,
    K_UPDATETEXT,
    K_USE,
    K_USER,
    K_VALUES,
    K_VARYING,
    K_VIEW,
    K_WAITFOR,
    K_WHEN,
    K_WHERE,
    K_WHILE,
    K_WITH,
    K_WITHIN,
    K_WRITETEXT,
    Comma,
    OpenBracket,
    CloseBracket,
    SemiColon
  }
}
