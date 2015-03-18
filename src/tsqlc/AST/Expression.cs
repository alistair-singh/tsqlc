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

namespace tsqlc.AST
{
  public class Expression
  {
  }

  public enum SqlType
  {
    Float, Int, BigInt, Real, Numeric, Varchar, NVarchar
  }

  public enum UnaryType
  {
    Positive, Negative, BitwiseNot
  }

  public enum BinaryType
  {
    Multiply = 201,
    Modulus = 202,
    Division = 203,
    Addition = 101,
    Subtraction = 102,
    BitwiseAnd = 103,
    BitwiseXor = 104,
    BitwiseOr = 105
  }

  public enum BooleanOperatorType
  {
    Equals,
    LessThan,
    GreaterThan,
    LessThanOrEqual,
    GreaterThanOrEqual,
    NotEqual,
    NotGreaterThan,
    NotLessThan,
    Like,
    Or = 101,
    And = 201
  }

  public enum RangeOperatorType
  {
    All, Any, Some
  }

  public class UnaryExpression : Expression
  {
    public UnaryType Type { get; set; }
    public Expression Right { get; set; }

    public override string ToString()
    {
      return string.Format("{0}({1})", Type, Right);
    }
  }

  public class NullExpression : Expression
  {
    public override string ToString()
    {
      return "(null)";
    }
  }

  public class ConstantExpression : Expression
  {
    public SqlType Type { get; set; }
    public object Value { get; set; }

    public override string ToString()
    {
      return Value.ToString();
    }
  }

  public class SelectStatementExpression : Expression
  {
    public Statement Statement { get; set; }
    public override string ToString()
    {
      return string.Format("(SELECT ...)");
    }
  }

  public class BooleanExpression : Expression
  {
  }

  public class BooleanNotExpresison : BooleanExpression
  {
    public Expression Right { get; set; }

    public override string ToString()
    {
      return string.Format("(Not {0})", Right);
    }
  }

  public class BooleanInExpression : BooleanExpression
  {
    public Expression Left { get; set; }
    public bool Not { get; set; }
  }

  public class BooleanInSubqueryExpression : BooleanInExpression
  {
    public SelectStatement Subquery { get; set; }

    public override string ToString()
    {
      return string.Format("({0}IN {1} ({2}))", Not ? "Not" : string.Empty, Left, Subquery);
    }
  }

  public class BooleanExistsExpression : BooleanExpression
  {
    public SelectStatement Subquery { get; set; }

    public override string ToString()
    {
      return string.Format("(EXISTS {0})", Subquery);
    }
  }

  public class BooleanInListExpression : BooleanInExpression
  {
    public ICollection<Expression> List { get; set; }

    public override string ToString()
    {
      return string.Format("({0}IN {1} ({2}))", Not ? "Not" : string.Empty, Left, string.Join(",", List));
    }
  }

  public class BooleanBetweenExpression : BooleanExpression
  {
    public Expression Left { get; set; }
    public bool Not { get; set; }
    public Expression First { get; set; }
    public Expression Second { get; set; }

    public override string ToString()
    {
      return string.Format("({0}BETWEEN {1} {2} {3})", Not ? "Not" : string.Empty, Left, First, Second);
    }
  }

  public class BooleanRangeExpression : BooleanExpression
  {
    public Expression Left { get; set; }
    public BooleanOperatorType BooleanOperator { get; set; }
    public RangeOperatorType RangeOperator { get; set; }
    public SelectStatement Subquery { get; set; }

    public override string ToString()
    {
      return string.Format("({0} {1} (:e {2}) (:e {3}))", BooleanOperator, RangeOperator, Left, Subquery);
    }
  }

  public class NullComparisonExpression : BooleanExpression
  {
    public Expression Left { get; set; }
    public bool IsNull { get; set; }

    public override string ToString()
    {
      return string.Format("({0} {1})", IsNull ? "IsNull" : "IsNotNull", Left);
    }
  }

  public class ComparisonExpression : BooleanExpression
  {
    public Expression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public Expression Right { get; set; }

    public override string ToString()
    {
      return string.Format("({0} (:e {1}) (:e {2}))", Type, Left, Right);
    }
  }

  public class FunctionCallExpression : Expression
  {
    public ReferenceExpression Function { get; set; }
    public ICollection<Expression> Parameters { get; set; }

    public override string ToString()
    {
      return string.Format("{0}({1})", Function, string.Join(", ", Parameters));
    }
  }

  public class BinaryOperationExpression : Expression
  {
    public Expression Left { get; set; }
    public BinaryType Type { get; set; }
    public Expression Right { get; set; }

    public override string ToString()
    {
      return string.Format("({0} {1} {2})", Type, Left, Right);
    }
  }

  public class BooleanBinaryExpression : BooleanExpression
  {
    public BooleanExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public BooleanExpression Right { get; set; }

    public override string ToString()
    {
      return string.Format("({0} {1} {2})", Type, Left, Right);
    }
  }

  public class ReferenceExpression : Expression
  {
    public ICollection<string> IdentifierParts;
    public string Identifier { get { return string.Join(".", IdentifierParts ?? new string[0]); } }

    public override string ToString()
    {
      return Identifier;
    }
  }
}
