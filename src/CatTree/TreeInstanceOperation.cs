using System;
using System.Collections.Generic;
using System.Linq;

namespace CatTree
{
    public class TreeInstanceOperation<TEntity, TIdType>
    {
        public Func<TEntity, TIdType> GetId { get; internal set; }
        internal Action<TEntity, TIdType> SetId { get; set; }

        public Func<TEntity, TIdType> GetParentId { get; internal set; }

        public Func<TEntity, IEnumerable<TEntity>> GetChildren { get; internal set; }

        public Func<TEntity, TEntity> GetParent { get; internal set; }

        internal Action<TEntity, ICollection<TEntity>> SetChildren { get; set; }

        internal Action<TEntity, TEntity> SetParent { get; set; }

        internal Func<ICollection<TEntity>> CreateChildren { get; set; }

    }
}
