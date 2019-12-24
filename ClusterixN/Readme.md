# Clusterix-N

Clusterix-N - исследовательский прототип параллельной консервативной СУБД, ориентированный на ускорение (в сравнении с СУБД Clusterix) обработки запросов к БД повышенных объемов (не умещающихся в оперативной памяти одного узла).

[Документация по Clusterix-N](https://bitbucket.org/rozh/ClusterixN/wiki)

## Базовая СУБД

Используемая СУБД: [MySQL 5.7](https://bitbucket.org/rozh/ClusterixN/downloads/mysql-5.7.9-winx64.exe)

В состав сборки входит:

* Сама СУБД MySQL 5.7.9
* Конфигурация `my.ini`
* Скрипт инициализации структуры хранения данных (*init_data_dir.bat*)
* Скрипты запуска (*start_mysqld.bat*) и остановки (*stop_mysqld.bat*)
* Скрипты установки (*service_install.bat*) и удаления службы (*service_uninstall.bat*)

### Установка

1. Скачать и распаковать СУБД [MySQL 5.7](https://bitbucket.org/rozh/ClusterixN/downloads/mysql-5.7.9-winx64.exe)
2. В директории СУБД запустить *init_data_dir.bat*
3. Запустить *start_mysqld.bat* для запуска сервера или *service_install.bat* для установки службы и запуска сервера. После установки как службы MySQL будет автоматически запускаться при старте системы.

### Подключение к MySQL

Для подключения к MySQL можно использовать любой клиент, например [HeidiSQL](http://www.heidisql.com/).

Параметры подключения:

* Адрес сервера: localhost
* Порт: 3306
* Пользователь: root
* Пароль: без пароля

### БД

Для генерации данных используется генератор из теста [TPC-H](http://www.tpc.org/tpch/default.asp)

#### Генерация данных

```
dbgen -s 5
```

#### Загрузка данных

Перед загрузкой данных в узлы IO необходимо их разбить. Для этого необходимо воспользоваться утилитой DataSplitter, которая принимает в качестве аргументов: количество узлов для разбития и директорию с файлами теста TPC-H.

```
DataSplitter.exe 3 c:\tmp\tpch5g
```

DataSplitter создаст новые загрузочные файлы и сгенерирует скрипты для загрузки.

## Сборка для Linux

В составе системы используется SQLite. Для ОС семейства Windows все необходимые библиотеки будут загружены из NuGET. Для Linux потребуется собрать нативный модуль.

### Сборка нативного модуля SQLite для Linux

1. Скачать исходные коды `sqlite-netFx-full-source` [http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki](http://system.data.sqlite.org/index.html/doc/trunk/www/downloads.wiki)
1. Распаковать пакет
1. Перейти в папку `Setup`
1. Выполнить `compile-interop-assembly-release.sh`
1. Переместить нативный модуль из `bin/2013/Release/bin/` в папку с системой

## Лицензии и авторские права

Copyright © 2017-2018 Классен Роман. Все права защищены [лицензионным соглашением Apache 2.0](LICENSE.txt)