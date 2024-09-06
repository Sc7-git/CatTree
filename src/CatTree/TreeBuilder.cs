using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CatTree
{
    public static class TreeBuilder
    {
        public static TreeResult<TEntity, TIdType> Build<TEntity, TIdType>(IEnumerable<ITree<TEntity, TIdType>> nodes, Action<TreeOptions<TEntity, TIdType>> configureOptions = null)
            where TEntity : ITree<TEntity, TIdType>, new()
        {

            #region 解析配置
            var options = new TreeOptions<TEntity, TIdType>();
            configureOptions?.Invoke(options);
            var instanceOperation = options.Configure();
            options.Default.CheckOptionsInternal();
            #endregion

            return Build(nodes, options);
        }

        public static TreeResult<TEntity, TIdType> Build<TEntity, TIdType>(IEnumerable<ITree<TEntity, TIdType>> nodes, TIdType rootId
            , Expression<Func<TEntity, TIdType>> accessId = null
            , Expression<Func<TEntity, TIdType>> accessParentId = null
            , Expression<Func<TEntity, IEnumerable<TEntity>>> accessChildren = null)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return Build(nodes, options =>
            {
                options.ConfigureRootId(rootId);
                if (accessId != null)
                    options.ConfigureId(accessId);
                if (accessParentId != null)
                    options.ConfigureParentId(accessParentId);
                if (accessChildren != null)
                    options.ConfigureChildren(accessChildren);
            });
        }

        public static TreeOptions<TEntity, TIdType> Configure<TEntity, TIdType>(Action<TreeOptions<TEntity, TIdType>> configureOptions) where TEntity : ITree<TEntity, TIdType>, new()
        {
            #region 解析配置
            var options = new TreeOptions<TEntity, TIdType>();
            configureOptions?.Invoke(options);
            options.Configure();
            #endregion
            return options;
        }

        public static TreeResult<TEntity, TIdType> Build<TEntity, TIdType>(IEnumerable<ITree<TEntity, TIdType>> nodes, TreeOptions<TEntity, TIdType> options = null)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            #region 参数校验
            if (nodes == null || !nodes.Any())
                throw new ArgumentException(nameof(nodes));
            #endregion


            #region 异步构建对象
            //ConcurrentQueue<InternalTreeNode<TEntity, TIdType>> internalTreeNodes = new ConcurrentQueue<InternalTreeNode<TEntity, TIdType>>();
            ////List<Task> tasks = new List<Task>();
            //var offset = nodes.Count() / 30;
            //var remainder = nodes.Count() % 30;
            //for (int i = 0; i < 30; i++)
            //{
            //    var task = Task.Run(() =>
            //    {
            //        for (int j = 0; j < offset; j++)
            //        {
            //            internalTreeNodes.Enqueue(new InternalTreeNode<TEntity, TIdType>());
            //        }
            //    });
            //    //tasks.Add(task);
            //}

            //var task2 = Task.Run(() =>
            //{
            //    for (int i = 0; i < remainder; i++)
            //    {
            //        internalTreeNodes.Enqueue(new InternalTreeNode<TEntity, TIdType>());
            //    }
            //});
            ////tasks.Add(task2);
            ////Task.WaitAll(tasks.ToArray()); 
            #endregion


            #region 解析配置
            //var options = new TreeOptions<TEntity, TIdType>();
            //configureOptions?.Invoke(options);
            //var instanceOperation = options.Configure();
            //options.Default.CheckOptionsInternal();
            if (options is null)
                options = new TreeOptions<TEntity, TIdType>();
            if (!options._isConfigured)
                options.Configure();
            var instanceOperation = options.InstanceOperation;
            options.Default.CheckOptionsInternal();
            #endregion

            #region 泛型推断
            //泛型推断类型Tree<TEntity, TIdType>改为TEntity
            //在后续的读写item中会用到TEntity类型，而不会再用TEntity的基类Tree<TEntity, TIdType>类型
            IEnumerable<TEntity> nodes2;
            //items2 = items.Cast<TEntity>(); 
            //items2 = items as IEnumerable<TEntity>;
            nodes2 = (IEnumerable<TEntity>)nodes;
            //items2 = items.OfType<TEntity>(); //效率稍低 稍微吃内存
            #endregion

            #region 集合转Hash
            //var dict = CheckNodeAndConvertToHash(nodes2, options, instanceOperation);
            var dict = CheckNodeAndConvertToHash2(nodes2, options, instanceOperation);
            #endregion

            #region 获取根对象
            var rootId = options.RootId;
            //TEntity root = GetRoot(dict, rootId, instanceOperation, true);
            var root = GetRoot2(dict, rootId, instanceOperation, true);
            #endregion

            foreach (var item in dict.Values)
            {
                //TEntity parent;
                InternalTreeNode<TEntity, TIdType> parent;
                var parentId = instanceOperation.GetParentId(item.Node);
                if (parentId.Equals(rootId))
                    parent = root;
                else
                {
                    if (!dict.TryGetValue(parentId, out parent))
                    {
                        if (!options.Node.AllowUnknownParent)
                            options.Default.UnknownParentHandlerInternal(item.Node);
                        continue;
                    }
                }
                //options.Default.AddChild(parent.Node, item.Node);
                options.Default.AddChild(parent, item.Node);

                //if (instanceOperation.SetParent is { })
                //    instanceOperation.SetParent(item.Node, parent.Node);
                instanceOperation.SetParent?.Invoke(item.Node, parent.Node);
            }

            #region 不支持，会引发真正的根节点的pid找不到上级。 一个奇怪的操作：允许非根节点扮演根节点，以下操作是保证TreeResult的逻辑正常，这种情况依然能在TreeResult获取真正的根节点

            //var rootParentId = options.InstanceOperation.GetParentId(root);
            //if (rootParentId != null)
            //{
            //    if (dict.TryGetValue(rootParentId, out var rootParent))
            //    {
            //        options.Default.AddChild(rootParent, root);
            //    }
            //}

            #endregion

            if (options.Node.IfTheRootHasEmptyChildrenThrowException)
            {
                var rootChildren = options.InstanceOperation.GetChildren(root.Node);
                if (rootChildren is null || !rootChildren.Any())
                    throw new TreeException(TreeExceptionEnums.None, "Root node has no Children");
            }



            //todo：扩展TrimExcess，利用反射
            /*
            if (false)
            {
                foreach (var item in items)
                {
                    var children = options.InstanceOperation.GetChildren(item);
                    if (children is not { })
                        continue;
                    if (children is List<TEntity> list)
                    {
                        list.TrimExcess();
                    }
                    if (children is HashSet<TEntity> hashSet)
                    {
                        hashSet.TrimExcess();
                    }
                }
            }
            */

            dict.TryAdd(rootId, root);

            if (options.Node.CanExecuteWrapChildren)
            {
                foreach (InternalTreeNode<TEntity, TIdType> item in dict.Values)
                {
                    //options.Node.WrapChildren?.Invoke(item.Node, item.SourceChildren);
                    options.Node.ExecuteWrapChildren(item.Node, item.SourceChildren);
                }
            }

            //if (options.Node.CleanUpChildren == ChildrenOperationEnum.New)
            //{
            //    foreach (TreeNodeHashValue<TEntity, TIdType> item in dict.Values)
            //    {
            //        if (options.Node.TransferChildrenToChildren != null)
            //        {
            //            options.Node.TransferChildrenToChildren?.Invoke(item);
            //        }
            //        else
            //        {
            //            options.InstanceOperation.SetChildren(item.Node, item.SafeTransferChildren);
            //        }
            //    }
            //}

            var result = new TreeResult<TEntity, TIdType>(root.Node, dict, options);

            return result;
        }


        [Obsolete]
        private static IDictionary<TIdType, TEntity> CheckNodeAndConvertToHash<TIdType, TEntity>(IEnumerable<TEntity> nodes, TreeOptions<TEntity, TIdType> options, TreeInstanceOperation<TEntity, TIdType> instanceOperation) where TEntity : ITree<TEntity, TIdType>, new()
        {
            return nodes.ToDictionary(node =>
            {
                options.Default.CheckNodeInternal(node);
                //options.Default.CleanUpChildren(node, options.Node.CleanUpChildren);
                var id = options.InstanceOperation.GetId(node);

                return id;
            }, node => node);

            //var nodes2 = CheckNodeAndConvertToHashCore(nodes, options, instanceOperation);
            //return new Dictionary<TIdType, TEntity>(nodes2);
        }

        [Obsolete]
        private static IEnumerable<KeyValuePair<TIdType, TEntity>> CheckNodeAndConvertToHashCore<TIdType, TEntity>(IEnumerable<TEntity> nodes, TreeOptions<TEntity, TIdType> options, TreeInstanceOperation<TEntity, TIdType> instanceOperation) where TEntity : ITree<TEntity, TIdType>, new()
        {
            foreach (var node in nodes)
            {
                options.Default.CheckNodeInternal(node);
                //options.Default.CleanUpChildren(node, options.Node.CleanUpChildren);
                var id = options.InstanceOperation.GetId(node);
                yield return new KeyValuePair<TIdType, TEntity>(id, node);
            }
        }

        private static IDictionary<TIdType, InternalTreeNode<TEntity, TIdType>> CheckNodeAndConvertToHash2<TIdType, TEntity>(IEnumerable<TEntity> nodes, TreeOptions<TEntity, TIdType> options, TreeInstanceOperation<TEntity, TIdType> instanceOperation, ConcurrentQueue<InternalTreeNode<TEntity, TIdType>> internalTreeNodes = null) where TEntity : ITree<TEntity, TIdType>, new()
        {
            #region 并发,任务实在是不耗时，还不如串行
            //var dict = new ConcurrentDictionary<TIdType, InternalTreeNode<TEntity, TIdType>>();
            //Parallel.ForEach(nodes, node =>
            //{
            //    options.Default.CheckNodeInternal(node);
            //    var id = options.InstanceOperation.GetId(node);
            //    dict.TryAdd(id, new InternalTreeNode<TEntity, TIdType>() { Node = node });
            //});
            //return dict; 
            #endregion

            //1
            var dict = new Dictionary<TIdType, InternalTreeNode<TEntity, TIdType>>(nodes.Count());
            //var dict2 = new Dictionary<TIdType, TEntity>(nodes.Count());
            //foreach (var node in nodes)
            //{
            //    options.Default.CheckNodeInternal(node);
            //    var id = options.InstanceOperation.GetId(node);
            //    dict2.Add(id, node);
            //}

            foreach (var node in nodes)
            {
                options.Default.CheckNodeInternal(node);
                var id = options.InstanceOperation.GetId(node);
                if (!dict.TryAdd(id, new InternalTreeNode<TEntity, TIdType>() { Node = node }))
                {
                    throw new TreeException(TreeExceptionEnums.None, "Id duplicate");
                }
            }
            return dict;

            //2
            //return nodes.ToDictionary(node =>
            //{
            //    options.Default.CheckNodeInternal(node);
            //    var id = options.InstanceOperation.GetId(node);

            //    return id;
            //}, node =>
            //{
            //    return new InternalTreeNode<TEntity, TIdType>() { Node = node };
            //    //while (true)
            //    //{
            //    //    if (internalTreeNodes.TryDequeue(out var internalTreeNode))
            //    //    {
            //    //        internalTreeNode.Node = node;
            //    //        return internalTreeNode;
            //    //    }
            //    //}
            //});
        }

        /// <summary>
        /// 从dict中获取root，如果没有就实例化root
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TIdType"></typeparam>
        /// <param name="dict"></param>
        /// <param name="rootId"></param>
        /// <param name="removeFromDict">从dict中删除root</param>
        /// <returns></returns>
        [Obsolete("不建议创建root对象，因为1、没有root的完整信息，2root对象的parentId是多少会对后续操作造成误导(例如root.id是0，parentId应该实例化为？)")]
        private static TEntity GetRootOrNew<TEntity, TIdType>(IDictionary<TIdType, TEntity> dict, TIdType rootId, TreeInstanceOperation<TEntity, TIdType> instanceOperation, bool removeFromDict = false) where TEntity : ITree<TEntity, TIdType>, new()
        {
            if (!dict.TryGetValue(rootId, out TEntity root))
            {
                root = new TEntity();
                instanceOperation.SetId(root, rootId);
            }
            else if (removeFromDict)
                dict.Remove(rootId);
            return root;
        }

        /// <summary>
        /// 从dict中获取root
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TIdType"></typeparam>
        /// <param name="dict"></param>
        /// <param name="rootId"></param>
        /// <param name="removeFromDict">从dict中删除root</param>
        /// <returns></returns>
        private static TEntity GetRoot<TEntity, TIdType>(IDictionary<TIdType, TEntity> dict, TIdType rootId, TreeInstanceOperation<TEntity, TIdType> instanceOperation, bool removeFromDict = false) where TEntity : ITree<TEntity, TIdType>, new()
        {
            if (!dict.TryGetValue(rootId, out TEntity root))
            {
                throw new TreeException(TreeExceptionEnums.None, "No root");
            }
            else if (removeFromDict)
                dict.Remove(rootId);

            if (instanceOperation.GetParentId(root) is TIdType parentId && parentId != null)
            {
                if (dict.TryGetValue(parentId, out _))
                {
                    throw new TreeException(TreeExceptionEnums.None, $"{rootId} has parent {parentId}");
                }
            }
            return root;
        }


        private static InternalTreeNode<TEntity, TIdType> GetRoot2<TEntity, TIdType>(IDictionary<TIdType, InternalTreeNode<TEntity, TIdType>> dict, TIdType rootId, TreeInstanceOperation<TEntity, TIdType> instanceOperation, bool removeFromDict = false) where TEntity : ITree<TEntity, TIdType>, new()
        {
            if (!dict.TryGetValue(rootId, out InternalTreeNode<TEntity, TIdType> root))
            {
                throw new TreeException(TreeExceptionEnums.None, "No root");
            }
            else if (removeFromDict)
                dict.Remove(rootId);

            if (instanceOperation.GetParentId(root.Node) is TIdType parentId && parentId != null)
            {
                if (dict.TryGetValue(parentId, out _))
                {
                    throw new TreeException(TreeExceptionEnums.None, $"{rootId} has parent {parentId},{rootId} not RootId");
                }
            }
            return root;
        }

    }


}
