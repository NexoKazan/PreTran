#region Copyright
/*
 * Copyright 2017 Roman Klassen
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy
 * of the License at
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 */
#endregion

﻿using System;
using System.Collections.Generic;
using ClusterixN.Common.Data.EventArgs;

namespace ClusterixN.Common.Interfaces
{
    /// <summary>
    /// Интерфейс для инструментальной СУБД
    /// </summary>
    public interface IDatabase : IDisposable
    {
        /// <summary>
        ///     Строка подключения
        /// </summary>
        string ConnectionString { get; set; }
        
        /// <summary>
        /// Загрузка файла в БД
        /// </summary>
        /// <param name="filePath">Путь к файлу на диске</param>
        /// <param name="tableName">Имя таблицы для загрузки</param>
        void LoadFile(string filePath, string tableName);

        /// <summary>
        /// Отключение индексации
        /// </summary>
        /// <param name="tableName">Имя таблицы для загрузки</param>
        void DissableKeys(string tableName);

        /// <summary>
        /// Включение индексации и индексирование
        /// </summary>
        /// <param name="tableName">Имя таблицы для загрузки</param>
        void EnableKeys(string tableName);

        /// <summary>
        /// Удаление таблицы
        /// </summary>
        /// <param name="tableName">Имя таблицы</param>
        void DropTable(string tableName);
        
        /// <summary>
        /// Удаление временных отношений
        /// </summary>
        void DropTmpRealtions();

        /// <summary>
        /// Создает отношение по указанной схеме
        /// </summary>
        /// <param name="name">имя отношения</param>
        /// <param name="fields">поля отношения</param>
        /// <param name="types">описание полей отношения</param>
        void CreateRelation(string name, string[] fields, string[] types);

        /// <summary>
        /// Создает индекс для указанного поля
        /// </summary>
        /// <param name="name">название индекса</param>
        /// <param name="relation">название отношения</param>
        /// <param name="fields">поля отношения по которым будет создан индекс</param>
        void AddIndex(string name, string relation, string[] fields);

        /// <summary>
        /// Создает первичный индекс для указанного поля
        /// </summary>
        /// <param name="relation">название отношения</param>
        /// <param name="fields">поля отношения по которым будет создан индекс</param>
        void AddPrimaryKey(string relation, string[] fields);

        /// <summary>
        /// Исполнение запроса и запись результата в указанное отношение
        /// </summary>
        /// <param name="relationName">имя отношения</param>
        /// <param name="query">запрос</param>
        void QueryIntoRelation(string relationName, string query);
        
        /// <summary>
        /// Получение данных из БД по блокам
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <param name="blockSize">Размер блока</param>
        void SelectBlocks(string query, int blockSize);

        /// <summary>
        /// Получение данных из БД по блокам
        /// </summary>
        /// <param name="query">Запрос</param>
        /// <param name="blockSize">Размер блока</param>
        List<byte[]> Select(string query, int blockSize);

        /// <summary>
        /// Останавливает или продолжает выборку из БД
        /// </summary>
        void ControlSelectBlocks(bool pause);

        /// <summary>
        /// Прочитан очередной блок
        /// </summary>
        event EventHandler<SelectResultEventArg> BlockReaded;

        void StopSelectQuery();
    }
}