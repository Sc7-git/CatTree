using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace CatTree
{
    public class TreeOptions<TEntity, TIdType> where TEntity : ITree<TEntity, TIdType>, new()
    {
        private TreeOptionsDefault<TEntity, TIdType> _default;
        private int _minChildrenCapacity = TreeDefault.MinChildrenCapacity;
        private TIdType _rootId;
        internal Type TEntityType = typeof(TEntity);
        internal bool _isConfigured = false;

        public int MinChildrenCapacity
        {
            get => _minChildrenCapacity;
            set
            {
                if (value < 0)
                    value = 0;
                _minChildrenCapacity = value;
            }
        }
        public TreeOptionsDefault<TEntity, TIdType> Default
        {
            internal get { return _default; }
            set
            {
                _default = value ?? throw new ArgumentNullException(nameof(value));
                _default._options = this;
            }
        }

        public TreeOptions()
        {
            Default = new TreeOptionsDefault<TEntity, TIdType>();
        }

        internal TIdType RootId
        {
            get => _rootId;
            private set
            {
                _rootId = value ?? throw new TreeException(TreeExceptionEnums.None, "RootId cannot be null");
            }
        }

        public TreeOptions<TEntity, TIdType> ConfigureRootId(TIdType rootId)
        {
            if (Default.IsIllegalId(rootId))
                throw new TreeException(TreeExceptionEnums.None, "Illegal rootId value");
            RootId = rootId;
            return this;
        }

        internal Expression<Func<TEntity, TIdType>> AccessId { get; private set; }

        public TreeOptions<TEntity, TIdType> ConfigureId(Expression<Func<TEntity, TIdType>> accessId)
        {
            if (accessId is null)
            {
                throw new ArgumentNullException(nameof(accessId));
            }
            if (accessId.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new TreeException(TreeExceptionEnums.None, "ConfigureId(Expression<Func<TEntity, TIdType>> accessId) can only be a MemberAccess expression");
            }
            AccessId = accessId;
            return this;
        }

        public TreeOptions<TEntity, TIdType> ConfigureId(Expression<Func<TEntity, TIdType>> accessId, TIdType rootId)
        {
            return this.ConfigureId(accessId).ConfigureRootId(rootId);
        }

        internal Expression<Func<TEntity, IEnumerable<TEntity>>> AccessChildren { get; private set; }

        public TreeOptions<TEntity, TIdType> ConfigureChildren(Expression<Func<TEntity, IEnumerable<TEntity>>> accessChildren)
        {
            if (accessChildren is null)
            {
                throw new ArgumentNullException(nameof(accessChildren));
            }

            if (accessChildren.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new TreeException(TreeExceptionEnums.None, "ConfigureChildren(Expression<Func<TEntity, ICollection<TEntity>>> accessChildren) can only be a MemberAccess expression");
            }

            AccessChildren = accessChildren;
            return this;
        }

        internal Expression<Func<TEntity, TEntity>> AccessParent { get; private set; }

        internal Expression<Func<TEntity, TIdType>> AccessParentId { get; private set; }

        public TreeOptions<TEntity, TIdType> ConfigureParentId(Expression<Func<TEntity, TIdType>> accessParentId)
        {
            if (accessParentId is null)
            {
                throw new ArgumentNullException(nameof(accessParentId));
            }
            if (accessParentId.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new TreeException(TreeExceptionEnums.None, "ConfigureParentId(Expression<Func<TEntity, TIdType>> accessParentId) can only be a MemberAccess expression");
            }

            AccessParentId = accessParentId;
            return this;
        }

        public TreeOptions<TEntity, TIdType> ConfigureParent(Expression<Func<TEntity, TEntity>> accessParent)
        {
            if (accessParent is null)
            {
                throw new ArgumentNullException(nameof(accessParent));
            }
            if (accessParent.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new TreeException(TreeExceptionEnums.None, "ConfigureParent(Expression<Func<TEntity, TEntity>> accessParent) can only be a MemberAccess expression");
            }
            AccessParent = accessParent;
            return this;
        }

        public TreeOptions<TEntity, TIdType> ConfigureParent(Expression<Func<TEntity, TIdType>> accessParentId, Expression<Func<TEntity, TEntity>> accessParent)
        {
            return ConfigureParentId(accessParentId).ConfigureParent(accessParent);
        }

        #region 配置ChildrenType

        internal Type ChildrenType { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="ChildrenType">Children类型</typeparam>
        /// <param name="wrapChildren">包装Children函数，第一个参数是节点，第二个参数是节点的真实Children</param>
        /// <returns></returns>
        /// <exception cref="TreeException"></exception>
        public TreeOptions<TEntity, TIdType> ConfigureChildrenType<ChildrenType>(Action<TEntity, IEnumerable<TEntity>> wrapChildren = null)
            where ChildrenType : ICollection<TEntity>, new()
        {
            var childrenType = typeof(ChildrenType);
            return this.ConfigureChildrenType(childrenType, wrapChildren);
        }

        public TreeOptions<TEntity, TIdType> ConfigureChildrenType(Type childrenType, Action<TEntity, IEnumerable<TEntity>> wrapChildren = null)
        {
            //if (this.ChildrenType != null)
            //    throw new TreeException(TreeExceptionEnums.None, "Cannot configure configureChildrenType repeatedly");//不能重复配置
            if (childrenType.IsAbstract || childrenType.IsInterface)
                throw new TreeException(TreeExceptionEnums.None, "ConfigureChildrenType configuration error");
            CheckChildrenTypeForProxy(childrenType);
            this.ChildrenType = childrenType;
            Node.WrapChildren = wrapChildren;
            return this;
        }


        private TreeOptions<TEntity, TIdType> ConfigureChildrenType(ConstructorInfo constructorInfo, object[] parameters)
        {
            //constructorInfo.GetParameters
            //暂不实现： 先判断是否实现了ICollection<>，然后判断泛型参数类型是不是TEntity，如果不是且当前类型不是泛型则error。
            //如果是泛型类型，就替换泛型参数，替换后再判断ICollection<>的泛型参数类型，防止不是同一个泛型参数类型
            //较为繁琐，且不是硬性需求，并且指定构造函数的参数也是个问题
            var type = constructorInfo.ReflectedType;

            //type.GetInterface(typeof(ICollection<>).Name)

            if (true)
            {
            }
            return this;
        }

        #endregion

        public TreeNodeOptions<TEntity, TIdType> Node { get; } = new TreeNodeOptions<TEntity, TIdType>();


        internal TreeInstanceOperation<TEntity, TIdType> InstanceOperation { get; } = new TreeInstanceOperation<TEntity, TIdType>();

        internal string AccessIdMemberName { get; set; }
        internal string AccessParentIdMemberName { get; set; }
        internal string AccessChildrenMemberName { get; set; }
        internal string AccessParentMemberName { get; set; }

        internal TreeInstanceOperation<TEntity, TIdType> Configure()
        {
            if (_isConfigured)
                return InstanceOperation;
            #region Id、ParentId不能配置为同一个字段
            /*
            //优化判断 .ConfigureId(e => e.Code) 和 ConfigureParentId(e => e.Parent.Code) 不是同一字段
            //var accessIdMemberInfo = ((MemberExpression)AccessId?.Body)?.Member;
            //var accessParentIdMemberInfo = ((MemberExpression)AccessParentId?.Body)?.Member;
            //AccessIdMemberName = accessIdMemberInfo?.Name ?? TreeDefault.IdMemberName;
            //AccessParentIdMemberName = accessParentIdMemberInfo?.Name ?? TreeDefault.ParentIdMemberName;
            */
            AccessIdMemberName = GetAccessMemberName(AccessId) ?? TreeDefault.IdMemberName;
            AccessParentIdMemberName = GetAccessMemberName(AccessParentId) ?? TreeDefault.ParentIdMemberName;
            if (AccessIdMemberName == AccessParentIdMemberName)
                throw new TreeException(TreeExceptionEnums.None, $"Id and ParentId cannot be configured as the same member,Please check {nameof(ConfigureId)} or {nameof(ConfigureParentId)} or {nameof(TreeDefault)}.{nameof(TreeDefault.IdMemberName)} or {nameof(TreeDefault)}.{nameof(TreeDefault.ParentIdMemberName)}");

            #endregion

            var accessParentMemberInfo = ((MemberExpression)AccessParent?.Body)?.Member;
            AccessParentMemberName = accessParentMemberInfo?.Name ?? TreeDefault.ParentMemberName;
            var accessChildrenMemberInfo = ((MemberExpression)AccessChildren?.Body)?.Member;
            AccessChildrenMemberName = accessChildrenMemberInfo?.Name ?? TreeDefault.ChildrenMemberName;

            var optionType = typeof(TreeOptions<TEntity, TIdType>);
            TreeInstanceOperation<TEntity, TIdType> instanceOperation = InstanceOperation;
            (instanceOperation.GetId, instanceOperation.SetId, _) = BuildGetAndSetMethod(AccessId, TreeDefault.IdMemberName, $"{optionType}.{nameof(ConfigureId)}");
            (instanceOperation.GetParentId, _, _) = BuildGetAndSetMethod(AccessParentId, TreeDefault.ParentIdMemberName, $"{optionType}.{nameof(ConfigureParentId)}");
            Type childrenMemberType = default;
            (instanceOperation.GetChildren, instanceOperation.SetChildren, childrenMemberType) = BuildGetAndSetMethod(AccessChildren, TreeDefault.ChildrenMemberName, $"{optionType}.{nameof(ConfigureChildren)}");
            if (AccessParent != null || !string.IsNullOrWhiteSpace(TreeDefault.ParentMemberName))
                (instanceOperation.GetParent, instanceOperation.SetParent, _) = BuildGetAndSetMethod(AccessParent, TreeDefault.ParentMemberName, $"{optionType}.{nameof(ConfigureParent)}", true);

            if (accessChildrenMemberInfo?.Name is null)
                CheckChildrenType(childrenMemberType);
            //ChildrenType默认配置为List集合
            if (ChildrenType is null)
                //this.ConfigureChildrenTypeAsList();
                this.ConfigureChildrenTypeByChildren(childrenMemberType);
            if (!childrenMemberType.IsAssignableFrom(ChildrenType))
                throw new TreeException(TreeExceptionEnums.None, $"Please check types {optionType}.{nameof(ConfigureChildrenType)} related method calls and {TEntityType.Name}.{AccessChildrenMemberName} member");

            instanceOperation.CreateChildren = this.BuildCreateChildren();

            if (instanceOperation.GetId is null)
                throw new TreeException(TreeExceptionEnums.None, "ConfigureId not configured correctlyl");
            if (instanceOperation.GetParentId is null)
                throw new TreeException(TreeExceptionEnums.None, "ConfigureParentId not configured correctlyl");

            _isConfigured = true;
            return instanceOperation;
        }

        private (Func<TEntity, TValue> getValue, Action<TEntity, TValue> setValue, Type memberType) BuildGetAndSetMethod<TValue>(Expression<Func<TEntity, TValue>>? expression, string memberName, string configureName, bool noMemberReturnNull = false)
        {
            if (expression is { })
            {
                return TreeExpressionExtensions.BuildGetAndSetMethod(expression);
            }
            else
            {
                try
                {
                    return TreeExpressionExtensions.BuildDefaultGetAndSetMethod<TEntity, TValue>(memberName, noMemberReturnNull);
                }
                //catch (TreeException)
                //{
                //    throw;
                //}
                catch (Exception ex)
                {
                    throw new TreeException(TreeExceptionEnums.None, $"Not configured {configureName} or {TEntityType.Name}.{memberName} does not exist,Please refer to the InnerException for detailed errors", ex);
                }
            }
        }

        private void CheckChildrenTypeForProxy(Type type)
        {
            //IsAssignableFrom不能用空泛型
            //if (type.IsGenericType)
            //{
            //    var childrenGenericTypeDefinition = type.GetGenericTypeDefinition();
            //    if (!typeof(ICollection<>).IsAssignableFrom(childrenGenericTypeDefinition))
            //        throw new TreeException(TreeExceptionEnums.None, $"Type {AccessChildrenMemberName} must implement ICollection<{TEntityType.Name}>");
            //}
            if (!typeof(ICollection<TEntity>).IsAssignableFrom(type))
                throw new TreeException(TreeExceptionEnums.None, $"Type {AccessChildrenMemberName} must implement ICollection<{TEntityType.Name}>");
        }

        private void CheckChildrenType(Type type)
        {
            CheckChildrenTypeForMember(type);
        }

        private void CheckChildrenTypeForMember(Type type)
        {
            if (!typeof(IEnumerable<TEntity>).IsAssignableFrom(type))
                throw new TreeException(TreeExceptionEnums.None, $"Type {AccessChildrenMemberName} must implement IEnumerable<{TEntityType.Name}>");
        }


        internal string GetAccessMemberName(LambdaExpression lambda)
        {
            if (lambda is null)
                return null;
            var member = lambda.Body as MemberExpression;
            if (member is null)
                return null;
            var memberName = member.Member.Name;
            Expression expression = member.Expression;
            if (expression.NodeType != ExpressionType.Parameter)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Complex expression {lambda.ToString()} is not supported");
            }

            #region todo:是否开放多层级？？？例如：e.Parent.Code，目前只支持e.PCode
            //参数访问表达式是倒着的，例如e.Parent.Code, lambda.Body获取到的第一个表达式是Code，lambda.Body.Expression获取到的是Parent，lambda.Body.Expression.Expression获取到的是e。
            /*
            while (true)
            {
                if (expression.NodeType == ExpressionType.MemberAccess)
                {
                    MemberExpression memberExpression = (MemberExpression)expression;
                    memberName = $"{memberExpression.Member.Name}.{memberName}";
                    expression = memberExpression.Expression;

                }
                else if (expression.NodeType == ExpressionType.Call)
                {
                    throw new TreeException(TreeExceptionEnums.None, $"Complex expression {lambda.ToString()} is not supported");
                    //MethodCallExpression call = expression as MethodCallExpression;
                    //expression = call.Arguments[0];
                }
                else
                {
                    break;
                }
            } 
            */
            #endregion

            return memberName;
        }
    }

}
