CREATE TABLE Student
(
	Id_student int IDENTITY(1,1) NOT NULL UNIQUE,
	Name nvarchar(50) NOT NULL,
	Surname nvarchar(50) NOT NULL,
	Patronymic nvarchar(50),
	Course nvarchar(3),
	Class nvarchar(4) NOT NULL,
	Gradebook nvarchar(2) NOT NULL,
	CONSTRAINT [pk_StudentID] PRIMARY KEY (Class,Gradebook), 
	CONSTRAINT [ch_Student] CHECK (Course in ( '1','2','3','4','---') AND Id_student > 0) 
);

CREATE TABLE Discipline
(
	Id_discipline int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Nickname nvarchar(10) NOT NULL,
	Fullname nvarchar(250) NOT NULL,
	Course tinyint
	CONSTRAINT [ch_Discipline] CHECK (Course > 0 AND Course < 5)
);

CREATE TABLE Lab_work
(
	Id_lab int IDENTITY(1,1) NOT NULL UNIQUE,
	Lab_number nvarchar(3) NOT NULL, 
	Id_discipline int NOT NULL,
	CONSTRAINT [pk_LabID] PRIMARY KEY (Lab_number,Id_discipline),
	CONSTRAINT [fk_Lab] FOREIGN KEY (Id_discipline) REFERENCES dbo.Discipline(Id_discipline) ON DELETE CASCADE
);

CREATE TABLE Plagiarism
(
	Id_plagiarism int IDENTITY(1,1) NOT NULL PRIMARY KEY,
	Id_discipline int NOT NULL,
	Id_lab int NOT NULL,
	Id_student1 int NOT NULL FOREIGN KEY REFERENCES dbo.Student(Id_student),
	Id_student2 int NOT NULL FOREIGN KEY REFERENCES dbo.Student(Id_student),
	Percent1 tinyint NOT NULL,
	Percent2 tinyint NOT NULL,
	Lines smallint NOT NULL,	
	CONSTRAINT [fk_PlagDisc] FOREIGN KEY (Id_discipline) REFERENCES dbo.Discipline(Id_discipline) ON DELETE CASCADE,
	CONSTRAINT [ch_Plagiarism] CHECK (Percent1 >= 0 AND Percent1 <= 100 
								  AND Percent2 >= 0 AND Percent2 <= 100 AND Lines >= 0)
);
