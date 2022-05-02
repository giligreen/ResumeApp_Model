using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    class Resume
    {
        public string Path { get; set; }
        public string Class { get; set; }
        public  IDictionary<string,int> AttributesValuesDict { get; set; }
        public string[] WordArr { get; set; }


        public Resume()
        {

        }

        public Resume(string path, string c)
        {
            Path = path;
            Class = c;
        }


    }
}
