using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace demo.entities
{
    public class MyList<T> : List<T>
    {
        public MyList()
        {

        }
    }

    public class AreaList : List<Area>
    {
    }
}
