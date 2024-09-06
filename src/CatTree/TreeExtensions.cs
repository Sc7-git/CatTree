using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace CatTree
{
    public static class TreeExtensions
    {

        public static TreeResult<TEntity, TIdType> BuildTree<TEntity, TIdType>(this IEnumerable<ITree<TEntity, TIdType>> nodes, Action<TreeOptions<TEntity, TIdType>> configureOptions = null)
           where TEntity : ITree<TEntity, TIdType>, new()
        {
            return TreeBuilder.Build(nodes, configureOptions);
        }

        public static TreeResult<TEntity, TIdType> BuildTree<TEntity, TIdType>(this IEnumerable<ITree<TEntity, TIdType>> nodes, TIdType rootId
            , Expression<Func<TEntity, TIdType>> accessId = null
            , Expression<Func<TEntity, TIdType>> accessParentId = null
            , Expression<Func<TEntity, IEnumerable<TEntity>>> accessChildren = null)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return TreeBuilder.Build(nodes, rootId, accessId, accessParentId, accessChildren);
        }


        public static TreeResult<TEntity, TIdType> BuildTree<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options, IEnumerable<ITree<TEntity, TIdType>> nodes)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return TreeBuilder.Build(nodes, options);
        }

        public static TreeResult<TEntity, TIdType> BuildTree<TEntity, TIdType>(this IEnumerable<ITree<TEntity, TIdType>> nodes, TreeOptions<TEntity, TIdType> options)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return TreeBuilder.Build(nodes, options);
        }

        internal static TreeOptions<TEntity, TIdType> AutoConfigureChildrenType<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options)
           where TEntity : ITree<TEntity, TIdType>, new()
        {
            //通过此配置决定是否调用ConfigureChildrenTypeByChildren
            throw new NotImplementedException();
        }

        internal static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeByChildren<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options, MemberInfo member)
           where TEntity : ITree<TEntity, TIdType>, new()
        {
            var memberType = member.MemberType is MemberTypes.Property ? ((PropertyInfo)member).PropertyType : ((FieldInfo)member).FieldType;
            return options.ConfigureChildrenTypeByChildren(memberType);
        }

        internal static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeByChildren<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options, Type memberType)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            if (memberType.IsGenericType)
            {
                var memberGenericTypeDefinition = memberType.GetGenericTypeDefinition();

                var types1 = new Type[] { typeof(IEnumerable<>), typeof(ICollection<>), typeof(IList<>), typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>) };
                if (types1.Contains(memberGenericTypeDefinition))
                    return options.ConfigureChildrenTypeAsList();
                //var types2 = new Type[] { typeof(IReadOnlyCollection<>), typeof(IReadOnlyList<>) };
                //if (types2.Contains(memberGenericTypeDefinition))
                //    return options.ConfigureChildrenTypeAsReadOnlyCollection();

                if (memberGenericTypeDefinition == typeof(List<>))
                    return options.ConfigureChildrenTypeAsList();
                if (memberGenericTypeDefinition == typeof(HashSet<>))
                    return options.ConfigureChildrenTypeAsHashSet();
                if (memberGenericTypeDefinition == typeof(Collection<>))
                    return options.ConfigureChildrenTypeAsCollection();
                if (memberGenericTypeDefinition == typeof(ReadOnlyCollection<>))
                    return options.ConfigureChildrenTypeAsReadOnlyCollection();
#if NETCOREAPP3_0_OR_GREATER
            if (memberGenericTypeDefinition == typeof(System.Collections.Immutable.ImmutableList<>))
                return options.ConfigureChildrenTypeAsImmutableList();

#endif
            }
            //else
            //{

            //    throw new TreeException(TreeExceptionEnums.None, "Explicitly configuring the ConfigChildrenType series method is required");
            //}
            //todo:增强
            return options.ConfigureChildrenType(memberType);

        }


        public static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeAsList<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return options.ConfigureChildrenType<List<TEntity>>();
        }

        public static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeAsHashSet<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return options.ConfigureChildrenType<HashSet<TEntity>>();
        }

        /// <summary>
        /// Collection<T>是用作派生类来提供自定义行为的基类，提供此类仅是测试无参构造函数使用，就算用了也没啥坏事。
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <typeparam name="TIdType"></typeparam>
        /// <param name="options"></param>
        /// <returns></returns>
        [Obsolete("Collection<T>is a base class used as a derived class to provide custom behavior. Providing this class is only for testing the use of non parametric constructors, and even if used, it is not a bad thing.", false)]
        public static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeAsCollection<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return options.ConfigureChildrenType<Collection<TEntity>>();
        }

        public static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeAsReadOnlyCollection<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options)
            where TEntity : ITree<TEntity, TIdType>, new()
        {
            return options.ConfigureChildrenType<List<TEntity>>((node, sourceChildren) =>
            {
                if (sourceChildren != null)
                {
                    var children = sourceChildren as IList<TEntity>;
                    options.InstanceOperation.SetChildren(node, new ReadOnlyCollection<TEntity>(children));
                }
            });
        }

#if NETCOREAPP3_0_OR_GREATER

        public static TreeOptions<TEntity, TIdType> ConfigureChildrenTypeAsImmutableList<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options)
           where TEntity : ITree<TEntity, TIdType>, new()
        {
            return options.ConfigureChildrenType<List<TEntity>>((node, sourceChildren) =>
            {
                if (sourceChildren != null)
                {
                    //var children = sourceChildren.ToImmutableList();
                    var children = System.Collections.Immutable.ImmutableList.ToImmutableList(sourceChildren);
                    options.InstanceOperation.SetChildren(node, children);
                }
            });
        }
#endif


    }
}
