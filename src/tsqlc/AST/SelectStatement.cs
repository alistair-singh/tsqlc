﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace tsqlc.AST
{
  public class SelectStatement : Statement
  {
    public ICollection<Column> Columns { get; set; }
  }
}
