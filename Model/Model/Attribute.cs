using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Model
{
    public class MyAttribute
    {
        /// שם התכונה
        public string Name { get; }
       
        /// רשימת הערכים האפשריים לתכונה - 0 או 1
        public List<string> DifferentAttributeNames { get; }

        ///לתכונה Gain ערך 
        public double InformationGain { get; set; }


        public MyAttribute(string name, List<string> differentAttributenames)
        {
            Name = name;
            DifferentAttributeNames = differentAttributenames;
        }

        public static List<string> GetDifferentAttributeValuesOfColumn(DataTable data, int columnIndex)
        {
            var differentAttributes = new List<string>();

            for (var i = 0; i < data.Rows.Count; i++)
            {
                var found = differentAttributes.Any(t => t.ToUpper().Equals(data.Rows[i][columnIndex].ToString().ToUpper()));

                if (!found)
                {
                    differentAttributes.Add(data.Rows[i][columnIndex].ToString());
                }
            }

            return differentAttributes;
        }
    }
}
