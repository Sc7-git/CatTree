using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CatTree
{
    public class TreeResult <TEntity, TIdType> : TreeResultBase<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {
        internal TreeResult(TEntity root, IDictionary<TIdType, InternalTreeNode<TEntity, TIdType>> treeDict, TreeOptions<TEntity, TIdType> options) : base(options)
        {
            TreeDict = treeDict;
            Root = root;
            //Options = options;
        }

        /// <summary>
        /// 根节点
        /// </summary>
        public TEntity Root { get; }

        //internal IDictionary<TIdType, TEntity> TreeDict { get; init; }
        internal IDictionary<TIdType, InternalTreeNode<TEntity, TIdType>> TreeDict { get; }

        /// <summary>
        /// 找到原始节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public TEntity FindRaw(TIdType id)
        {
            if (id == null)
                throw new TreeException(TreeExceptionEnums.None, "id cannot be null");
            if (TreeDict.TryGetValue(id, out InternalTreeNode<TEntity, TIdType> result))
                return result.Node;
            return default;
        }

        /// <summary>
        /// 找到节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TreeResultNode<TEntity, TIdType> Find(TIdType id)
        {
            var result = FindRaw(id);
            if (result != null)
                return new TreeResultNode<TEntity, TIdType>(result, Options);
            return null;
        }

        /// <summary>
        /// 获取所有后代
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public IEnumerable<TEntity> GetAllDescendants(TIdType parentId)
        {
            if (parentId == null)
                throw new TreeException(TreeExceptionEnums.None, "id cannot be null");
            var parent = FindRaw(parentId);
            return GetAllDescendants(parent);
        }

        /// <summary>
        /// 获取所有后代
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public IEnumerable<TEntity> GetAllDescendants(TEntity parent = default)
        {
            if (parent == null)
                //throw new TreeException(TreeExceptionEnums.None, "parent cannot be null");
                parent = Root;
            return GetAllDescendantsCore(parent);
        }

        /// <summary>
        /// 获取所有祖先(从小到大顺序排列)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public IEnumerable<TEntity> GetAllAncestors(TEntity node)
        {
            //if (node == null)
            //    throw new TreeException(TreeExceptionEnums.None, "node cannot be null");

            return GetAllAncestorsCore(node);
        }

        /// <summary>
        /// 获取所有祖先(从小到大顺序排列)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public IEnumerable<TEntity> GetAllAncestors(TIdType id)
        {
            if (id == null)
                throw new TreeException(TreeExceptionEnums.None, "id cannot be null");
            var node = FindRaw(id);
            return GetAllAncestors(node);
        }
    }

    public class TreeResultNode<TEntity, TIdType> : TreeResultBase<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {
        public TreeResultNode(TEntity node, TreeOptions<TEntity, TIdType> options) : base(options)
        {
            Node = node;
        }

        /// <summary>
        /// 当前节点
        /// </summary>
        public TEntity Node { get; }

        /// <summary>
        /// 获取所有后代
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public IEnumerable<TEntity> GetAllDescendants()
        {
            return GetAllDescendantsCore(Node);
        }


        /// <summary>
        /// 获取所有祖先(从小到大顺序排列)
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public IEnumerable<TEntity> GetAllAncestors()
        {
            return GetAllAncestorsCore(Node);
        }

    }

    public class TreeResultBase<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {
        public TreeResultBase(TreeOptions<TEntity, TIdType> options)
        {
            Options = options;
        }

        internal TreeOptions<TEntity, TIdType> Options { get; }

        protected IEnumerable<TEntity> GetAllDescendantsCore(TEntity parent)
        {
            if (parent == null)
                throw new TreeException(TreeExceptionEnums.None, "parent cannot be null");
            return GetAllDescendantsCoreRecursion(parent);
        }

        private IEnumerable<TEntity> GetAllDescendantsCoreRecursion(TEntity parent)
        {
            var children = Options.InstanceOperation.GetChildren(parent);
            if (children == null || !children.Any())
                yield break;

            foreach (var child in children)
            {
                if (child == null)
                    continue;
                yield return child;

                foreach (var child2 in GetAllDescendantsCoreRecursion(child))
                {
                    yield return child2;
                }

            }
        }

        /// <summary>
        /// 获取所有祖先(从小到大顺序排列)
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        protected IEnumerable<TEntity> GetAllAncestorsCore(TEntity node)
        {
            if (node == null)
                throw new TreeException(TreeExceptionEnums.None, "node cannot be null");

            CheckParentConfigure();

            while (true)
            {
                node = Options.InstanceOperation.GetParent(node);
                if (node == null)
                    yield break;
                yield return node;
            }
        }

        private void CheckParentConfigure()
        {
            if (Options.InstanceOperation.GetParent == null)
                throw new TreeException(TreeExceptionEnums.None, "Parent does not exist. This feature requires configuring Parent. Please check ConfigParent or specify TreeDefault.ParentMemberName");
        }
    }
}
