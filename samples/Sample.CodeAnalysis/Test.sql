
DELETE from dbo.tb_Person where name = 'some name'

SELECT  t.Name
		,t.Age
FROM    tb_Account t
		left join tb_Balance b
			ON t.AccountID = b.AccountID
WHERE	t.Name = 'Person with no Name'
		AND b.Balance > 0


SELECT  t.Name
		,Age
FROM    tb_Account t
		inner join tb_AccountInfo a
			ON t.AccountID = a.AccountID
WHERE	t.Name = 'Person with a Name'
		AND a.Balance > 0

SELECT	  Name
		  ,Age
FROM	  tb_Account 

DELETE	  t
FROM    tb_test1 t
		left join tb_test2 b
			ON t.AccountID = b.AccountID
WHERE	t.col1 = 'some string'
		AND b.col2 > 0
