
SELECT  Name
		,Age
FROM    tb_Person
WHERE	Name = 'Person with no Name'

SELECT	p.Name
	    ,p.Age
		,c.Name
FROM	tb_Person p
		INNER JOIN tb_Car c
			ON p.PersonId = c.PersonId

insert tb_Friend (Name, Age, AnotherName)
SELECT	p.Name
	    ,p.Age
		,c.Name
FROM	tb_Person p
		INNER JOIN tb_Car c
			ON p.PersonId = c.PersonId

delete x from tb_person x inner join tb_car u on x.personid = u.personid where x.name like 'x%'
delete u from tb_person x inner join tb_car u on x.personid = u.personid where x.name like 'x%'
/*
	tb_Person

	Name
*/

UPDATE x
SET    x.Name = 'Toyota'
FROM   dbo.tb_Person x
		INNER JOIN dbo.tb_Friend f
			ON f.PersonId = x.PersonID
		INNER JOIN dbo.tb_person y
		 ON y.FriendID = y.PersonID
WHERE y.Name LIKE 'A%' AND y.age > 20 

DELETE 
FROM   tb_Car
WHERE  PersonID = 2

insert tb_Car (name, model, year)
values (1,2,3)

SELECT Name
	   ,Age
FROM   tb_Person p
		INNER JOIN tb_Friend f
		 ON f.PersonId = o.PersonID

 UPDATE tb_Car
 SET	Name = 'Mercedes'
