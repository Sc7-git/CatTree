using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace CatTree
{

    public interface ITree<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new() { }

    public interface ITree<TEntity> : ITree<TEntity, string> where TEntity : ITree<TEntity, string>, new() { }

    public abstract class Tree<TEntity, TIdType> : ITree<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new() { }

    public abstract class Tree<TEntity> : ITree<TEntity, string> where TEntity : ITree<TEntity, string>, new() { }
}
