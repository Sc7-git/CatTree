using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CatTree
{
    public static class TreeExpressionExtensions//<TEntity, TIdType> where TEntity : Tree<TEntity, TIdType>, new()
    {


        /// <summary>
        /// user对象和值都是通过参数传递
        /// </summary>
        /// <param name="expression"></param>
        /// <exception cref="Exception"></exception>
        public static (Func<TEntity, TValue> getValue, Action<TEntity, TValue> setValue, Type memberType) BuildGetAndSetMethod<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression.NodeType != ExpressionType.Lambda)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Only supported Lambda");
            }

            if (expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Only supported ExpressionType.MemberAccess");
            }
            var stringType = typeof(string);
            var objectType = typeof(object);
            var entityType = typeof(TEntity);
            var valueType = typeof(TValue);

            var parameter = expression.Parameters[0];
            var memberExpression = (MemberExpression)expression.Body;
            var member = memberExpression.Member;

            if (!(member is PropertyInfo) && !(member is FieldInfo))
            {
                throw new TreeException(TreeExceptionEnums.None, $"Only supported Property or Field");
            }

            #region 声明表达式属性或字段
            //表达式右边是参数访问，可以直接使用,无需声明表达式属性或字段
            /*
            if (member is PropertyInfo)
            {
                memberExpression = Expression.Property(Expression.Constant(user), member as PropertyInfo);
            }
            else if (member is FieldInfo)
            {
                memberExpression = Expression.Field(Expression.Constant(user), member as FieldInfo);
            }
            else
            {
                  throw new Exception($"Only supported Property or Field");
            }
            */
            #endregion

            var parameter2 = Expression.Parameter(typeof(TValue), "value");

            var memberType = memberExpression.Member.MemberType is MemberTypes.Property ? ((PropertyInfo)memberExpression.Member).PropertyType : ((FieldInfo)memberExpression.Member).FieldType;
            Expression assignRight;
            Expression parameter2Check = default;
            if (valueType != memberType)
            {

                var parameterRuntimeType = Expression.Call(parameter2, objectType.GetMethod("GetType"));
                //运行时判断参数类型是否正确
                var callIsAssignableFrom = Expression.Call(Expression.Constant(memberType), typeof(Type).GetMethod(nameof(memberType.IsAssignableFrom)), parameterRuntimeType);
                var checkCondition = Expression.AndAlso(Expression.NotEqual(parameter2, Expression.Constant(null, memberType)), Expression.IsFalse(callIsAssignableFrom));
                //parameter2Check = Expression.IfThen(Expression.IsFalse(callIsAssignableFrom),
                parameter2Check = Expression.IfThen(checkCondition,

                    Expression.Throw(Expression.New(typeof(TreeException).GetConstructor(new Type[] { typeof(TreeExceptionEnums), stringType }),
                    new Expression[]{
                        Expression.Constant(TreeExceptionEnums.None),
                        Expression.Call(null, stringType.GetMethod(nameof(string.Format), new Type[] { stringType, objectType, objectType }), new Expression[]
                        {
                            //Expression.Constant("Tree and configurexx have configured the wrong type:{0}<-->{1}", typeof(string)),
                            Expression.Constant($"{member.Name} have configured the wrong type:{{0}}<-->{{1}};Please check the tree class or TreeOptions or {nameof(TreeDefault)}", typeof(string)),
                            Expression.Constant(memberType.Name),
                            Expression.Property(parameterRuntimeType, nameof(Type.Name))
                        })
                    })));

                //parameter2参数类型转换为parameter成员的的类型
                var parameter2TypeAs = Expression.TypeAs(parameter2, memberType);
                assignRight = parameter2TypeAs;
            }
            else
            {
                assignRight = parameter2;
            }
            //Expression assignmentExpression = Expression.Assign(memberExpression, parameter2);
            Expression assignmentExpression = Expression.Assign(memberExpression, assignRight);
            Expression setLambda = assignmentExpression;
            if (parameter2Check is { })
            {
                setLambda = Expression.Block(parameter2Check, assignmentExpression);
            }

            Expression<Action<TEntity, TValue>> setValueExpression = Expression.Lambda<Action<TEntity, TValue>>(setLambda, parameter, parameter2);
            var setValue = setValueExpression.Compile();

            return (getValue: expression.Compile(), setValue, memberType);

        }

        /// <summary>
        /// user对象和值都是通过参数传递
        /// </summary>
        /// <param name="expression"></param>
        /// <exception cref="Exception"></exception>
        private static (Func<TEntity, TValue> getValue, Action<TEntity, TValue> setValue) BuildGetAndSetMethod简化<TEntity, TValue>(Expression<Func<TEntity, TValue>> expression)
        {
            if (expression is null)
            {
                throw new ArgumentNullException(nameof(expression));
            }

            if (expression.NodeType != ExpressionType.Lambda)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Only supported Lambda");
            }

            if (expression.Body.NodeType != ExpressionType.MemberAccess)
            {
                throw new TreeException(TreeExceptionEnums.None, $"Only supported ExpressionType.MemberAccess");
            }
            var treeParameterExpression = expression.Parameters[0];
            var body = (MemberExpression)expression.Body;
            var member = body.Member;

            if (!(member is PropertyInfo) && !(member is FieldInfo))
            {
                throw new TreeException(TreeExceptionEnums.None, $"Only supported Property or Field");
            }

            #region 声明表达式属性或字段
            //表达式右边是参数访问，可以直接使用,无需声明表达式属性或字段
            Expression memberExpression = body;
            /*
            if (member is PropertyInfo)
            {
                memberExpression = Expression.Property(Expression.Constant(user), member as PropertyInfo);
            }
            else if (member is FieldInfo)
            {
                memberExpression = Expression.Field(Expression.Constant(user), member as FieldInfo);
            }
            else
            {
                  throw new Exception($"Only supported Property or Field");
            }
            */
            #endregion

            var parameter2 = Expression.Parameter(typeof(TValue), "value");
            Expression assignmentExpression = Expression.Assign(memberExpression, parameter2);

            Expression<Action<TEntity, TValue>> setValueExpression = Expression.Lambda<Action<TEntity, TValue>>(assignmentExpression, treeParameterExpression, parameter2);
            var setValue = setValueExpression.Compile();

            return (getValue: expression.Compile(), setValue);

        }

        /// <summary>
        /// user对象和值都是通过参数传递,并且属性是通过字符串指定的
        /// </summary>
        /// <param name="user"></param>
        /// <param name="value"></param>
        /// <param name="memberName"></param>
        /// <exception cref="Exception"></exception>
        public static (Func<TEntity, TValue> getValue, Action<TEntity, TValue> setValue, Type memberType) BuildDefaultGetAndSetMethod<TEntity, TValue>(string memberName, bool noMemberReturnNull = false)
        {
            if (memberName is null)
            {
                throw new ArgumentNullException(nameof(memberName));
            }
            var stringType = typeof(string);
            var objectType = typeof(object);
            var entityType = typeof(TEntity);
            var members = entityType.GetMember(memberName, BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (members.Length != 1)
            {
                if (!members.Any() && noMemberReturnNull)
                    return (null, null, null);
                throw new TreeException(TreeExceptionEnums.None, $"Unclear or undefined member {memberName}");
            }
            //var member = members.First();

            var parameter = Expression.Parameter(entityType, "entity");
            MemberExpression memberExpression = Expression.PropertyOrField(parameter, memberName);

            var valueType = typeof(TValue);
            var parameter2 = Expression.Parameter(valueType, "value");

            var memberType = memberExpression.Member.MemberType is MemberTypes.Property ? ((PropertyInfo)memberExpression.Member).PropertyType : ((FieldInfo)memberExpression.Member).FieldType;
            Expression assignRight;
            Expression parameter2Check = default;
            if (valueType != memberType)
            {

                var parameterRuntimeType = Expression.Call(parameter2, objectType.GetMethod("GetType"));
                //运行时判断参数类型是否正确
                var callIsAssignableFrom = Expression.Call(Expression.Constant(memberType), typeof(Type).GetMethod(nameof(memberType.IsAssignableFrom)), parameterRuntimeType);
                var checkCondition = Expression.AndAlso(Expression.NotEqual(parameter2, Expression.Constant(null, memberType)), Expression.IsFalse(callIsAssignableFrom));
                parameter2Check = Expression.IfThen(checkCondition,
                //parameter2Check = Expression.IfThen(Expression.IsFalse(callIsAssignableFrom),
                Expression.Throw(Expression.New(typeof(TreeException).GetConstructor(new Type[] { typeof(TreeExceptionEnums), stringType }),
                    new Expression[]{
                        Expression.Constant(TreeExceptionEnums.None),
                        Expression.Call(null, stringType.GetMethod(nameof(string.Format), new Type[] { stringType, objectType, objectType }), new Expression[]
                        {
                            Expression.Constant($"{memberName} have configured the wrong type:{{0}}<-->{{1}};Please check the tree class or TreeOptions or {nameof(TreeDefault)}", typeof(string)),
                            Expression.Constant(memberType.Name),
                            Expression.Property(parameterRuntimeType, nameof(Type.Name))
                        })
                    })));

                //parameter2参数类型转换为parameter成员的的类型
                var parameter2TypeAs = Expression.TypeAs(parameter2, memberType);
                assignRight = parameter2TypeAs;
            }
            else
            {
                assignRight = parameter2;
            }
            //Expression assignmentExpression = Expression.Assign(memberExpression, parameter2);
            Expression assignmentExpression = Expression.Assign(memberExpression, assignRight);
            Expression setLambda = assignmentExpression;
            if (parameter2Check is { })
            {
                setLambda = Expression.Block(parameter2Check, assignmentExpression);
            }

            Expression<Action<TEntity, TValue>> setValueExpression = Expression.Lambda<Action<TEntity, TValue>>(setLambda, parameter, parameter2);
            var setValue = setValueExpression.Compile();

            Expression memberAccessExpression = Expression.PropertyOrField(parameter, memberName);
            //if (!valueType.IsAssignableFrom(memberType))//成员返回值和Getxxx返回值不匹配
            //{
            //    memberAccessExpression= Expression.TypeAs(memberAccessExpression, valueType);
            //}
            Expression<Func<TEntity, TValue>> getValueExpression = Expression.Lambda<Func<TEntity, TValue>>(memberAccessExpression, parameter);
            var getValue = getValueExpression.Compile();

            return (getValue, setValue, memberType);
        }


        internal static Func<ICollection<TEntity>> BuildCreateChildren<TEntity, TIdType>(this TreeOptions<TEntity, TIdType> options) where TEntity : ITree<TEntity, TIdType>, new()
        {
            if (options is null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (options.ChildrenType is null)
                throw new TreeException(TreeExceptionEnums.None, "ConfigureChildrenType configuration error , is null");

            if (options.ChildrenType.IsAbstract || options.ChildrenType.IsInterface)
                throw new TreeException(TreeExceptionEnums.None, "ConfigureChildrenType configuration error");

            Type intType = typeof(int);
            var childrenConstructor = options.ChildrenType.GetConstructor(new Type[1] { intType });
            Expression[] childrenConstructorParameters = default;

            if (childrenConstructor is null || !childrenConstructor.GetParameters()[0].Name.Contains("capacity", StringComparison.OrdinalIgnoreCase))
            {
                childrenConstructor = options.ChildrenType.GetConstructor(new Type[0]);
                if (childrenConstructor is null)//目前有泛型约束new,可以去掉此判断
                    throw new TreeException(TreeExceptionEnums.None, "ConfigureChildrenType configuration error ,ChildrenType No parameterless constructor");
            }
            else
            {
                childrenConstructorParameters = new Expression[1] { Expression.Constant(options.MinChildrenCapacity, intType) };
            }


            Expression<Func<ICollection<TEntity>>> createChildren = Expression.Lambda<Func<ICollection<TEntity>>>(Expression.New(childrenConstructor, childrenConstructorParameters));

            return createChildren.Compile();
        }
    }
}
