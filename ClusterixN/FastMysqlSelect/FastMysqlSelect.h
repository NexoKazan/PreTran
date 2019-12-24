// ����������� ���� ���� ifdef - ��� ����������� ����� �������� ��������, ���������� ��������� 
// �������� �� ��������� DLL. ��� ����� ������ DLL �������������� � �������������� ������� FASTMYSQLSELECT_EXPORTS,
// � ��������� ������. ���� ������ �� ������ ���� ��������� � �����-���� �������
// ������������ ������ DLL. ��������� ����� ����� ������ ������, ��� �������� ����� �������� ������ ����, ����� 
// ������� FASTMYSQLSELECT_API ��� ��������������� �� DLL, ����� ��� ������ DLL ����� �������,
// ������������ ������ ��������, ��� ����������������.
#ifdef FASTMYSQLSELECT_EXPORTS
#define FASTMYSQLSELECT_API __declspec(dllexport)
#else
#define FASTMYSQLSELECT_API __declspec(dllimport)
#endif
#include <stddef.h>
#include <string>

struct DataBlock
{
	char* data;
	int length;
};

// ���� ����� ������������� �� FastMysqlSelect.dll
class FASTMYSQLSELECT_API CFastMysqlSelect {
public:
	CFastMysqlSelect(void);
	char* FastMysqlSelect(const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int* rowCount);
	DataBlock** FastBlocksMysqlSelect(const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int blockLenght);
	std::string *SelectResult;
	DataBlock** SelectBlocksResult;
	int DataBlockCount;
	std::string *Error;
};

extern "C" FASTMYSQLSELECT_API DataBlock** FastSelectBloksString(void *handle, const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int blockLenght);
extern "C" FASTMYSQLSELECT_API char* FastSelectString(void *handle, const char* host, const char* user, const char* pass, const char* dbname, const char* query, int* lenght, int* rowCount);
extern "C" FASTMYSQLSELECT_API void* INIT();
extern "C" FASTMYSQLSELECT_API char* GetErrorMessage(void *handle, int* message_lenght);
extern "C" FASTMYSQLSELECT_API void DESTROY(void *handle);
