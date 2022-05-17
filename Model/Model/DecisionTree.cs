using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using ClosedXML.Excel;

namespace Model
{
    public class DecisionTree
    {
        public TreeNode Root { get; set; }
        
        

        public static void Print(TreeNode node, string result)
        {
            if (node?.ChildNodes == null || node.ChildNodes.Count == 0)
            {
                var seperatedResult = result.Split(' ');

                //foreach (var item in seperatedResult)
                //{
                //    if (item.Equals(seperatedResult[0]))
                //    {
                //        Console.ForegroundColor = ConsoleColor.Magenta;
                //    }
                //    else if (item.Equals("--") || item.Equals("-->"))
                //    {
                //        // empty if but better than checking at .ToUpper() and .ToLower() if
                //    }
                //    else if (item.Equals("YES") || item.Equals("NO"))
                //    {
                //        Console.ForegroundColor = ConsoleColor.Green;
                //    }
                //    else if (item.ToUpper().Equals(item))
                //    {
                //        Console.ForegroundColor = ConsoleColor.Cyan;
                //    }
                //    else
                //    {
                //        Console.ForegroundColor = ConsoleColor.Yellow;
                //    }

                //    Console.Write($"{item} ");
                //    Console.ResetColor();
                //}

                Console.WriteLine();

                return;
            }

            foreach (var child in node.ChildNodes)
            {
                if (child != null) { 
                Print(child, result + " -- " + child.Edge.ToLower() + " --> " + child.Name.ToUpper());
                                }
            }
        }

        //public static void PrintLegend(string headline)
        //{
        //    Console.ForegroundColor = ConsoleColor.White;
        //    Console.WriteLine($"\n{headline}");
        //    Console.ForegroundColor = ConsoleColor.Magenta;
        //    Console.WriteLine("Magenta color indicates the root node");
        //    Console.ForegroundColor = ConsoleColor.Yellow;
        //    Console.WriteLine("Yellow color indicates an edge");
        //    Console.ForegroundColor = ConsoleColor.Cyan;
        //    Console.WriteLine("Cyan color indicates a node");
        //    Console.ForegroundColor = ConsoleColor.Green;
        //    Console.WriteLine("Green color indicates a decision");
        //    Console.ResetColor();
        //}



        /// <summary>
        /// (predict() פונקציה שמסווגת נתון חדש - קורות חיים חדש, לפי עץ ההחלטה שהיא מקבלת (בפייתון - הפונקציה  
        /// </summary>
        /// <param name="root">עץ ההחלטה</param>
        /// <param name="valuesForQuery"> מילון עם ערכי התכונות של קורות החיים לסיווג - 0 או 1 לכל מילה</param>
        /// <param name="result">הסיווג - מתעדכן במהלך הרקורסיה</param>
        /// <returns></returns>
        public static string CalculateResult(TreeNode root, IDictionary<string, string> valuesForQuery, string result)
        {
            var valueFound = false;

            result += root.Name.ToUpper() + " -- ";

            if (root.IsLeaf)
            {
                result = root.Edge.ToLower() + " --> " + root.Name.ToUpper();
                valueFound = true;
            }
            else
            {
                foreach (var childNode in root.ChildNodes)
                {
                    foreach (var entry in valuesForQuery)
                    {
                        if (childNode.Edge.ToUpper().Equals(entry.Value.ToUpper()) && root.Name.ToUpper().Equals(entry.Key.ToUpper()))
                        {
                            valuesForQuery.Remove(entry.Key);

                            return result + CalculateResult(childNode, valuesForQuery, $"{childNode.Edge.ToLower()} --> ");
                        }
                    }
                }
            }

            if (!valueFound)
            {
                result = "Attribute not found";
            }

            return result;
        }


        /// <summary>
        ///     פונקציה שלומדת את הנתונים מתאמנת ובונה עץ החלטה בצורה רקורסיבית (fit() בפיתון- הפונקציה )      
        /// </summary>
        /// <param name="data">טבלה של הנתונים</param>
        /// <param name="edgeName">ערך התכונה שהובילה לצומת הנוכחית - בפעם הראשונה - אין משמעות לנתון זה</param>
        /// <returns></returns>
        public static TreeNode Learn(DataTable data, string edgeName)
        {
            if (data.Columns.Count == 2)
            {
                Console.WriteLine("בדיקה");
            }
            
                var root = GetRootNode(data, edgeName);
            if (root != null) { 
                foreach (var item in root.NodeAttribute.DifferentAttributeNames)
                {
                    // אם זה עלה, העלה יתווסף בפונקציה זו
                    var isLeaf = CheckIfIsLeaf(root, data, item);

                    // בצע קריאה רקורסיבית כל עוד הצומת אינו עלה
                    if (!isLeaf)
                    {
                        var reducedTable = CreateSmallerTable(data, item, root.TableIndex);

                        root.ChildNodes.Add(Learn(reducedTable, item));
                    }
                }
            }
            return root;
        }


