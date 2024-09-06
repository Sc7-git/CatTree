using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CatTree
{
    public class TreeDefault
    {
        public static int MinChildrenCapacity = 10;
        //public static bool ParentOptional = true;

        private const string _defaultIdMemberName = "Id";
        private const string _defaultParentIdMemberName = "ParentId";
        private const string _defaultChildrenMemberName = "Children";
        private const string _defaultParentMemberName = "Parent";
        private static string _idMemberName = _defaultIdMemberName;
        private static string _parentIdMemberName = _defaultParentIdMemberName;
        private static string _childrenMemberName = _defaultChildrenMemberName;
        private static string _parentMemberName = _defaultParentMemberName;


        /// <summary>
        /// 必填
        /// </summary>
        public static string IdMemberName
        {
            get => _idMemberName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    CheckMemberName(nameof(IdMemberName), value);
                    _idMemberName = value;
                }
            }
        }

        /// <summary>
        /// 必填
        /// </summary>
        public static string ParentIdMemberName
        {
            get => _parentIdMemberName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    CheckMemberName(nameof(ParentIdMemberName), value);
                    _parentIdMemberName = value;
                }
            }
        }

        /// <summary>
        /// 必填
        /// </summary>
        public static string ChildrenMemberName
        {
            get => _childrenMemberName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    CheckMemberName(nameof(ChildrenMemberName), value);
                    _childrenMemberName = value;
                }
            }
        }

        /// <summary>
        /// 可以为Null
        /// </summary>
        public static string ParentMemberName
        {
            get => _parentMemberName;
            set
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    CheckMemberName(nameof(ParentMemberName), value);
                }
                //可以为Null
                _parentMemberName = value;
            }
        }

        internal static void CheckMemberName(string @nameof, string memberName)
        {
            if (!Regex.IsMatch(memberName, "^[a-zA-Z_][a-zA-Z0-9_]+$"))
                throw new TreeException(TreeExceptionEnums.None, $"The {@nameof} '{memberName}' is illegal");
        }


    }

}
