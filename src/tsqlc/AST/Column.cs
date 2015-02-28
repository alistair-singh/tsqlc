using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tsqlc.AST
{
  public class Column
  {
    public Expression Expression { get; set; }
    public string Alias { get; set; }
  }
}
