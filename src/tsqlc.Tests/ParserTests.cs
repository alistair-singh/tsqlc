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
    public void EmptyInputYieldsNoTokens()
    {
      var sql = @"   ";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens);
      Assert.IsTrue(!ast.Any(), "must be empty");
    }

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
    public void SelectReferenceTest()
    {
      var sql = @"select x.x.x.x, xxx.xxx, ......x. x  . x, x . x. x.  x";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statement.GetType(), "not a select statement");
      Assert.AreEqual(statement.ColumnList.Count, 4, "must have x columns");

      Assert.AreEqual("x.x.x.x", statement.ColumnList[0].Expression.Identifier, "reference not equal");
      Assert.AreEqual("xxx.xxx", statement.ColumnList[1].Expression.Identifier, "reference not equal");
      Assert.AreEqual("......x.x.x", statement.ColumnList[2].Expression.Identifier, "reference not equal");
      Assert.AreEqual("x.x.x.x", statement.ColumnList[3].Expression.Identifier, "reference not equal");
    }

    [TestMethod]
    public void SelectStatementSingleColumnNoFromTest()
    {
      var sql = @"select 1 as col1";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statement.GetType(), "not a select statement");

      Assert.AreEqual(1, statement.ColumnList.Count, "invalid number of columns");
      Assert.AreEqual(typeof(ExpressionColumn), statement.ColumnList[0].GetType(), "column not expression");
      Assert.AreEqual(typeof(ConstantExpression), statement.ColumnList[0].Expression.GetType(), "column not constant");
      Assert.AreEqual(1, statement.ColumnList[0].Expression.Value, "column not correct value");
      Assert.AreEqual("col1", statement.ColumnList[0].Alias, "column incorrect alias");

      //TODO: Finish unit test for where clause and and from list
    }

    [TestMethod]
    public void IfStatementAcidTest()
    {
      var sql = @"if 1 = 0 select 1 else select 0";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(IfStatement), statement.GetType(), "not a statement type");

      Assert.IsTrue(statement.Test.Left.Value == 1 &&
        statement.Test.Right.Value == 0 &&
        statement.Test.Type == BooleanOperatorType.Equals);

      Assert.IsTrue(statement.TrueBody != null &&
        statement.FalseBody != null);
    }

    [TestMethod]
    public void IfStatementNoElse()
    {
      var sql = @"if 1 = 0 select 1 ";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(IfStatement), statement.GetType(), "not a statement type");

      Assert.IsTrue(statement.Test.Left.Value == 1 &&
        statement.Test.Right.Value == 0 &&
        statement.Test.Type == BooleanOperatorType.Equals);

      Assert.IsTrue(statement.TrueBody != null &&
        statement.FalseBody == null);
    }

    [TestMethod]
    public void WhileStatementAcidTest()
    {
      var sql = @"while 1 = 1 select 4";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(WhileStatement), statement.GetType(), "not a statement type");

      Assert.IsTrue(statement.Test.Left.Value == 1 &&
        statement.Test.Right.Value == 1 &&
        statement.Test.Type == BooleanOperatorType.Equals);

      Assert.IsTrue(statement.Body != null);
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

    [TestMethod]
    public void UpdateStatementBasic()
    {
      var sql = @"update top 5 dbo.xxx set val1 = 1, val2 = 3 where val2 <> 4";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(UpdateStatement), statement.GetType(), "not a delete statement");
      Assert.AreEqual(5, statement.TopExpression.Value, "top expression failed");

      Assert.AreEqual(2, statement.SetColumnList.Count, "wrong number of sets");
      Assert.AreEqual("val1", statement.SetColumnList[0].Reference.Identifier, "wrong identifier");
      Assert.AreEqual(1, statement.SetColumnList[0].Expression.Value, "wrong identifier");
      Assert.AreEqual("val2", statement.SetColumnList[1].Reference.Identifier, "wrong identifier");
      Assert.AreEqual(3, statement.SetColumnList[1].Expression.Value, "wrong identifier");

      Assert.AreEqual("dbo.xxx", statement.Target.Name.Identifier, "incorrect identifier");
      Assert.AreEqual("val2", statement.WhereClause.Left.Identifier, "incorrect where clause");
      Assert.AreEqual(4, statement.WhereClause.Right.Value, "incorrect where clause");
      Assert.AreEqual(BooleanOperatorType.NotEqual, statement.WhereClause.Type, "incorrect where clause");
    }

    [TestMethod]
    public void UpdateStatementCompound()
    {
      var sql = @"update top 5 dbo.xxx set val1 = 1, val2 = 3 
                  from dbo.xxx inner join yyy on val1 <= val5
                  where val2 <> 4";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(UpdateStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(5, statement.TopExpression.Value, "top expression failed");

      Assert.AreEqual(2, statement.SetColumnList.Count, "wrong number of sets");
      Assert.AreEqual("val1", statement.SetColumnList[0].Reference.Identifier, "wrong identifier");
      Assert.AreEqual(1, statement.SetColumnList[0].Expression.Value, "wrong identifier");
      Assert.AreEqual("val2", statement.SetColumnList[1].Reference.Identifier, "wrong identifier");
      Assert.AreEqual(3, statement.SetColumnList[1].Expression.Value, "wrong identifier");

      Assert.AreEqual(2, statement.FromList.Count, "wrong number of sets");
      Assert.AreEqual("dbo.xxx", statement.FromList[0].Name.Identifier, "invalid from list");
      Assert.AreEqual("yyy", statement.FromList[1].Name.Identifier, "invalid from list");
      Assert.AreEqual(JoinType.INNER, statement.FromList[1].Join, "invalid join list");
      Assert.AreEqual("val1", statement.FromList[1].OnClause.Left.Identifier, "invalid identifier");
      Assert.AreEqual(BooleanOperatorType.LessThanOrEqual, statement.FromList[1].OnClause.Type, "invalid identifier");
      Assert.AreEqual("val5", statement.FromList[1].OnClause.Right.Identifier, "invalid identifier");

      Assert.AreEqual("dbo.xxx", statement.Target.Name.Identifier, "incorrect identifier");
      Assert.AreEqual("val2", statement.WhereClause.Left.Identifier, "incorrect where clause");
      Assert.AreEqual(4, statement.WhereClause.Right.Value, "incorrect where clause");
      Assert.AreEqual(BooleanOperatorType.NotEqual, statement.WhereClause.Type, "incorrect where clause");
    }

    [TestMethod]
    public void TestInsertBasic()
    {
      var sql = @"insert dbo.tb_test with(tablockx) values (1,2,'ss')";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(ValuesInsertStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(null, statement.TopExpression, "top expression failed");

      Assert.AreEqual(0, statement.ColumnSpecification.Count, "incorrect number of columns specified");
      Assert.AreEqual("dbo.tb_test", statement.Target.Name.Identifier, "incorrect number of columns specified");
      Assert.AreEqual(1, statement.Target.Hints.Count, "incorrect number of hints");
      Assert.AreEqual(TableHint.TABLOCKX, statement.Target.Hints[0], "incorrect hints");
      Assert.AreEqual(1, statement.Values.Rows.Count, "incorrect number of values");
      Assert.AreEqual(3, statement.Values.Rows[0].Expressions.Count, "incorrect number of expressions");
      Assert.AreEqual(1, statement.Values.Rows[0].Expressions[0].Value, "incorrect values expression");
      Assert.AreEqual(2, statement.Values.Rows[0].Expressions[1].Value, "incorrect values expression");
      Assert.AreEqual("ss", statement.Values.Rows[0].Expressions[2].Value, "incorrect values expression");
    }

    [TestMethod]
    public void TestInsertWithTop()
    {
      var sql = @"insert top (3443) dbo.tb_test with(tablockx) values (1,2,'ss')";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(ValuesInsertStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(3443, statement.TopExpression.Value, "top expression failed");

      Assert.AreEqual(0, statement.ColumnSpecification.Count, "incorrect number of columns specified");
      Assert.AreEqual("dbo.tb_test", statement.Target.Name.Identifier, "incorrect number of columns specified");
      Assert.AreEqual(1, statement.Target.Hints.Count, "incorrect number of hints");
      Assert.AreEqual(TableHint.TABLOCKX, statement.Target.Hints[0], "incorrect hints");
      Assert.AreEqual(1, statement.Values.Rows.Count, "incorrect number of values");
      Assert.AreEqual(3, statement.Values.Rows[0].Expressions.Count, "incorrect number of expressions");
      Assert.AreEqual(1, statement.Values.Rows[0].Expressions[0].Value, "incorrect values expression");
      Assert.AreEqual(2, statement.Values.Rows[0].Expressions[1].Value, "incorrect values expression");
      Assert.AreEqual("ss", statement.Values.Rows[0].Expressions[2].Value, "incorrect values expression");
    }

    [TestMethod]
    public void TestInsertWithColumnList()
    {
      var sql = @"insert into dbo.tb_test with(tablockx)(xx,yy,zz) values (1,2,'ss')";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(ValuesInsertStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(null, statement.TopExpression, "top expression failed");

      Assert.AreEqual(3, statement.ColumnSpecification.Count, "incorrect number of columns specified");
      Assert.AreEqual("xx", statement.ColumnSpecification[0].Identifier, "incorrect columns specified");
      Assert.AreEqual("yy", statement.ColumnSpecification[1].Identifier, "incorrect columns specified");
      Assert.AreEqual("zz", statement.ColumnSpecification[2].Identifier, "incorrect columns specified");
      Assert.AreEqual("dbo.tb_test", statement.Target.Name.Identifier, "incorrect number of columns specified");
      Assert.AreEqual(1, statement.Target.Hints.Count, "incorrect number of hints");
      Assert.AreEqual(TableHint.TABLOCKX, statement.Target.Hints[0], "incorrect hints");
      Assert.AreEqual(1, statement.Values.Rows.Count, "incorrect number of values");
      Assert.AreEqual(3, statement.Values.Rows[0].Expressions.Count, "incorrect number of expressions");
      Assert.AreEqual(1, statement.Values.Rows[0].Expressions[0].Value, "incorrect values expression");
      Assert.AreEqual(2, statement.Values.Rows[0].Expressions[1].Value, "incorrect values expression");
      Assert.AreEqual("ss", statement.Values.Rows[0].Expressions[2].Value, "incorrect values expression");
    }

    [TestMethod]
    public void TestInsertWithMultipleValues()
    {
      var sql = @"insert dbo.tb_test with(tablockx) values (1,2,'ss'),(3,4,'22')";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(ValuesInsertStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(null, statement.TopExpression, "top expression failed");

      Assert.AreEqual(0, statement.ColumnSpecification.Count, "incorrect number of columns specified");
      Assert.AreEqual("dbo.tb_test", statement.Target.Name.Identifier, "incorrect number of columns specified");
      Assert.AreEqual(1, statement.Target.Hints.Count, "incorrect number of hints");
      Assert.AreEqual(TableHint.TABLOCKX, statement.Target.Hints[0], "incorrect hints");
      Assert.AreEqual(2, statement.Values.Rows.Count, "incorrect number of values");
      Assert.AreEqual(3, statement.Values.Rows[0].Expressions.Count, "incorrect number of expressions");
      Assert.AreEqual(1, statement.Values.Rows[0].Expressions[0].Value, "incorrect values expression");
      Assert.AreEqual(2, statement.Values.Rows[0].Expressions[1].Value, "incorrect values expression");
      Assert.AreEqual("ss", statement.Values.Rows[0].Expressions[2].Value, "incorrect values expression");
      Assert.AreEqual(3, statement.Values.Rows[1].Expressions[0].Value, "incorrect values expression");
      Assert.AreEqual(4, statement.Values.Rows[1].Expressions[1].Value, "incorrect values expression");
      Assert.AreEqual("22", statement.Values.Rows[1].Expressions[2].Value, "incorrect values expression");
    }

    [TestMethod]
    public void TestInsertWithSelect()
    {
      var sql = @"insert dbo.tb_test with(tablockx) (aa,bb,cc) select col1, col2, col3 from tb_y";

      var tokens = new Lexer(sql);
      var ast = new Parser(tokens).ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectInsertStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(null, statement.TopExpression, "top expression failed");

      Assert.AreEqual(3, statement.ColumnSpecification.Count, "incorrect number of columns specified");
      Assert.AreEqual("aa", statement.ColumnSpecification[0].Identifier, "incorrect columns specified");
      Assert.AreEqual("bb", statement.ColumnSpecification[1].Identifier, "incorrect columns specified");
      Assert.AreEqual("cc", statement.ColumnSpecification[2].Identifier, "incorrect columns specified");
      Assert.AreEqual("dbo.tb_test", statement.Target.Name.Identifier, "incorrect number of columns specified");
      Assert.AreEqual(1, statement.Target.Hints.Count, "incorrect number of hints");
      Assert.AreEqual(TableHint.TABLOCKX, statement.Target.Hints[0], "incorrect hints");
      Assert.AreEqual(3, statement.SelectStatement.ColumnList.Count, "invalid columns");
      Assert.AreEqual("col1", statement.SelectStatement.ColumnList[0].Expression.Identifier, "invalid columns");
      Assert.AreEqual("col2", statement.SelectStatement.ColumnList[1].Expression.Identifier, "invalid columns");
      Assert.AreEqual("col3", statement.SelectStatement.ColumnList[2].Expression.Identifier, "invalid columns");
      Assert.AreEqual(1, statement.SelectStatement.FromList.Count, "invalid from");
      Assert.AreEqual("tb_y", statement.SelectStatement.FromList[0].Name.Identifier, "invalid from");
    }
  }
}
