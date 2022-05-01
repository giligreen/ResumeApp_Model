using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

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

                foreach (var item in seperatedResult)
                {
                    if (item.Equals(seperatedResult[0]))
                    {
                        Console.ForegroundColor = ConsoleColor.Magenta;
                    }
                    else if (item.Equals("--") || item.Equals("-->"))
                    {
                        // empty if but better than checking at .ToUpper() and .ToLower() if
                    }
                    else if (item.Equals("YES") || item.Equals("NO"))
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                    }
                    else if (item.ToUpper().Equals(item))
                    {
                        Console.ForegroundColor = ConsoleColor.Cyan;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    Console.Write($"{item} ");
                    Console.ResetColor();
                }

                Console.WriteLine();

                return;
            }

            foreach (var child in node.ChildNodes)
            {
                Print(child, result + " -- " + child.Edge.ToLower() + " --> " + child.Name.ToUpper());
            }
        }

        public static void PrintLegend(string headline)
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"\n{headline}");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Magenta color indicates the root node");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Yellow color indicates an edge");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Cyan color indicates a node");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Green color indicates a decision");
            Console.ResetColor();
        }



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
            var root = GetRootNode(data, edgeName);

            foreach (var item in root.NodeAttribute.DifferentAttributeNames)
            {
                // if a leaf, leaf will be added in this method
                var isLeaf = CheckIfIsLeaf(root, data, item);

                // make a recursive call as long as the node is not a leaf
                if (!isLeaf)
                {
                    var reducedTable = CreateSmallerTable(data, item, root.TableIndex);

                    root.ChildNodes.Add(Learn(reducedTable, item));
                }
            }

            return root;
        }


        private static bool CheckIfIsLeaf(TreeNode root, DataTable data, string attributeToCheck)
        {
            var isLeaf = true;
            var allEndValues = new List<string>();

            // get all leaf values for the attribute in question
            for (var i = 0; i < data.Rows.Count; i++)
            {
                if (data.Rows[i][root.TableIndex].ToString().Equals(attributeToCheck))
                {
                    allEndValues.Add(data.Rows[i][data.Columns.Count - 1].ToString());
                }
            }

            // check whether all elements of the list have the same value
            if (allEndValues.Count > 0 && allEndValues.Any(x => x != allEndValues[0]))
            {
                isLeaf = false;
            }

            // create leaf with value to display and edge to the leaf
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

            // add column titles
            for (var i = 0; i < data.Columns.Count; i++)
            {
                smallerData.Columns.Add(data.Columns[i].ToString());
            }

            // add rows which contain edgePointingToNextNode to new datatable
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
        /// פונקציה שמוצאת את שורוש העץ
        /// </summary>
        /// <param name="data">טבלת נתונים- בפעם הראשונה הטבלה כולה, בפעמים הבאות טבלה מוקטנת </param>
        /// <param name="edge">הקשת הקודמת שמובילה לפיצול זה בפעם הראשונה מתקבל ערך ריק</param>
        /// <returns></returns>
        private static TreeNode GetRootNode(DataTable data, string edge)
        {
            var attributes = new List<MyAttribute>();
            var highestInformationGainIndex = -1;
            var highestInformationGain = double.MinValue;

            // Get all names, amount of attributes and attributes for every column             
            for (var i = 0; i < data.Columns.Count - 1; i++)
            {
                var differentAttributenames = MyAttribute.GetDifferentAttributeNamesOfColumn(data, i);
                attributes.Add(new MyAttribute(data.Columns[i].ToString(), differentAttributenames));
            }

            // Calculate Entropy (S)
            var tableEntropy = CalculateTableEntropy(data);

            for (var i = 0; i < attributes.Count; i++)
            {
                attributes[i].InformationGain = GetGainForAllAttributes(data, i, tableEntropy);

                if (attributes[i].InformationGain > highestInformationGain)
                {
                    highestInformationGain = attributes[i].InformationGain;
                    highestInformationGainIndex = i;
                }
            }

            return new TreeNode(attributes[highestInformationGainIndex].Name, highestInformationGainIndex, attributes[highestInformationGainIndex], edge);
        }



        /// <summary>
        ///    מסויים Attributeל Gain פונקציה שמחשבת 
        /// </summary>
        /// <param name="data">טבלת נתונים</param>
        /// <param name="colIndex"> Attribute -  מספר העמודה  </param>
        /// <param name="entropyOfDataset">אנטרופיה כללית לטבלה - לפני הפיצול הרצוי</param>
        /// <returns></returns>
        private static double GetGainForAllAttributes(DataTable data, int colIndex, double entropyOfDataset)
        {
            var totalRows = data.Rows.Count;
            var amountForDifferentValue = GetAmountOfEdgesAndTotalResults(data, colIndex);
            var stepsForCalculation = new List<double>();

            foreach (var item in amountForDifferentValue)
            {
                // helper for calculation
                var firstDivision = item[0, 1] / (double)item[0, 0];
                var secondDivision = item[0, 2] / (double)item[0, 0];
                var thirdDivision = item[0, 3] / (double)item[0, 0];
                var fourthDivision = item[0, 4] / (double)item[0, 0];
                var fifthDivision = item[0, 5] / (double)item[0, 0];
                var sixthDivision = item[0, 6] / (double)item[0, 0];

                // prevent dividedByZeroException
                if (firstDivision == 0 || secondDivision == 0 || thirdDivision == 0 || fourthDivision == 0 || fifthDivision == 0 || sixthDivision == 0)
                {
                    stepsForCalculation.Add(0.0);
                }
                else
                {
                    stepsForCalculation.Add(-firstDivision * Math.Log(firstDivision, 2) - secondDivision * Math.Log(secondDivision, 2) - thirdDivision * Math.Log(thirdDivision, 2) - fourthDivision * Math.Log(fourthDivision, 2) - fifthDivision * Math.Log(fifthDivision, 2) - sixthDivision * Math.Log(sixthDivision, 2));
                }
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
                .Select(division => -division * Math.Log(division, 2))
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
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("CompPrograming"))
                        {
                            CompProgramingAmount++;
                        }
                        // Counts the Education cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("Education"))
                        {
                            EducationAmount++;
                        }
                        // Counts the OfficeManagement cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("OfficeManagement"))
                        {
                            OfficeManagementAmount++;
                        }
                        // Counts the Accounting cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("Accounting"))
                        {
                            AccountingAmount++;
                        }
                        // Counts the Architecture cases and adds the sum later to the array for the calculation
                        if (data.Rows[i][data.Columns.Count - 1].ToString().Equals("Architecture"))
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
        ///  העמודה המסויימת- Attributeפונקציה שמחזירה רשימה של הסוגים השונים של ה
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
    }
}
