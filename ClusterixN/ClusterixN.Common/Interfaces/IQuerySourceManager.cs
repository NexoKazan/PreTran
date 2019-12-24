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

using ClusterixN.Common.Data.Query;

namespace ClusterixN.Common.Interfaces
{
    public interface IQuerySourceManager
    {
        /// <summary>
        ///     �������� ����� ������ �� ������ �� 1 �� 14
        /// </summary>
        /// <param name="number">����� �������</param>
        /// <returns>������ �� ����������</returns>
        Query GetQueryByNumber(int number);

        /// <summary>
        /// ���������� ��������� ���������� ������� � ���� csv
        /// </summary>
        /// <param name="queryNumber">���������� ����� �������</param>
        /// <param name="result">��������� �������</param>
        void WriteResult(int queryNumber, string result);

        /// <summary>
        /// ���������� ���������� �����������
        /// </summary>
        string DirName { get; }
    }
}