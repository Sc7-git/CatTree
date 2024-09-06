using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FastTree;

namespace demo.entities
{   
    public class Tree : Tree<Tree, string>
    {

        public string Name { get; set; }

        public string Key { get; set; }

        public int Level { get; set; }

        /// <summary>
        /// 排序使用的随机数
        /// </summary>
        public long Order { get; set; }

        public string Id { get; set; }
        public string ParentId { get; set; }

        public ICollection<Tree> Children { get; set; }

        //public HashSet<Tree> Children { get; set; }

        public Tree Parent { get; set; }

        public override string ToString()
        {
            return $"{Key}:{Name}";
        }
    }

}
