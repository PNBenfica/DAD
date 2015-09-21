using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackListaNomes
{
    interface IListNames
    {
        void add(string name);

        string list();

        void clear();
    }
}
