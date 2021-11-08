CREATE TABLE Orders(
	Id int IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	CustomerName nvarchar(50)
);

CREATE TABLE Orderlines(
	Id int IDENTITY(1,1) PRIMARY KEY NOT NULL, 
	OrderId int FOREIGN KEY REFERENCES Orders(Id) NOT NULL, 
	Product nvarchar(50) NOT NULL, 
	UnitPrice decimal(18,2) NOT NULL, 
	Quantity int DEFAULT(1)
);

