SELECT Operation, SUM(Duration)/1000 FROM times Group by Operation

SELECT Module, Operation, SUM(Duration)/1000 FROM times Group by Module, Operation

SELECT Number, Operation, SUM(Duration)/1000 FROM times INNER JOIN query ON times.QueryId = query.Id  Group by Number, Operation

SELECT Number, Module, Operation, SUM(Duration)/1000 FROM times INNER JOIN query ON times.QueryId = query.Id  Group by Number, Module, Operation


SELECT "IO", Operation, SUM(Duration)/1000 FROM times WHERE Module LIKE "IO%" Group by Operation
UNION
SELECT "JOIN", Operation, SUM(Duration)/1000 FROM times WHERE Module LIKE "JOIN%" Group by Operation
UNION
SELECT "SORT", Operation, SUM(Duration)/1000 FROM times WHERE Module LIKE "SORT%" Group by Operation
UNION
SELECT "MGM", Operation, SUM(Duration)/1000 FROM times WHERE Module LIKE "MGM%" Group by Operation


 -- Получение времен по запросам
SELECT 
	Number, 
	d1.Duration AS DataTransfer, 
	d2.Duration AS LoadData, 
	d3.Duration AS ProcessingSelect, 
	d4.Duration AS ProcessingJoin, 
	d5.Duration AS ProcessingSort, 
	d6.Duration AS FileSave 
