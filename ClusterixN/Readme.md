# Clusterix-N

Clusterix-N - ����������������� �������� ������������ �������������� ����, ��������������� �� ��������� (� ��������� � ���� Clusterix) ��������� �������� � �� ���������� ������� (�� ����������� � ����������� ������ ������ ����).

[������������ �� Clusterix-N](https://bitbucket.org/rozh/ClusterixN/wiki)

## ������� ����

������������ ����: [MySQL 5.7](https://bitbucket.org/rozh/ClusterixN/downloads/mysql-5.7.9-winx64.exe)

� ������ ������ ������:

* ���� ���� MySQL 5.7.9
* ������������ `my.ini`
* ������ ������������� ��������� �������� ������ (*init_data_dir.bat*)
* ������� ������� (*start_mysqld.bat*) � ��������� (*stop_mysqld.bat*)
* ������� ��������� (*service_install.bat*) � �������� ������ (*service_uninstall.bat*)

### ���������

1. ������� � ����������� ���� [MySQL 5.7](https://bitbucket.org/rozh/ClusterixN/downloads/mysql-5.7.9-winx64.exe)
2. � ���������� ���� ��������� *init_data_dir.bat*
3. ��������� *start_mysqld.bat* ��� ������� ������� ��� *service_install.bat* ��� ��������� ������ � ������� �������. ����� ��������� ��� ������ MySQL ����� ������������� ����������� ��� ������ �������.

### ����������� � MySQL

��� ����������� � MySQL ����� ������������ ����� ������, �������� [HeidiSQL](http://www.heidisql.com/).

��������� �����������:

* ����� �������: localhost
* ����: 3306
* ������������: root
* ������: ��� ������

### ��

��� ��������� ������ ������������ ��������� �� ����� [TPC-H](http://www.tpc.org/tpch/default.asp)

#### ��������� ������

```
dbgen -s 5
```

#### �������� ������

����� ��������� ������ � ���� IO ���������� �� �������. ��� ����� ���������� ��������������� �������� DataSplitter, ������� ��������� � �������� ����������: ���������� ����� ��� �������� � ���������� � ������� ����� TPC-H.

```
DataSplitter.exe 3 c:\tmp\tpch5g
```

DataSplitter ������� ����� ����������� ����� � ����������� ������� ��� ��������.

## ������ ��� Linux

� ������� ������� ������������ SQLite. ��� �� ��������� Windows ��� ����������� ���������� ����� ��������� �� NuGET. ��� Linux ����������� ������� �������� ������.

### ������ ��������� ������ SQLite ��� Linux

1. ������� �������� ���� `sqlite-netFx-full-source` [http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki](http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki)
1. ����������� �����
1. ������� � ����� `Setup`
1. ��������� `compile-interop-assembly-release.sh`
1. ����������� �������� ������ �� `bin/2013/Release/bin/` � ����� � ��������

## �������� � ��������� �����

Copyright � 2017-2018 ������� �����. ��� ����� �������� [������������ ����������� Apache 2.0](LICENSE.txt)