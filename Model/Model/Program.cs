using System;
using System.Collections.Generic;
using System.Data;

namespace Model
{
    class Program
    {
        static void Main(string[] args)
        {
            string DataBaseForClassification_ExcelFile = @"my-files\‏‏DataBaseForClassification.xlsx";
            string DataBaseForClassification_CsvFile = @"my-files\FinialDataBaseForClassification.csv";

            DatabaseConstruction.FillInDataBase(DataBaseForClassification_ExcelFile);
            DataTable db = CsvFileHandler.ImportFromCsvFile(DataBaseForClassification_CsvFile);
            TreeNode tree = DecisionTree.Learn(db, " ");
            DecisionTree.Print(tree, tree.Name);
        }
    }
}
