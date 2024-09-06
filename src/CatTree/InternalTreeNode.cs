using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatTree
{
    internal class InternalTreeNode<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {
        //public HashSet<TEntity> Nodes { get; set; }
        public TEntity Node { get; set; }
        /// <summary>
        /// 源 子集
        /// </summary>
        public ICollection<TEntity> SourceChildren { get; set; }

    }
}
