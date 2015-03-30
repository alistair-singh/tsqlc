
/*
	Some procedure by me
*/
--CREATE PROCEDURE dbo.pr_SomeProcedure 
--AS 

-- log that this information was retreived
INSERT tb_Log
VALUES ('name selected', getdate(), SUSER_NAME())

SELECT  t.Name
		,t.Age
FROM    tb_Account t
WHERE	t.Name = 'Person with no Name'