FROM
	(SELECT Number, Id FROM query) AS q
	LEFT JOIN
	(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DataTransfer" Group by QueryId, Operation) AS d1
	ON q.Id = d1.QueryId
	LEFT JOIN
	(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "LoadData" Group by QueryId, Operation) AS d2
	ON q.Id = d2.QueryId
	LEFT JOIN
	(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSelect" Group by QueryId, Operation) AS d3
	ON q.Id = d3.QueryId
	LEFT JOIN
	(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingJoin" Group by QueryId, Operation) AS d4
	ON q.Id = d4.QueryId
	LEFT JOIN
	(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSort" Group by QueryId, Operation) AS d5
	ON q.Id = d5.QueryId
	LEFT JOIN
	(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "FileSave" Group by QueryId, Operation) AS d6
	ON q.Id = d6.QueryId


 -- Получение среднего времен по запросам
SELECT 
	Number, 
	AVG(DataTransfer) AS DataTransfer, 
	AVG(LoadData) AS LoadData, 
	AVG(ProcessingSelect) AS ProcessingSelect, 
	AVG(ProcessingJoin) AS ProcessingJoin, 
	AVG(ProcessingSort) AS ProcessingSort, 
	AVG(FileSave) AS FileSave, 
	AVG(DeleteData) AS DeleteData
FROM 
	(SELECT 
		Number, 
		d1.Duration AS DataTransfer, 
		d2.Duration AS LoadData, 
		d3.Duration AS ProcessingSelect, 
		d4.Duration AS ProcessingJoin, 
		d5.Duration AS ProcessingSort, 
		d6.Duration AS FileSave, 
		d7.Duration AS DeleteData 
	FROM
		(SELECT Number, Id FROM query) AS q
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DataTransfer" Group by QueryId, Operation) AS d1
		ON q.Id = d1.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "LoadData" Group by QueryId, Operation) AS d2
		ON q.Id = d2.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSelect" Group by QueryId, Operation) AS d3
		ON q.Id = d3.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingJoin" Group by QueryId, Operation) AS d4
		ON q.Id = d4.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSort" Group by QueryId, Operation) AS d5
		ON q.Id = d5.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "FileSave" Group by QueryId, Operation) AS d6
		ON q.Id = d6.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DeleteData" Group by QueryId, Operation) AS d7
		ON q.Id = d7.QueryId
	) AS TIMES
GROUP BY Number
ORDER BY Number;

 -- Получение суммы времен по запросам
SELECT 
	Number, 
	SUM(DataTransfer) AS DataTransfer, 
	SUM(LoadData) AS LoadData, 
	SUM(ProcessingSelect) AS ProcessingSelect, 
	SUM(ProcessingJoin) AS ProcessingJoin, 
	SUM(ProcessingSort) AS ProcessingSort, 
	SUM(FileSave) AS FileSave, 
	SUM(DeleteData) AS DeleteData
FROM 
	(SELECT 
		Number, 
		d1.Duration AS DataTransfer, 
		d2.Duration AS LoadData, 
		d3.Duration AS ProcessingSelect, 
		d4.Duration AS ProcessingJoin, 
		d5.Duration AS ProcessingSort, 
		d6.Duration AS FileSave, 
		d7.Duration AS DeleteData 
	FROM
		(SELECT Number, Id FROM query) AS q
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DataTransfer" Group by QueryId, Operation) AS d1
		ON q.Id = d1.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "LoadData" Group by QueryId, Operation) AS d2
		ON q.Id = d2.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSelect" Group by QueryId, Operation) AS d3
		ON q.Id = d3.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingJoin" Group by QueryId, Operation) AS d4
		ON q.Id = d4.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSort" Group by QueryId, Operation) AS d5
		ON q.Id = d5.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "FileSave" Group by QueryId, Operation) AS d6
		ON q.Id = d6.QueryId
		LEFT JOIN
		(SELECT QueryId, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DeleteData" Group by QueryId, Operation) AS d7
		ON q.Id = d7.QueryId
	) AS TIMES
GROUP BY Number
ORDER BY Number;

 -- Время работы стадий по модулям
 SELECT 
	stages.Operation AS Stage, 
	d1.Duration AS IoDuration, 
	d2.Duration AS JoinDuration, 
	d3.Duration AS SortDuration, 
	d4.Duration AS MgmDuration
FROM
	(SELECT DISTINCT Operation FROM times) AS stages
	LEFT JOIN
	(SELECT Operation, SUM(Duration)/1000 AS Duration FROM times WHERE Module LIKE "IO%" Group by Operation) AS d1
	ON stages.Operation = d1.Operation
	LEFT JOIN
	(SELECT Operation, SUM(Duration)/1000 AS Duration FROM times WHERE Module LIKE "JOIN%" Group by Operation) AS d2
	ON stages.Operation = d2.Operation
	LEFT JOIN
	(SELECT Operation, SUM(Duration)/1000 AS Duration FROM times WHERE Module LIKE "SORT%" Group by Operation) AS d3
	ON stages.Operation = d3.Operation
	LEFT JOIN
	(SELECT Operation, SUM(Duration)/1000 AS Duration FROM times WHERE Module LIKE "MGM%" Group by Operation) AS d4
	ON stages.Operation = d4.Operation
ORDER BY Stage

 -- Время работы каждой стадии по каждому узлу
SELECT 
	nodes.Module AS Node, 
	d1.Duration AS DataTransfer, 
	d2.Duration AS LoadData, 
	d3.Duration AS ProcessingSelect, 
	d4.Duration AS ProcessingJoin, 
	d5.Duration AS ProcessingSort, 
	d6.Duration AS FileSave, 
	d7.Duration AS DeleteData 
FROM
	(SELECT DISTINCT Module FROM times) AS nodes
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DataTransfer" Group by Module, Operation) AS d1
	ON nodes.Module = d1.Module
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "LoadData" Group by Module, Operation) AS d2
	ON nodes.Module = d2.Module
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSelect" Group by Module, Operation) AS d3
	ON nodes.Module = d3.Module
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingJoin" Group by Module, Operation) AS d4
	ON nodes.Module = d4.Module
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "ProcessingSort" Group by Module, Operation) AS d5
	ON nodes.Module = d5.Module
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "FileSave" Group by Module, Operation) AS d6
	ON nodes.Module = d6.Module
	LEFT JOIN
	(SELECT Module, SUM(Duration)/1000 AS Duration FROM times WHERE Operation = "DeleteData" Group by Module, Operation) AS d7
	ON nodes.Module = d7.Module
ORDER BY  Node

-- Количество запросов
SELECT Number, Count(Id) FROM query GROUP BY Number