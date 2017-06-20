CREATE TRIGGER tr_StudentCourse
ON Student
INSTEAD OF INSERT
AS
BEGIN
		
	DECLARE @startTime DATE,
	@y int, 
	@course nvarchar(3);
	SET @startTime = '2000-09-01';
	SET @startTime = DATEADD(YEAR,(DATEPART(YEAR,GETDATE()) % 2000) / 10 * 10,@startTime);

	SELECT @y = RIGHT(Class,2)/10 
	FROM inserted

	SET @startTime = DATEADD(YEAR,@y,@startTime);
	SET @y = DATEDIFF(MONTH,@startTime,GETDATE())/12+1;
	IF (@y > 4) SET @course = '---'; 
	ELSE SET @course = @y

	INSERT INTO Student(Name, Surname, Patronymic, Course, Class, Gradebook)
	SELECT Name, Surname, Patronymic, @course AS Course, Class, Gradebook
	FROM inserted;

END;