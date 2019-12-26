#region Copyright
/*
 * Copyright 2019 Roman Klassen
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

using System.IO;
using System.Xml.Serialization;

namespace PreTran
{
    /// <summary>
    ///     Класс реализует методы сериализации/десериализации XML
    /// </summary>
    public class XmlSerializationBase<T>
    {
        /// <summary>
        ///     Загрузка конфигурации из файла XML
        /// </summary>
        public static T Load(string fileName)
        {
            object result;
            using (var fs = File.Open(fileName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    result = (T) serializer.Deserialize(fs);
                }
                finally
                {
                    fs.Flush();
                }
            }
            return (T) result;
        }

        /// <summary>
        ///     Загрузка конфигурации из файла потока
        /// </summary>
        public static T Load(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(T));
            object result = (T) serializer.Deserialize(stream);
            return (T) result;
        }

        /// <summary>
        ///     Сохранение конфигурации в файл XML
        /// </summary>
        public void Save(string fileName)
        {
            using (var fs = File.Open(fileName, FileMode.Create))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(T));
                    serializer.Serialize(fs, this);
                    fs.Flush();
                }
                finally
                {
                    fs.Flush();
                }
            }
        }

        /// <summary>
        ///     Сохранение конфигурации в поток
        /// </summary>
        public void Save(Stream stream)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(stream, this);
                stream.Flush();
            }
            finally
            {
                stream.Flush();
            }
        }
    }
}