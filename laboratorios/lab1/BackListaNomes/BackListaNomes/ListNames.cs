using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;

namespace BackListaNomes
{
    public class ListNames : IListNames{

        private ArrayList names = new ArrayList();

        public ListNames(){
        }

        public void add(string name){
            names.Add(name);
        }

        public string list(){
            string response = "";
            foreach(string aux in names)
                response += aux + "\r\n";
            return response;
        }

        public void clear(){
            names.Clear();
        }
    }
}
