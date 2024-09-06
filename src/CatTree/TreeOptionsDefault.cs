using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CatTree
{


    /// <summary>
    /// 可以派生此类，重写虚方法
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TIdType"></typeparam>
    public class TreeOptionsDefault<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {
        internal TreeOptions<TEntity, TIdType> _options;

        protected TreeOptions<TEntity, TIdType> Options => _options;


        protected virtual void CheckNode(TEntity entity)
        {

        }

        protected virtual void CheckOptions()
        {

        }

        /// <summary>
        /// 未知父级处理函数
        /// </summary>
        protected virtual void UnknownParentHandler(TEntity entity)
        {
            //override skip or error?
            throw new TreeException(TreeExceptionEnums.None, $"Unable to find {InstanceOperation.GetId(entity)} parent {InstanceOperation.GetParentId(entity)}");
        }

        internal void UnknownParentHandlerInternal(TEntity entity)
        {
            UnknownParentHandler(entity);
        }

        protected TreeInstanceOperation<TEntity, TIdType> InstanceOperation => Options.InstanceOperation;

        /// <summary>
        /// 检查列表元素
        /// 1、正确配置获取id、parentId的委托
        /// 2、id不能为null
        /// 3、只有root的parentid才允许为null
        /// 4、id和parentId不能相同
        /// </summary>
        /// <exception cref="TreeException"></exception>
        internal void CheckNodeInternal(TEntity node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));
            #region configure中已判断
            //if (InstanceOperation.GetId is null)
            //    throw new TreeException(TreeExceptionEnums.None, "ConfigureId not configured correctlyl");
            //if (InstanceOperation.GetParentId is null)
            //    throw new TreeException(TreeExceptionEnums.None, "ConfigureParentId not configured correctlyl"); 
            #endregion
            TIdType id = default;
            try
            {
                id = InstanceOperation.GetId(node);
            }
            catch (NullReferenceException ex)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Accessing {Options.AccessId.ToString()} encountered a \"NullReferenceException\", please ensure data correctness.", ex);
            }

            TIdType parentId = default;
            try
            {
                parentId = InstanceOperation.GetParentId(node);
            }
            catch (NullReferenceException ex)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Accessing {Options.AccessParentId.ToString()} encountered a \"NullReferenceException\", please ensure data correctness.", ex);
            }
            CheckIllegalId(id);
            CheckIllegalParentId(id, parentId);
            CheckNode(node);
        }

        /// <summary>
        /// 不支持id为null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal bool IsIllegalId(TIdType id)
        {
            return id == null;
        }

        /// <summary>
        /// 不支持id为null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        internal void CheckIllegalId(TIdType id)
        {
            if (IsIllegalId(id))
            {
                IllegalIdThrowException();
            }
        }
        internal void CheckIllegalParentId(TIdType id, TIdType parentId)
        {
            if (parentId == null)
            {
                if (!id.Equals(Options.RootId))
                    throw new TreeException(TreeExceptionEnums.None, $"Illegal {Options.AccessParentIdMemberName}(The result of ConfigureParentId), only the root {Options.AccessParentIdMemberName}(The result of ConfigureParentId) is allowed to be null");
            }

            if (id.Equals(parentId))
            {
                throw new TreeException(TreeExceptionEnums.None, $"{Options.AccessIdMemberName}(The result of ConfigureId) and {Options.AccessParentIdMemberName}(The result of ConfigureParentId) value cannot be the same");
            }
        }

        internal void IllegalIdThrowException()
        {
            throw new TreeException(TreeExceptionEnums.None, $"Illegal {Options.AccessIdMemberName}(The result of ConfigureId) value , Cannot be null");
        }

        internal void IllegalParentIdThrowException()
        {
            throw new TreeException(TreeExceptionEnums.None, $"Illegal {Options.AccessParentIdMemberName}(The result of ConfigureParentId) value , Cannot be null");
        }

        internal void CheckOptionsInternal()
        {

            //_ = Options.RootId ?? throw new TreeException(TreeExceptionEnums.None, "RootId cannot be null");
            CheckOptions();
        }


        internal void AddChild(InternalTreeNode<TEntity, TIdType> parent, TEntity child)
        {
            if (parent.SourceChildren == null)
            {
                parent.SourceChildren = InstanceOperation.CreateChildren();

                if (parent.SourceChildren == null)
                    throw new TreeException(TreeExceptionEnums.None, $"Unable to instantiate {Options.TEntityType.Name}.{Options.AccessChildrenMemberName}(The result of ConfigureChildren),Please check the collection type configuration");
                if (parent.SourceChildren.IsReadOnly)
                    throw new TreeException(TreeExceptionEnums.None, $"{Options.TEntityType.Name}.{Options.AccessChildrenMemberName} cannot be a read-only collect,Please check the {Options.TEntityType.Name}.{Options.AccessChildrenMemberName} collection type");

                //不包装结果，直接赋值给parent
                if (!Options.Node.CanExecuteWrapChildren)
                    InstanceOperation.SetChildren(parent.Node, parent.SourceChildren);
            }
            parent.SourceChildren.Add(child);
        }

        [Obsolete]
        private ICollection<TEntity> EnsureGetChildren(TEntity tree)
        {
            var children = (ICollection<TEntity>)InstanceOperation.GetChildren(tree);
            if (children == null)
            {
                children = InstanceOperation.CreateChildren();
                if (children == null)
                    throw new TreeException(TreeExceptionEnums.None, $"Unable to instantiate {Options.TEntityType.Name}.{Options.AccessChildrenMemberName}(The result of ConfigureChildren),Please check the collection type configuration");
                if (children.IsReadOnly)
                    throw new TreeException(TreeExceptionEnums.None, $"{Options.TEntityType.Name}.{Options.AccessChildrenMemberName} cannot be a read-only collect,Please check the collection type configuration");
                InstanceOperation.SetChildren(tree, children);
            }
            return children;
        }

        [Obsolete]
        private void AddChild(TEntity parent, TEntity child)
        {
            var children = EnsureGetChildren(parent);
            AddChild(children, child);
        }

        [Obsolete]
        private void AddChild(ICollection<TEntity> children, TEntity child)
        {
            children?.Add(child);
        }

    }

}
