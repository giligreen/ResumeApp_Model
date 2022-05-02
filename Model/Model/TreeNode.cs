using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Model
{
    public class TreeNode
    { 
        //משמש למספור הצמתים
        static int index = 0;

        public int Id { get; set; }
        public string Name { get; set; }

        public string Edge { get; set; }

        public MyAttribute NodeAttribute { get; set; }

        public List<TreeNode> ChildNodes { get; set; }

        public int TableIndex { get; set; }

        public bool IsLeaf { get; set; }

        public TreeNode() { }

        public TreeNode(string name, int tableIndex, MyAttribute nodeAttribute, string edge)
        {
            Id = index++;
            Name = name;
            TableIndex = tableIndex;
            NodeAttribute = nodeAttribute;
            ChildNodes = new List<TreeNode>();
            Edge = edge;
        }

        public TreeNode(bool isleaf, string name, string edge)
        {
            Id = index++;
            IsLeaf = isleaf;
            Name = name;
            Edge = edge;
        }

       
    }
}
