CREATE TABLE [dbo].[Table]
(
	[Id] INT NOT NULL PRIMARY KEY, 
    [user_id] VARCHAR(MAX) NOT NULL, 
    [data_inicio] DATE NOT NULL, 
    [data_fim] DATE NULL, 
    [total_dias] INT NULL
)
