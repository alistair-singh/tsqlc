using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tsqlc.AST
{
  public class Column {}

  public class StarColumn : Column
  {
    public string TableAlias { get; set; }

    public override string ToString()
    {
      return string.Format("StartColumn -> {0}", TableAlias);
    }
  }

  public class ExpressionColumn : Column
  {
    public Expression Expression { get; set; }
    public string Alias { get; set; }

    public override string ToString()
    {
      return string.Format("ExpressionColumn -> {0}", Alias);
    }
  }
}
