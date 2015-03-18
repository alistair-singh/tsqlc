using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using tsqlc.AST;
using tsqlc.Parse;

namespace tsqlc.Tests
{
  [TestClass]
  public class ParserTests
  {
    [TestMethod]
    public void SelectStatementAcidTest()
    {
      var sql = @"select  top (100*10) *, NULL AS col1
                  from    tb_test as t with (nolock) 
                          inner join tb_test2 t2 
                            on t1.id = t2.id
                          join t3 on 1=2
                          full join t3 on 0=3
                          left join t3 on 0=2
                          right join t3 on 0=1
                          full outer join t4 on 333=4
                          left outer join t4 on x =x
                          right outer join t4 on y = z
                  where   name like 'alistair%'
                          and name is not null
                          or age is null
                          or not age is null";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statement.GetType(), "not a select statement");
      Assert.IsTrue(statement.TopExpression.Type == BinaryType.Multiply &&
         statement.TopExpression.Left.Value == 100 &&
         statement.TopExpression.Right.Value == 10, "top expression failed");

      Assert.AreEqual(2, statement.ColumnList.Count, "invalid number of columns");
      Assert.AreEqual(typeof(StarColumn), statement.ColumnList[0].GetType(), "first column not '*'");
      Assert.AreEqual(typeof(NullExpression), statement.ColumnList[1].Expression.GetType(), "second column not null");
      Assert.AreEqual("col1", statement.ColumnList[1].Alias, "second column incorrect alias");

      //TODO: Finish unit test for where clause and and from list
    }

    [TestMethod]
    public void DeleteStatementAcidTest()
    {
      var sql = @"delete top (5) from x from xxxx x (TABLOCKX) where 1 = 2";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(DeleteStatement), statement.GetType(), "not a delete statement");
      Assert.AreEqual(5, statement.TopExpression.Value, "top expression failed");

      Assert.AreEqual("x", statement.Target.Name.Identifier, "invalid target");
      Assert.AreEqual("xxxx", statement.FromList[0].Name.Identifier, "invalid from list");
      Assert.AreEqual("x", statement.FromList[0].Alias, "invalid from list");

      Assert.AreEqual(BooleanOperatorType.Equals, statement.WhereClause.Type, "invalid where clause");
      Assert.AreEqual(2, statement.WhereClause.Right.Value, "invalid where clause");
      Assert.AreEqual(1, statement.WhereClause.Left.Value, "invalid where clause");
    }
  }
}
