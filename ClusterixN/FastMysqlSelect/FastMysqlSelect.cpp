// FastMysqlSelect.cpp: определяет экспортированные функции для приложения DLL.
//

#include "stdafx.h"
#include "FastMysqlSelect.h"
#include "mysql_driver.h"
#include "cppconn/resultset.h"
#include "cppconn/statement.h"
#include <fstream>
#include <mutex>
#include <vector>
using namespace std;

// Конструктор для экспортированного класса.
// см. определение класса в FastMysqlSelect.h
CFastMysqlSelect::CFastMysqlSelect(): SelectResult(nullptr), SelectBlocksResult(nullptr)
{
	Error = new string();
	DataBlockCount = 0;
	return;
}

inline char* StringToCharArray(string *str)
{
	auto cstr = new char[str->length()];
	memcpy_s(cstr, str->length(), str->c_str(), str->length());
	return cstr;
}

mutex mysql_driver_mutex;

char* CFastMysqlSelect::FastMysqlSelect(const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int* rowCount)
{
	mysql_driver_mutex.lock();
	auto driver = sql::mysql::get_mysql_driver_instance();
	mysql_driver_mutex.unlock();

	auto con = driver->connect(host, user, pass);
	con->setSchema(dbname);

	auto *stmt = con->createStatement();
	//stmt->setQueryTimeout(86400);
	
	auto *res = stmt->executeQuery(query);
	int colCount = res->getMetaData()->getColumnCount();
	SelectResult = new string();
	*rowCount = res->rowsCount();

	while (res->next())
	{
		for (auto i = 1; i <= colCount; i++)
		{
			if (res->isNull(i))
			{
				SelectResult->append("NULL");
			}
			else
			{
				SelectResult->append("\"");
				SelectResult->append(res->getString(i));
				SelectResult->append("\"");
			}
			SelectResult->append(i != colCount ? "|" : "\n");
		}
	}

	delete res;
	delete stmt;
	con->close();
	delete con;
	driver->threadEnd();

	auto buffer = StringToCharArray(SelectResult);
	*lenght = SelectResult->length();
	SelectResult->clear();
	return buffer;
}

DataBlock** CFastMysqlSelect::FastBlocksMysqlSelect(const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int blockLenght)
{
	mysql_driver_mutex.lock();
	auto driver = sql::mysql::get_mysql_driver_instance();
	mysql_driver_mutex.unlock();

	auto con = driver->connect(host, user, pass);
	con->setSchema(dbname);

	auto stmt = con->createStatement();

	auto res = stmt->executeQuery(query);
	int colCount = res->getMetaData()->getColumnCount();

	auto resultBuf = new string();
	int rowProcessed = 0;
	int blockIndex = 0;
	int rowPerBlock = blockLenght;

	SelectBlocksResult = static_cast<DataBlock**>(malloc(sizeof(DataBlock*) * (res->rowsCount() / rowPerBlock + 1)));

	while (res->next())
	{
		for (auto i = 1; i <= colCount; i++)
		{
			if (res->isNull(i))
			{
				resultBuf->append("NULL");
			}
			else
			{
				resultBuf->append("\"");
				resultBuf->append(res->getString(i));
				resultBuf->append("\"");
			}
			resultBuf->append(i != colCount ? "|" : "\n");
		}
		rowProcessed++;

		if (rowProcessed >= rowPerBlock)
		{
			DataBlock* data_block = new DataBlock();
			data_block->data = StringToCharArray(resultBuf);
			data_block->length = resultBuf->length();
			SelectBlocksResult[blockIndex++] = data_block;
			resultBuf->clear();
			rowProcessed = 0;
		}
	}

	if (rowProcessed > 0)
	{
		DataBlock* data_block = new DataBlock();
		data_block->data = StringToCharArray(resultBuf);
		data_block->length = resultBuf->length();
		SelectBlocksResult[blockIndex++] = data_block;
	}

	delete resultBuf;

	delete res;
	delete stmt;
	delete con;

	DataBlockCount = blockIndex;
	*lenght = blockIndex;

	return SelectBlocksResult;
}

FASTMYSQLSELECT_API char* FastSelectString(void* handle, const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int* rowCount)
{
	CFastMysqlSelect *selHandle = (CFastMysqlSelect*)handle;
	return selHandle->FastMysqlSelect(host, user, pass, dbname, query, lenght, rowCount);
}

FASTMYSQLSELECT_API DataBlock** FastSelectBloksString(void* handle, const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int blockLenght)
{
	CFastMysqlSelect *selHandle = (CFastMysqlSelect*)handle;
	try {
		return selHandle->FastBlocksMysqlSelect(host, user, pass, dbname, query, lenght, blockLenght);
	}
	catch (exception &e) {
		selHandle->Error->append("# ERR: SQLException in ");
		selHandle->Error->append(__FILE__);
		selHandle->Error->append("\n");
		selHandle->Error->append("(");
		selHandle->Error->append(__FUNCTION__);
		selHandle->Error->append(__FILE__);
		selHandle->Error->append(")");
		selHandle->Error->append("\n");
		selHandle->Error->append("# ERR: ");
		selHandle->Error->append(e.what());
		selHandle->Error->append("\n");
		cout << "# ERR: SQLException in " << __FILE__;
		cout << "(" << __FUNCTION__ << ") on line " << __LINE__ << endl;
		/* what() (derived from std::runtime_error) fetches error message */
		cout << "# ERR: " << e.what();
		return nullptr;
	}
}

FASTMYSQLSELECT_API void* INIT()
{
	return new CFastMysqlSelect();
}

char* GetErrorMessage(void* handle, int* message_lenght)
{
	CFastMysqlSelect *selHandle = (CFastMysqlSelect*)handle;
	auto buffer = const_cast<char*>(selHandle->Error->c_str());
	*message_lenght = selHandle->Error->length();
	return buffer;
}

FASTMYSQLSELECT_API void DESTROY(void *handle)
{
	CFastMysqlSelect *selHandle = (CFastMysqlSelect*)handle;
	delete selHandle->SelectResult;
	if (selHandle->SelectBlocksResult != nullptr) 
	{
		for (int i = 0; i < selHandle->DataBlockCount; i++)
		{
			delete selHandle->SelectBlocksResult[i]->data;
			delete selHandle->SelectBlocksResult[i];
		}
		delete selHandle->SelectBlocksResult;
	}
	delete selHandle;
}
