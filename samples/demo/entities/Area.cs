using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastTree;
using Masuit.Tools.Models;

namespace demo.entities
{
    public class Area : ITree<Area, long>
    {
        public long Code { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public long Pcode { get; set; }

        public int Category { get; set; }

        public Area Parent { get; set; }

        //public ReadOnlyCollection<Area> Children { get; set; }
        //public ImmutableList<Area> Children { get; set; }
        //public ICollection<Area> Children { get; set; }
        //public List<Area> Children { get; set; }
        public IEnumerable<Area> Children { get; set; }
        //public MyList<Area> Children { get; set; }
        //public AreaList Children { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AreaEntity : ITree<AreaEntity, long>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public long ParentId { get; set; }

        public int Category { get; set; }

        public AreaEntity Parent { get; set; }

        public HashSet<AreaEntity> Children { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }

    public class AreaEntity2 : ITree<AreaEntity2, long>, Masuit.Tools.Models.ITreeEntity<AreaEntity2, long>
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public long? ParentId { get; set; }

        public int Category { get; set; }

        public AreaEntity2 Parent { get; set; }

        public ICollection<AreaEntity2> Children { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