        private static bool CheckIfIsLeaf(TreeNode root, DataTable data, string attributeToCheck)
        {
            var isLeaf = true;
            var allEndValues = new List<string>();

            //  קבל את כל ערכי העלים עבור התכונה המדוברת
            for (var i = 0; i < data.Rows.Count; i++)
            {
                if (data.Rows[i][root.TableIndex].ToString().Equals(attributeToCheck))
                {
                    allEndValues.Add(data.Rows[i][data.Columns.Count - 1].ToString());
                }
            }
            // בדוק אם לכל הרכיבים ברשימה יש אותו ערך
            if (allEndValues.Count > 0 && allEndValues.Any(x => x != allEndValues[0]))
            {
                isLeaf = false;
            }

            //צור עלה עם הערך 
            if (isLeaf)
            {
                root.ChildNodes.Add(new TreeNode(true, allEndValues[0], attributeToCheck));
            }

            return isLeaf;
        }


        /// <summary>
        /// פונקציה שבונה טבלה קטנה יותר עבור הפיצול הבא לעץ 
        /// </summary>
        /// <param name="data">טבלת הנתונים</param>
        /// <param name="edgePointingToNextNode"> לפיצול הבא Attribute שם הקשת - שם מסויים בתוך ה</param>
        /// <param name="rootTableIndex">   Attribute - האינדקס של העמודה בטבלה   - </param>
        /// <returns>edgePointingToNextNodeטבלה קטנה יותר שמכילה רק את השורות בהם הערך בעמודה שהתקבלה שווה לערך ה</returns>
        private static DataTable CreateSmallerTable(DataTable data, string edgePointingToNextNode, int rootTableIndex)
        {
            var smallerData = new DataTable();

            //הוסף כותרות עמודות
            for (var i = 0; i < data.Columns.Count; i++)
            {
                smallerData.Columns.Add(data.Columns[i].ToString());
            }

            // לטבלת נתונים חדשה edgePointingToNextNode  הוסף שורות המכילות  
            for (var i = 0; i < data.Rows.Count; i++)
            {
                if (data.Rows[i][rootTableIndex].ToString().Equals(edgePointingToNextNode))
                {
                    var row = new string[data.Columns.Count];

                    for (var j = 0; j < data.Columns.Count; j++)
                    {
                        row[j] = data.Rows[i][j].ToString();
                    }

                    smallerData.Rows.Add(row);
                }
            }

            // remove column which was already used as node            
            smallerData.Columns.Remove(smallerData.Columns[rootTableIndex]);

            return smallerData;
        }


        /// <summary>
        /// פונקציה שמוצאת את שורש העץ
        /// </summary>
        /// <param name="data">טבלת נתונים- בפעם הראשונה הטבלה כולה, בפעמים הבאות טבלה מוקטנת </param>
        /// <param name="edge">הקשת הקודמת שמובילה לפיצול זה בפעם הראשונה מתקבל ערך ריק</param>
        /// <returns></returns>
        private static TreeNode GetRootNode(DataTable data, string edge)
        { 
            var attributes = new List<MyAttribute>();
            var highestInformationGainIndex = -1;
            var highestInformationGain = double.MinValue;

            //   קבל את כל הערכים השונים עבור כל עמודה - תכונה 
            if (data.Columns.Count <= 1)
            {
                return null;
            }
            else
            {
                for (var i = 0; i < data.Columns.Count - 1; i++)
                {
                    var differentAttributevalues = MyAttribute.GetDifferentAttributeValuesOfColumn(data, i);
                    attributes.Add(new MyAttribute(data.Columns[i].ToString(), differentAttributevalues));
                }

                // חשב אנטרופיה
                var tableEntropy = CalculateTableEntropy(data);

                for (var i = 0; i <= attributes.Count - 1; i++)
                {
                    if (i == 2078)
                    {
                        Console.WriteLine(" ");
                    }
                    attributes[i].InformationGain = GetGainForAttribute(data, i, tableEntropy);

                    if (attributes[i].InformationGain > highestInformationGain)
                    {
                        highestInformationGain = attributes[i].InformationGain;
                        highestInformationGainIndex = i;
                    }
                }
                if (highestInformationGainIndex < 0)
                {
                    Console.WriteLine("aaa");
                }
                return new TreeNode(attributes[highestInformationGainIndex].Name, highestInformationGainIndex, attributes[highestInformationGainIndex], edge);
            }
        }




