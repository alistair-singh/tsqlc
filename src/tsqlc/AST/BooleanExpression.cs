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
using tsqlc.Util;

namespace tsqlc.AST
{
  public interface IBooleanExpression : ITreeVisitable 
  {
    Token Token { get; set; }
  }

  public class BooleanNotExpresison : IBooleanExpression
  {
    public Token Token { get; set; }
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
    public Token Token { get; set; }
    public IExpression Left { get; set; }
    public bool Not { get; set; }
    public SelectStatement Subquery { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanExistsExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public SelectStatement Subquery { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanInListExpression : IBooleanInExpression
  {
    public Token Token { get; set; }
    public IExpression Left { get; set; }
    public bool Not { get; set; }
    public ICollection<IExpression> List { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanBetweenExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public IExpression Left { get; set; }
    public bool Not { get; set; }
    public IExpression First { get; set; }
    public IExpression Second { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class GroupedBooleanExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public IBooleanExpression Group { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanRangeExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public IExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public RangeOperatorType RangeType { get; set; }
    public SelectStatement Subquery { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanNullCheckExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public IExpression Left { get; set; }
    public bool IsNull { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanComparisonExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public IExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public IExpression Right { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }

  public class BooleanBinaryExpression : IBooleanExpression
  {
    public Token Token { get; set; }
    public IBooleanExpression Left { get; set; }
    public BooleanOperatorType Type { get; set; }
    public IBooleanExpression Right { get; set; }
    public void Accept(ITreeVisitor visitor) { visitor.Visit(this); }
  }
}