using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CatTree
{
    public class TreeException : Exception
    {

        public TreeException(TreeExceptionEnums state)
        {

        }

        public TreeException(TreeExceptionEnums state, string? msg) : base(msg)
        {

        }

        public TreeException(TreeExceptionEnums state, string? msg, Exception? innerException) : base(msg, innerException)
        {

        }

    }

    public enum TreeExceptionEnums
    {
        None = 0,

    }
}
