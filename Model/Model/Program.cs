using System;
using System.Data;

namespace Model
{
    class Program
    {
        static void Main(string[] args)
        {
            //string DataBaseForClassification_ExcelFile = @"my-files\‏‏DataBaseForClassification.xlsx";
            //   DatabaseConstruction.FillInDataBase(DataBaseForClassification_ExcelFile);
           
            string DataBaseForClassification_CsvFile = @"my-files\FinialDataBaseForClassification.csv";
            //string DataBaseForClassification_CsvFile = @"my-files\smallDB.csv";
           
            DataTable db = CsvFileHandler.ImportFromCsvFile(DataBaseForClassification_CsvFile);
            TreeNode tree = DecisionTree.Learn(db, " ");
            DecisionTree.Print(tree, "");
            //DecisionTree.Print(tree, tree.Name);
            DecisionTree.SaveTreeIntoFile(tree);
            DecisionTree t = new DecisionTree();
            TreeNode tree2=t.LoadTreeFromFile(@"my-files/trying.xlsx");
        }
    }
}
