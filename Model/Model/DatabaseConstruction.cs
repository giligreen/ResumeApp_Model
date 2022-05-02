using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using Aspose.Words;
using ClosedXML.Excel;
using System.Text.RegularExpressions;
using System.Data;



namespace Model
{
    static class DatabaseConstruction
    {

        /// <summary>
        /// פונקציה שעוברת על תקיית המאגר ומחזירה מערך של נתיבים לכל הקבצים בתקיה
        /// </summary>
        /// <param name="folderPath">נתיב לתקיה</param>
        /// <returns>מערך של נתיבי הקבצים</returns>
       public static string[] ScanFolder(string folderPath)
        {
            string[] files;
            files = Directory.GetFiles(folderPath, "*.pdf", SearchOption.AllDirectories).ToArray()
                .Union(Directory.GetFiles(folderPath, "*.docx", SearchOption.AllDirectories)).ToArray()
                .Union(Directory.GetFiles(folderPath, "*.doc", SearchOption.AllDirectories)).ToArray();
            return files;
        }


        /// <summary>
        /// פונקציה שמקבלת נתיב לקובץ אקסל שמכיל את נתיבי הקבצים בתקיה וסיווגם  
        ///  היא עוברת על השורות ובונה אובייקטים מסוג קורות חיים שיכילו
        ///  את הנתיב לקובץ ואת הסיווג שלהם ע'פ הנתונים מקובץ האקסל
        /// </summary>
        /// <param name="pathesDataBase">נתיב לקובץ האקסל</param>
        /// <returns> resume מערך של אובייקטים מסוג  </returns>
        public static Resume[] BuildResumesObjects(string pathesDataBase)
        {

            var wbook = new XLWorkbook(pathesDataBase);
            var ws1 = wbook.Worksheet(1);
            Resume[] resumes = new Resume[ws1.RowsUsed().Count()];

            for (int i = 1; i <= ws1.RowsUsed().Count(); i++)
            {
                string c = ws1.Cell(i, 2).GetValue<string>();
                string path = ws1.Cell(i, 3).GetValue<string>();
                resumes[i - 1] = new Resume(path, c);
            }

            return resumes;
        }



        /// <summary>
        /// פונקציה שמקבלת נתיב לקובץ וורד או פידיאף - קובץ בינארי, וממירה אותו לקובץ טקסט
        /// </summary>
        /// <param name="File_path">נתיב הקובץ להמרה</param>
        /// <returns>נתיב לקובץ הטקסט החדש</returns>
        public static string ConvertFileToTextFile(String FilePath)
        {

            var doc = new Document("my-files/Resume database/" + FilePath);
            string newPath = "my-files/text files/" + Path.GetFileNameWithoutExtension(FilePath) + ".txt";
            doc.Save(newPath);
            return newPath;
        }



        /// <summary>
        ///  string פונקציה שמקבלת נתיב של קובץ טקסט וקוראת את כל תוכנו לתוך משתנה
        /// </summary>
        /// <param name="filePath">נתיב לקובץ</param>
        /// <returns></returns>
        public static string ReadAllTextFromFile(string filePath)
        {

            string s = File.ReadAllText(filePath);
            return s;
        }



        /// <summary>
        /// פונקציה שמקבלת נתיב לקובץ טקסט 
        /// ומחזירה מערך של מילים 
        /// </summary>
        /// <param name="filePass"></param>
        public static List<string> DividingFileIntoWords(string filePath)
        {
            string text = ReadAllTextFromFile(filePath);

            string cleantext = Regex.Replace(text, "[^A-Za-z א-ת]", " ").TrimStart().TrimEnd();
            List<string> words = new List<string>();
            char[] charsToSplit = new char[2];
            charsToSplit[0] = ' ';
            charsToSplit[1] = (char)10;
            words = cleantext.Split(charsToSplit).ToList<string>();


            words.RemoveAll((item) => string.IsNullOrEmpty(item) || string.IsNullOrWhiteSpace(item));
            words.RemoveRange(0, 10);
            words.RemoveRange(words.Count() - 22 - 1, 23);
            return words;
        }



