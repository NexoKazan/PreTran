# Претранслятор SQL к регулярному плану для Clusterix-N.
Данный претранслятор разбирает SQL запрос с помощью библиотеки Antlr4 и составляет запросы трёх типов для параллельной СУБД класса BigData Clusterix-N.
Типы запросов:
1. Запросы SELECT-PROJECT, которые отправляются на узлы IO СУБД Clusterix-N.
2. Запросы JOIN, которые отправляются на узлы JOIN СУБД Clusterix-N.
3. Запрос SORT, который отправляется на узел SORT СУБД Clusterix-N.

Запросы формируются таким образом, чтобы их можно было выполнять согласно регулярному плану.
В претрансляторе разработан алгоритм составления последовательности запросов JOIN, который выбирает лучшую из возможных последовательностей JOIN.

Опубликованные статьи:
1. ВКИТ 2020-10 - http://vkit.ru/index.php/current-issue/973-011-020
2. 2021 International Russian Automation Conference (RusAutoCon), IEEE Xplore: 17 September 2021 - https://ieeexplore.ieee.org/document/9537394

История разработки в выступлениях на Республиканском Семинаре "Методы Моделирования" в КНИТУ-КАИ:
1. https://www.youtube.com/watch?v=14cX2HPJT4c
2. https://www.youtube.com/watch?v=QbAzSsGiMRE
3. https://www.youtube.com/watch?v=-mMX-Cxw6BA
4. https://www.youtube.com/watch?v=_Gj_5fVA1v0
5. https://www.youtube.com/watch?v=zGqm11uqi48

[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
