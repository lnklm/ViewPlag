CREATE PROC spDeleteDiscipline
@id_discipline int
AS
BEGIN
	DELETE FROM Discipline
	WHERE id_discipline = @id_discipline;
END;


GO
CREATE PROC spDeleteLab
@id_discipline int,
@id_lab int
AS
BEGIN
	DELETE FROM Lab_work
	WHERE Id_lab = @id_lab AND id_discipline = @id_discipline;
END;

GO
CREATE PROC spDeleteAll
AS
BEGIN
	DELETE FROM Discipline
	DELETE FROM Student	
END;

GO
CREATE PROC spInsertStudent
@name nvarchar(50), 
@surname nvarchar(50), 
@patronymic nvarchar(50), 
@class nvarchar(4), 
@gradebook nvarchar(2)
AS
BEGIN
	INSERT INTO Student (Name, Surname, Patronymic, Class, Gradebook)
	VALUES(@name, @surname, @patronymic, @class, @gradebook);
END;

GO
CREATE PROC spInsertLab
@lab_number nvarchar(3),
@id_discipline int
AS
BEGIN
	INSERT INTO Lab_work (Lab_number, Id_discipline)  
    VALUES(@lab_number, @id_discipline);
END;

GO
CREATE PROC spInsertPlagiarism
@id_discipline int,
@id_lab int, 
@id_student1 int, 
@id_student2 int,
@percent1 tinyint,
@percent2 tinyint,
@lines smallint
AS
BEGIN
	INSERT INTO Plagiarism (Id_discipline, Id_lab, Id_student1, Id_student2, Percent1, Percent2, Lines)
	VALUES(@id_discipline, @id_lab, @id_student1, @id_student2, @percent1, @percent2, @lines);
END;

GO
CREATE PROC spInsertDiscipline
@nickname nvarchar(10), 
@fullname nvarchar(250), 
@course tinyint
AS
BEGIN
	INSERT INTO Discipline (Nickname, Fullname, Course)
	VALUES(@nickname, @fullname, @course);
END;

GO
CREATE PROC spSeekStudentId
@class nvarchar(4), 
@gradebook nvarchar(2)
AS
BEGIN
	SELECT Id_student FROM Student
	WHERE Class = @class AND Gradebook = @gradebook;
END;

GO
CREATE PROC spSeekLabId
@id_discipline int,
@lab_number nvarchar(3)
AS
BEGIN
	SELECT Id_lab FROM Lab_work
	WHERE id_discipline = @id_discipline AND lab_number = @lab_number;
END;

GO
CREATE PROC spStudPlagList
@discId NVARCHAR(5)
AS
BEGIN
DECLARE @cols as NVARCHAR(MAX),
	@query as NVARCHAR(MAX)

SELECT @cols = COALESCE(@cols + ', ', '') + '[' + CAST(Lab_number AS VARCHAR(20)) + ']'
FROM Lab_work 
WHERE Id_discipline = @discId
ORDER BY Lab_number

SET @query = '
SELECT S.Id_student, S.Surname, S.Name, S.Patronymic, S.Course, S.Class, S.Gradebook as [#], ' + @cols + '
FROM Student S
JOIN (
	SELECT *
	FROM(
			SELECT B.Id_student, L.Lab_number, B."%"
			FROM(
				SELECT Id_student, Id_lab, MAX("%") as "%"
				FROM(
					SELECT Id_plagiarism, Id_discipline, Id_lab, Id_student1 as Id_student, Cast(Percent1 as int) as "%", Lines
					FROM Plagiarism
					WHERE Id_discipline =' + @discId + '
					UNION
					SELECT Id_plagiarism, Id_discipline, Id_lab, Id_student2 as Id_student, Cast(Percent2 as int) as "%", Lines
					FROM Plagiarism
					WHERE Id_discipline =' + @discId + '
				) as A
				GROUP BY Id_student, Id_lab, Id_discipline
			) as B
			JOIN Lab_work as L
			ON L.Id_lab = B.Id_lab
		) as C
	PIVOT(
		SUM("%")
		FOR Lab_number IN 	
		(' + @cols + ')
	) as pivotTable
) R
ON R.Id_student = S.Id_student'
execute sp_executesql @query;
END;

GO
CREATE PROC spViewDiscipline
AS
BEGIN
	SELECT Id_discipline, Nickname 
	FROM Discipline;
END;

GO
CREATE PROC spViewLab
@id_discipline int
AS
BEGIN
	SELECT Id_lab, Lab_number FROM Lab_work  
    WHERE Lab_work.Id_discipline = @id_discipline 
    ORDER BY Lab_number ASC;
END;

GO
CREATE PROC spViewCourse
AS
BEGIN
	SELECT DISTINCT Course FROM Student
END;

GO
CREATE PROC spPlagByCell
@id_student int,
@id_discipline int,
@lab int
AS
BEGIN
	DECLARE @id_lab int;
	SELECT @id_lab = Id_lab
	FROM Lab_work 
	WHERE Lab_number = @lab AND Id_discipline = @id_discipline;

	SELECT S.Surname,P.Percent2 as "%",P.Lines 
	FROM Plagiarism as P
	JOIN Student as S
	ON P.Id_student1 = S.Id_student AND P.Id_student2 = @id_student
	AND P.Id_discipline = @id_discipline AND P.Id_lab = @id_lab
	UNION
	SELECT S.Surname,P.Percent1 as "%",P.Lines 
	FROM Plagiarism as P
	JOIN Student as S
	ON P.Id_student2 = S.Id_student AND P.Id_student1 = @id_student
	AND P.Id_discipline = @id_discipline AND P.Id_lab = @id_lab
	ORDER BY [%] DESC, Lines DESC
END;

GO
CREATE PROC spStudentClassByCourse
@Course nvarchar(3)
AS
BEGIN
	if @Course = 'All'
		SELECT DISTINCT Class 
		FROM Student 
	ELSE
		SELECT DISTINCT Class 
		FROM Student 	
		WHERE Course = @Course
END;


