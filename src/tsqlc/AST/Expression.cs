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
using tsqlc.Util;

namespace tsqlc.AST
{
  public interface IExpression : ITreeVisitable { }

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

  public class UnaryExpression : IExpression
  {
    public UnaryType Type { get; set; }
    public IExpression Right { get; set; }

    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class GroupedExpression : IExpression
  {
    public IExpression Group { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class NullExpression : IExpression
  {
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class ConstantExpression : IExpression
  {
    public SqlType Type { get; set; }
    public object Value { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class SelectStatementExpression : IExpression
  {
    public SelectStatement Statement { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public interface IBooleanExpression : ITreeVisitable { }

  public class BooleanNotExpresison : IBooleanExpression
  {
    public IBooleanExpression Right { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public interface IBooleanInExpression : IBooleanExpression
  {
    IExpression Left { get; set; }
    bool Not { get; set; }
  }

  public class BooleanInSubqueryExpression : IBooleanInExpression
  {
    public IExpression Left { get; set; }
    public bool Not { get; set; }
    public SelectStatement Subquery { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanExistsExpression : IBooleanExpression
  {
    public SelectStatement Subquery { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanInListExpression : IBooleanInExpression
  {
    public IExpression Left { get; set; }
    public bool Not { get; set; }
    public ICollection<IExpression> List { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanBetweenExpression : IBooleanExpression
  {
    public IExpression Left { get; set; }
    public bool Not { get; set; }
    public IExpression First { get; set; }
    public IExpression Second { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class GroupedBooleanExpression : IBooleanExpression
  {
    public IBooleanExpression Group { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanRangeExpression : IBooleanExpression
  {
    public IExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public RangeOperatorType RangeType { get; set; }
    public SelectStatement Subquery { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanNullCheckExpression : IBooleanExpression
  {
    public IExpression Left { get; set; }
    public bool IsNull { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanComparisonExpression : IBooleanExpression
  {
    public IExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public IExpression Right { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class FunctionCallExpression : IExpression
  {
    public ReferenceExpression Function { get; set; }
    public ICollection<IExpression> Parameters { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BinaryOperationExpression : IExpression
  {
    public IExpression Left { get; set; }
    public BinaryType Type { get; set; }
    public IExpression Right { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanBinaryExpression : IBooleanExpression
  {
    public IBooleanExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public IBooleanExpression Right { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class ReferenceExpression : IExpression
  {
    public ICollection<string> IdentifierParts;
    public string Identifier { get { return string.Join(".", IdentifierParts ?? new string[0]); } }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }
}