        /// <summary>
        ///    מסויים Attributeל Gain פונקציה שמחשבת 
        /// </summary>
        /// <param name="data">טבלת נתונים</param>
        /// <param name="colIndex"> Attribute -  מספר העמודה  </param>
        /// <param name="entropyOfDataset">אנטרופיה כללית לטבלה - לפני הפיצול הרצוי</param>
        /// <returns></returns>
        private static double GetGainForAttribute(DataTable data, int colIndex, double entropyOfDataset)
        {
            var totalRows = data.Rows.Count;
             var amountForDifferentValue = GetAmountOfEdgesAndTotalResults(data, colIndex);
            var stepsForCalculation = new List<double>();

            foreach (var item in amountForDifferentValue)
            {
                // עבור כל תחום – חישוב היחס בין כמות הדוגמאות שסווגו 
                //לתחום מתוך כל הדוגמאות שרלוונטיות לפיצול הנוכחי
                var firstDivision = item[0, 1] / (double)item[0, 0];
                var secondDivision = item[0, 2] / (double)item[0, 0];
                var thirdDivision = item[0, 3] / (double)item[0, 0];
                var fourthDivision = item[0, 4] / (double)item[0, 0];
                var fifthDivision = item[0, 5] / (double)item[0, 0];
                var sixthDivision = item[0, 6] / (double)item[0, 0];

                // prevent dividedByZeroException
                stepsForCalculation.Add(
                          (firstDivision == 0 ? 0 : -firstDivision * Math.Log(firstDivision, 2))
                        - (secondDivision == 0 ? 0 : secondDivision * Math.Log(secondDivision, 2))
                        - (thirdDivision == 0 ? 0 : thirdDivision * Math.Log(thirdDivision, 2))
                        - (fourthDivision == 0 ? 0 : fourthDivision * Math.Log(fourthDivision, 2))
                        - (fifthDivision == 0 ? 0 : fifthDivision * Math.Log(fifthDivision, 2))
                        - (sixthDivision == 0 ? 0 : sixthDivision * Math.Log(sixthDivision, 2))
                        );
            }

            var gain = stepsForCalculation.Select((t, i) => amountForDifferentValue[i][0, 0] / (double)totalRows * t).Sum();

            gain = entropyOfDataset - gain;

            return gain;
        }


        /// <summary>
        /// פונקציה שמחשבת את האנטרופיה הכללית לכל הטבלה
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static double CalculateTableEntropy(DataTable data)
        {
            var totalRows = data.Rows.Count;
            var amountForDifferentValue = GetAmountOfEdgesAndTotalResults(data, data.Columns.Count - 1);
            
            var stepsForCalculation = amountForDifferentValue
                .Select(item => item[0, 0] / (double)totalRows)
                .Select(division => division == 0 ? 0 : -division * Math.Log(division, 2))
                .ToList();



            return stepsForCalculation.Sum();
        }


        /// <summary>
        /// פונקציה שמחזירה את כמות הנתונים מכל סוג של התכונה הזו ולכל סוג כמה יש מכל סיווג סופי 
        /// </summary>
        /// <param name="data">טבלת הנתונים</param>
        /// <param name="indexOfColumnToCheck">העמו</param>
        /// <returns>רשימה</returns>
        private static List<int[,]> GetAmountOfEdgesAndTotalResults(DataTable data, int indexOfColumnToCheck)
        {
            var foundValues = new List<int[,]>();
            var knownValues = CountKnownValues(data, indexOfColumnToCheck);

            foreach (var item in knownValues)
            {
                var amount = 0;
                var CompProgramingAmount = 0;//תכנות  
                var OfficeManagementAmount = 0;//מזכירות 
                var EducationAmount = 0;//ההוראה וחינוך 
                var AccountingAmount = 0;//חשבונאות ויעוץ מס 
                var ArchitectureAmount = 0;//אדריכלות
                var GraphicsAndDesignAmount = 0;//גרפיקה ועיצוב  

                for (var i = 0; i < data.Rows.Count; i++)
                {
                    if (data.Rows[i][indexOfColumnToCheck].ToString().Equals(item))
                    {
                        amount++;

                        // Counts the  CompPrograming cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("CompProgramin"))
                        {
                            CompProgramingAmount++;
                        }
                        // Counts the Education cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("Educatio"))
                        {
                            EducationAmount++;
                        }
                        // Counts the OfficeManagement cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("OfficeManagemen"))
                        {
                            OfficeManagementAmount++;
                        }
                        // Counts the Accounting cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("Accountin"))
                        {
                            AccountingAmount++;
                        }
                        // Counts the Architecture cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("Architectur"))
                        {
                            ArchitectureAmount++;
                        }
                        // Counts the GraphicsAndDesign cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("GraphicsAndDesign"))
                        {
                            GraphicsAndDesignAmount++;
                        }

                    }
                }

                int[,] array = { { amount, CompProgramingAmount, EducationAmount, OfficeManagementAmount, AccountingAmount, ArchitectureAmount, GraphicsAndDesignAmount } };
                foundValues.Add(array);
            }

            return foundValues;
        }