        public const int wordsRow = 2;
        public const int SubjectColumn = 2;
        public const int LevelColumn = 1;
        public const int firstWordColumn = 3;

        /// <summary>
        ///   פונקציה שמקבלת מערך של מילים של קו"ח מסויים
        ///  ומכניסה כל מילה לקובץ אקסל(הוספת עמודה שכותרתה היא מילה זו) וסימון 1 בשורה החדשה  
        /// </summary>
        /// <param name="ExcelFilePath">נתיב לקובץ האקסל</param>
        /// <param name="word">מערך המילים של הקו"ח</param>
        /// <param name="row">תחום הקו"ח - הכותרת לשורה - הסיווג של הקו"ח</param>
        public static void FeelExcelWithNewResumeWords(string ExcelFilePath, string[] word, string row)
        {   //עבודה עם closedxml

            var wbook = new XLWorkbook(ExcelFilePath);
            var ws1 = wbook.Worksheet(1);

            var newRow = ws1.LastRowUsed().RowNumber() + 1;//השורה החדשה

            //מילוי כל השורה באפסים
            var numOfColumns = ws1.ColumnsUsed().Count();
            for (int k = firstWordColumn; k <= numOfColumns; k++)
            {
                ws1.Cell(newRow, k).Value = 0;
            }

            int WordColumn;
            bool wordExists = false;
            //מעבר על כל המילים במערך
            for (int i = 0; i < word.Length; i++)
            {
                wordExists = false;
                WordColumn = firstWordColumn;
                //חיפוש האם המילה כבר קיימת
                while (!ws1.Cell(wordsRow, WordColumn).IsEmpty() && !wordExists)
                {
                    //אם המילה כבר קיימת
                    if (ws1.Cell(wordsRow, WordColumn).GetValue<string>() == word[i])
                    {
                        //מילוי כותרת השורה - הסיווג
                        ws1.Cell(newRow, SubjectColumn).Value = row;

                        //מילוי התא המתאים באחד
                        ws1.Cell(newRow, WordColumn).Value = 1;
                        wordExists = true;
                    }
                    else
                    {
                        WordColumn++;
                    }

                }

                //אם המילה לא נמצאה
                if (ws1.Cell(wordsRow, WordColumn).IsEmpty())
                {
                    for (int j = wordsRow + 1; j < newRow; j++)
                    {
                        ws1.Cell(j, WordColumn).Value = 0;
                    }
                    //מילוי כותרת השורה - הסיווג
                    ws1.Cell(newRow, firstWordColumn - 1).Value = row;
                    //מילוי התא המתאים באחד
                    ws1.Cell(newRow, WordColumn).Value = 1;
                    //מילוי כותרת העמודה החדשה - במילה
                    ws1.Cell(wordsRow, WordColumn).Value = word[i];
                }
                wbook.Save();
            }
        }
     
        
        
        
        /// <summary>
       ///DataTable פונקציה שמקבלת נתיב לקובץ ובונה ממנו 
       /// </summary>
       /// <param name="CSVFile">csv נתיב לקובץ</param>
       /// <returns> עם כל הנתונים dataTable</returns>
        public static void FillInDataBase(string DataBaseForClassification_ExcelFile)
        {

            string pathesDataBase = @"my-files\PathesDataBase.xlsx";
            Resume[] resumes =BuildResumesObjects(pathesDataBase);
            foreach (var item in resumes)
            {
                string newPath = ConvertFileToTextFile(item.Path);
               // List<string> words = DividingFileIntoWords(newPath);
               // string[] wordsArray = words.ToArray();
               // FeelExcelWithNewResumeWords(DataBaseForClassification_ExcelFile, wordsArray, item.Class);
            }
            
        }

    }
}
