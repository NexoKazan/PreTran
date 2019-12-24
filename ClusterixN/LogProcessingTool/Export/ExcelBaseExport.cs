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
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace LogProcessingTool.Export
{
    public abstract class ExcelBaseExport
    {
        #region Private fields
        
        protected string ExportFilename;

        protected ExcelPackage ExcelPackage;

        protected ExcelWorkbook Workbook;

        protected ExcelWorksheet CurrentWorksheet;

        protected ExcelRange CurrentCell;

        protected int CurrentRow = 1;

        protected int CurrentColumn = 1;

        protected readonly byte[] Template;


        #endregion

        #region Help methods

        protected ExcelBaseExport(string defaultFirstWorksheetName = "Export", byte[] template = null)
        {
            ExportFilename = new ExportUtils().ExportTempName(GetType().Name, "xlsx");
            if (template!=null)
            {
                File.WriteAllBytes(ExportFilename, template);
                ExcelPackage = new ExcelPackage(new FileInfo(ExportFilename));
            }
            else
            {
                ExcelPackage = new ExcelPackage();
            }
            Template = template;
            Workbook = ExcelPackage.Workbook;
            CurrentWorksheet = template != null ? Workbook.Worksheets.First() : Workbook.Worksheets.Add(defaultFirstWorksheetName);

            CurrentCell = CurrentWorksheet.Cells[CurrentRow, CurrentColumn];
        }

        protected void AddWorksheet(string worksheetName)
        {
            CurrentWorksheet = Workbook.Worksheets.Add(worksheetName);

            CurrentRow = 1;
            CurrentColumn = 1;
            CurrentCell = CurrentWorksheet.Cells[CurrentRow, CurrentColumn];
        }

        protected void CopyWorksheet(string worksheetName)
        {
            CurrentWorksheet = Workbook.Worksheets.Add(worksheetName, CurrentWorksheet);

            CurrentRow = 1;
            CurrentColumn = 1;
            CurrentCell = CurrentWorksheet.Cells[CurrentRow, CurrentColumn];
        }

        protected void DeleteFirstWorkSheet()
        {
            Workbook.Worksheets.Delete(1);
        }

        protected string SaveAndGetFileName()
        {
            if (Template != null)
            {
                ExcelPackage.File.MoveTo(ExportFilename);
                ExcelPackage.Save();
                return ExportFilename;
            }
            var excelFileInfo = new FileInfo(ExportFilename);
            ExcelPackage.SaveAs(excelFileInfo);
            return ExportFilename;
        }

        protected void WriteCell(int row, int column, object cellValue)
        {
            CurrentWorksheet.Cells[row, column].Value = cellValue;
        }

        protected static void WriteCell(ExcelRange cell, object cellValue, string styleName = "")
        {
            cell.Value = cellValue;

            if (styleName != "")
            {
                cell.StyleName = styleName;
            }
        }

        protected void WriteCell(object cellValue, string styleName = "", int width = 0)
        {
            WriteCell(CurrentCell, cellValue, styleName);

            if (width != 0)
            {
                CurrentCell.AutoFitColumns(width, width);
            }
            MoveToNextColumn();
        }

        protected void WriteCellNotNull(decimal value, string styleName = "")
        {
            if (value != 0)
            {
                WriteCell(value, styleName);
            }
            else
            {
                WriteCell(String.Empty, styleName);
            }
        }

        protected void WriteVerticalMergedCell(object cellValue, int countMergedCells, string styleName = "", int width = 0)
        {
            WriteCell(cellValue, styleName, width);

            if (countMergedCells == 0)
            {
                return;
            }

            for (int i = 0; i < countMergedCells; i++)
            {
                MoveToNextColumn(-1);
                MoveToNextRowAndStayColumn();
                WriteCell(string.Empty, styleName);
            }
            MoveToNextRowAndStayColumn(-countMergedCells);

            CurrentWorksheet.Cells[CurrentRow, CurrentColumn - 1, CurrentRow + countMergedCells, CurrentColumn - 1].Merge = true;
        }

        protected void WriteHorizontalMergedCell(object cellValue, int countMergedCells, string styleName = "", int width = 0)
        {
            WriteCell(cellValue, styleName, width);

            if (countMergedCells == 0)
            {
                return;
            }

            for (int i = 0; i < countMergedCells; i++)
            {
                WriteCell(string.Empty, styleName);
            }

            CurrentWorksheet.Cells[CurrentRow, CurrentColumn - (countMergedCells + 1), CurrentRow, CurrentColumn - 1].Merge = true;
        }

        protected void MoveToNextColumn()
        {
            CurrentColumn++;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected void MoveToNextColumn(int step)
        {
            CurrentColumn += step;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected void MoveHorizontal(int step)
        {
            CurrentColumn += step;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected void MoveVertical(int step)
        {
            CurrentRow += step;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected void MoveToNextRow()
        {
            CurrentColumn = 1;
            MoveToNextRowAndStayColumn();
        }

        protected void MoveToNextRow(int step)
        {
            CurrentColumn = 1;
            MoveToNextRowAndStayColumn(step);
        }

        protected void MoveToNextRowAndStayColumn()
        {
            CurrentRow++;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected void MoveToNextRowAndStayColumn(int step)
        {
            CurrentRow += step;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected void MoveToFirstColumnAndStayRow()
        {
            CurrentColumn = 1;
            CurrentCell = CurrentCell[CurrentRow, CurrentColumn];
        }

        protected virtual string GetFontName()
        {
            return "Calibri";
        }

        #endregion
    }
}
