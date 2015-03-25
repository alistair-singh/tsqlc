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
    //TODO: Test where clause
    //TODO: Test expression parsing
    //TODO: Test From clause

    [TestMethod]
    public void EmptyInputYieldsNoTokens()
    {
      var sql = @"   ";
      var ast = sql.Parse();
      Assert.IsTrue(!ast.Any(), "must be empty");
    }

    [TestMethod]
    public void EmptyStatementTest()
    {
      var sql = @"   ; ;";
      var ast = sql.Parse().ToArray();

      Assert.AreEqual(2, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(TerminatedStatement), ast[0].GetType(), "not the right statement");
      Assert.IsTrue((ast[0] as TerminatedStatement).HasTerminator, "not the right statement");
      Assert.AreEqual(typeof(TerminatedStatement), ast[1].GetType(), "not the right statement");
      Assert.IsTrue((ast[1] as TerminatedStatement).HasTerminator, "not the right statement");
    }

    [TestMethod]
    public void BlockTest()
    {
      var sql = @"begin
                  select 1 as n;
                  delete tb_table
                end";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(BlockStatement), statement.GetType(), "not the right statement");

      Assert.AreEqual(typeof(SelectStatement), statement.Body[0].GetType(), "not the right statement");
      Assert.IsTrue(statement.Body[0].HasTerminator, "not the right statement");
      Assert.AreEqual(typeof(DeleteStatement), statement.Body[1].GetType(), "not the right statement");
      Assert.IsFalse(statement.Body[1].HasTerminator, "not the right statement");
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

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statement.GetType(), "not a select statement");
      Assert.IsTrue(statement.TopExpression.Group.Type == BinaryType.Multiply &&
         statement.TopExpression.Group.Left.Value == 100 &&
         statement.TopExpression.Group.Right.Value == 10, "top expression failed");

      Assert.AreEqual(2, statement.ColumnList.Count, "invalid number of columns");
      Assert.AreEqual(typeof(StarColumn), statement.ColumnList[0].GetType(), "first column not '*'");
      Assert.AreEqual(typeof(NullExpression), statement.ColumnList[1].Expression.GetType(), "second column not null");
      Assert.AreEqual("col1", statement.ColumnList[1].Alias, "second column incorrect alias");

      //TODO: Finish unit test for where clause and and from list
      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void SelectReferenceTest()
    {
      var sql = @"select x.x.x.x, xxx.xxx, ......x. x  . x, x . x. x.  x";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statement.GetType(), "not a select statement");
      Assert.AreEqual(statement.ColumnList.Count, 4, "must have x columns");

      Assert.AreEqual("x.x.x.x", statement.ColumnList[0].Expression.Identifier, "reference not equal");
      Assert.AreEqual("xxx.xxx", statement.ColumnList[1].Expression.Identifier, "reference not equal");
      Assert.AreEqual("......x.x.x", statement.ColumnList[2].Expression.Identifier, "reference not equal");
      Assert.AreEqual("x.x.x.x", statement.ColumnList[3].Expression.Identifier, "reference not equal");

      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void SelectThreeStatementsTest()
    {
      var sql = @"select 1 as n select 5; select 9";

      var ast = sql.Parse().ToArray();
      dynamic statements = ast;

      Assert.AreEqual(3, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statements[0].GetType(), "not a select statement");
      Assert.AreEqual(typeof(SelectStatement), statements[1].GetType(), "not a select statement");
      Assert.AreEqual(typeof(SelectStatement), statements[2].GetType(), "not a select statement");
      Assert.AreEqual(1, statements[0].ColumnList.Count, "must have x columns");
      Assert.AreEqual(1, statements[1].ColumnList.Count, "must have x columns");
      Assert.AreEqual(1, statements[2].ColumnList.Count, "must have x columns");

      Assert.AreEqual(1, statements[0].ColumnList[0].Expression.Value, "value not equal");
      Assert.AreEqual("n", statements[0].ColumnList[0].Alias, "value not equal");
      Assert.IsFalse(statements[0].HasTerminator, "value not equal");
      Assert.AreEqual(5, statements[1].ColumnList[0].Expression.Value, "value not equal");
      Assert.IsTrue(statements[1].HasTerminator, "value not equal");
      Assert.AreEqual(9, statements[2].ColumnList[0].Expression.Value, "value not equal");
      Assert.IsFalse(statements[2].HasTerminator, "value not equal");
    }

    [TestMethod]
    public void ExpressionTest()
    {
      var sql = @"select dbo.fn_(1, 'sda') as t, 1*-2+5 as b, -(2323) as c";

      var ast = sql.Parse().ToArray();
      dynamic statements = ast;

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statements[0].GetType(), "not a select statement");
      Assert.AreEqual(3, statements[0].ColumnList.Count, "must have x columns");

      Assert.AreEqual("dbo.fn_", statements[0].ColumnList[0].Expression.Function.Identifier, "function name incorrect");
      Assert.AreEqual(2, statements[0].ColumnList[0].Expression.Parameters.Count, "function parameters incorrect");
      Assert.AreEqual(1, statements[0].ColumnList[0].Expression.Parameters[0].Value, "function parameters incorrect");
      Assert.AreEqual("sda", statements[0].ColumnList[0].Expression.Parameters[1].Value, "function parameters incorrect");
      Assert.AreEqual("t", statements[0].ColumnList[0].Alias, "value not equal");

      Assert.AreEqual(1, statements[0].ColumnList[1].Expression.Left.Left.Value, "value not equal");
      Assert.AreEqual(BinaryType.Addition, statements[0].ColumnList[1].Expression.Type, "value not equal");
      Assert.AreEqual(5, statements[0].ColumnList[1].Expression.Right.Value, "value not equal");
      Assert.AreEqual(BinaryType.Multiply, statements[0].ColumnList[1].Expression.Left.Type, "value not equal");
      Assert.AreEqual(2, statements[0].ColumnList[1].Expression.Left.Right.Right.Value, "value not equal");
      Assert.AreEqual(UnaryType.Negative, statements[0].ColumnList[1].Expression.Left.Right.Type, "value not equal");
      Assert.AreEqual("b", statements[0].ColumnList[1].Alias, "value not equal");

      Assert.AreEqual(UnaryType.Negative, statements[0].ColumnList[2].Expression.Type, "value not equal");
      Assert.AreEqual(2323, statements[0].ColumnList[2].Expression.Right.Group.Value, "value not equal");
      Assert.AreEqual("c", statements[0].ColumnList[2].Alias, "value not equal");

      Assert.IsFalse(statements[0].HasTerminator, "value not equal");
    }

    [TestMethod]
    public void SelectStatementSingleColumnNoFromTest()
    {
      var sql = @"select 1 as col1";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(SelectStatement), statement.GetType(), "not a select statement");

      Assert.AreEqual(1, statement.ColumnList.Count, "invalid number of columns");
      Assert.AreEqual(typeof(ExpressionColumn), statement.ColumnList[0].GetType(), "column not expression");
      Assert.AreEqual(typeof(ConstantExpression), statement.ColumnList[0].Expression.GetType(), "column not constant");
      Assert.AreEqual(1, statement.ColumnList[0].Expression.Value, "column not correct value");
      Assert.AreEqual("col1", statement.ColumnList[0].Alias, "column incorrect alias");

      //TODO: Finish unit test for where clause and and from list
      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void IfStatementAcidTest()
    {
      var sql = @"if 1 = 0 select 1 else select 0";

      var ast = sql.Parse().ToArray();
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

      var ast = sql.Parse().ToArray();
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
      var sql = @"while 1 = 1 select 4;";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(WhileStatement), statement.GetType(), "not a statement type");

      Assert.IsTrue(statement.Test.Left.Value == 1 &&
        statement.Test.Right.Value == 1 &&
        statement.Test.Type == BooleanOperatorType.Equals);

      Assert.IsTrue(statement.Body != null);

      Assert.IsTrue(statement.Body.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void DeleteStatementAcidTest()
    {
      var sql = @"delete top (5) from x from xxxx x (TABLOCKX) where 1 = 2";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(DeleteStatement), statement.GetType(), "not a delete statement");
      Assert.AreEqual(5, statement.TopExpression.Group.Value, "top expression failed");

      Assert.AreEqual("x", statement.Target.Name.Identifier, "invalid target");
      Assert.AreEqual(1, statement.FromList.Count, "invalid from list");
      Assert.AreEqual("xxxx", statement.FromList[0].Name.Identifier, "invalid from list");
      Assert.AreEqual("x", statement.FromList[0].Alias, "invalid from list");

      Assert.AreEqual(BooleanOperatorType.Equals, statement.WhereClause.Type, "invalid where clause");
      Assert.AreEqual(2, statement.WhereClause.Right.Value, "invalid where clause");
      Assert.AreEqual(1, statement.WhereClause.Left.Value, "invalid where clause");

      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void DeleteStatementBasic()
    {
      var sql = @"delete from x where 1 = 2;";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(DeleteStatement), statement.GetType(), "not a delete statement");
      Assert.AreEqual(null, statement.TopExpression, "top expression failed");

      Assert.AreEqual("x", statement.Target.Name.Identifier, "invalid target");
      Assert.AreEqual(0, statement.FromList.Count, "invalid from list");

      Assert.AreEqual(BooleanOperatorType.Equals, statement.WhereClause.Type, "invalid where clause");
      Assert.AreEqual(2, statement.WhereClause.Right.Value, "invalid where clause");
      Assert.AreEqual(1, statement.WhereClause.Left.Value, "invalid where clause");

      Assert.IsTrue(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void UpdateStatementBasic()
    {
      var sql = @"update top 5 dbo.xxx set val1 = 1, val2 = 3 where val2 <> 4";

      var ast = sql.Parse().ToArray();
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

      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void UpdateStatementCompound()
    {
      var sql = @"update top 5 dbo.xxx set val1 = 1, val2 = 3 
                  from dbo.xxx inner join yyy on val1 <= val5
                  where val2 <> 4;";

      var ast = sql.Parse().ToArray();
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

      Assert.IsTrue(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void TestInsertBasic()
    {
      var sql = @"insert dbo.tb_test with(tablockx) values (1,2,'ss')";

      var ast = sql.Parse().ToArray();
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

      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void TestInsertWithTop()
    {
      var sql = @"insert top (3443) dbo.tb_test with(tablockx) values (1,2,'ss')";

      var ast = sql.Parse().ToArray();
      dynamic statement = ast.FirstOrDefault();

      Assert.AreEqual(1, ast.Length, "must only contain one statement");
      Assert.AreEqual(typeof(ValuesInsertStatement), statement.GetType(), "not the right statement");
      Assert.AreEqual(3443, statement.TopExpression.Group.Value, "top expression failed");

      Assert.AreEqual(0, statement.ColumnSpecification.Count, "incorrect number of columns specified");
      Assert.AreEqual("dbo.tb_test", statement.Target.Name.Identifier, "incorrect number of columns specified");
      Assert.AreEqual(1, statement.Target.Hints.Count, "incorrect number of hints");
      Assert.AreEqual(TableHint.TABLOCKX, statement.Target.Hints[0], "incorrect hints");
      Assert.AreEqual(1, statement.Values.Rows.Count, "incorrect number of values");
      Assert.AreEqual(3, statement.Values.Rows[0].Expressions.Count, "incorrect number of expressions");
      Assert.AreEqual(1, statement.Values.Rows[0].Expressions[0].Value, "incorrect values expression");
      Assert.AreEqual(2, statement.Values.Rows[0].Expressions[1].Value, "incorrect values expression");
      Assert.AreEqual("ss", statement.Values.Rows[0].Expressions[2].Value, "incorrect values expression");

      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void TestInsertWithColumnList()
    {
      var sql = @"insert into dbo.tb_test with(tablockx)(xx,yy,zz) values (1,2,'ss');";

      var ast = sql.Parse().ToArray();
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

      Assert.IsTrue(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void TestInsertWithMultipleValues()
    {
      var sql = @"insert dbo.tb_test with(tablockx) values (1,2,'ss'),(3,4,'22')";

      var ast = sql.Parse().ToArray();
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

      Assert.IsFalse(statement.HasTerminator, "does not have terminator");
    }

    [TestMethod]
    public void TestInsertWithSelect()
    {
      var sql = @"insert dbo.tb_test with(tablockx) (aa,bb,cc) select col1, col2, col3 from tb_y;";

      var ast = sql.Parse().ToArray();
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

      Assert.IsTrue(statement.HasTerminator, "does not have terminator");
    }
  }
}