        /// <summary>
        ///  העמודה המסויימת- Attributeפונקציה שמחזירה רשימה של הערכים השונים של ה
        /// </summary>
        /// <param name="data">טבלת הנתונים</param>
        /// <param name="indexOfColumnToCheck">העמודה לבדיקה</param>
        private static IEnumerable<string> CountKnownValues(DataTable data, int indexOfColumnToCheck)
        {
            var knownValues = new List<string>();

            // add the value of the first row to the list
            if (data.Rows.Count > 0)
            {
                knownValues.Add(data.Rows[0][indexOfColumnToCheck].ToString());
            }

            for (var j = 1; j < data.Rows.Count; j++)
            {
                var newValue = knownValues.All(item => !data.Rows[j][indexOfColumnToCheck].ToString().Equals(item));

                if (newValue)
                {
                    knownValues.Add(data.Rows[j][indexOfColumnToCheck].ToString());
                }
            }

            return knownValues;
        }



        static int staticRow;
        public static void SaveTreeIntoFile(TreeNode tree)
        { 
            var path = @"my-files/trying.xlsx";
            var wbook = new XLWorkbook(path);
            var ws1 = wbook.Worksheet(1);
            staticRow = 1;
            RecToSaveTreeIntoFile(tree, ws1, -1);
            wbook.Save();
        }

        public static void RecToSaveTreeIntoFile(TreeNode tree, IXLWorksheet ws, int parentId)
        {
            if (tree!=null)
            {
            staticRow++;
            ws.Cell(staticRow, 1).SetValue<int>(tree.Id);
            ws.Cell(staticRow, 2).SetValue<string>(tree.Name);
            ws.Cell(staticRow, 3).SetValue<string>(tree.Edge);
            ws.Cell(staticRow, 4).SetValue<string>(tree.IsLeaf.ToString());
            ws.Cell(staticRow, 5).SetValue<int>(tree.TableIndex);
            ws.Cell(staticRow, 6).SetValue<int>(parentId);
            if (!tree.IsLeaf)
            {
                RecToSaveTreeIntoFile(tree.ChildNodes[0], ws, tree.Id);
                if(tree.ChildNodes.Count>1)
                    RecToSaveTreeIntoFile(tree.ChildNodes[1], ws, tree.Id);
            }
            }
        }

     
        public  TreeNode LoadTreeFromFile(string path)
        {
            var wbook = new XLWorkbook(path);
            var ws = wbook.Worksheet(1);
            staticRow = 1;
            TreeNode root= RecToLoadTreeFromFile(ws);
            return root;
        }

        public  TreeNode RecToLoadTreeFromFile(IXLWorksheet ws)
        {
            staticRow++;
            if (staticRow <=ws.RowsUsed().Count())
            {
            TreeNode newNode = new TreeNode();
            newNode.Id =Convert.ToInt32( ws.Cell(staticRow, 1).Value);
            newNode.Name = ws.Cell(staticRow, 2).Value.ToString();
            newNode.Edge = ws.Cell(staticRow, 3).Value.ToString();
            newNode.IsLeaf =Convert.ToBoolean( ws.Cell(staticRow, 4).Value);
            newNode.TableIndex = Convert.ToInt32(ws.Cell(staticRow, 5).Value);
            if (!newNode.IsLeaf) {
                newNode.ChildNodes = new List<TreeNode>();
               newNode.ChildNodes.Add(RecToLoadTreeFromFile(ws));
               newNode.ChildNodes.Add(RecToLoadTreeFromFile(ws));
            }
           
            return newNode;
            }
            return null;
        }

    } 
}
