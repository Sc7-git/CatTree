using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatTree
{
    public class TreeNodeOptions<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {

        //internal ChildrenOperationEnum CleanUpChildren { get; set; } = ChildrenOperationEnum.New;

        /// <summary>
        /// 根节点Children为空报错
        /// </summary>
        public bool IfTheRootHasEmptyChildrenThrowException { get; set; } = false;

        /// <summary>
        /// 允许未知Parent
        /// </summary>
        public bool AllowUnknownParent { get; set; } = true;

        internal Action<TEntity, IEnumerable<TEntity>> WrapChildren { private get; set; }

        public void ExecuteWrapChildren(TEntity node, IEnumerable<TEntity> sourceChildren)
        {
            try
            {
                WrapChildren?.Invoke(node, sourceChildren);
            }
            catch (Exception ex)
            {

                throw new TreeException(TreeExceptionEnums.None, "WrapChildren failed, please check the parameter wrapChildren for ConfigChildrenType. Abnormal information reference innerException", ex);
            }
        }

        public bool CanExecuteWrapChildren => WrapChildren != null;
    }

}
