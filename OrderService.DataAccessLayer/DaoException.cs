using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DataAccessLayer
{

    [Serializable]
    public class DaoException : Exception
    {
        public DaoException() { }
        public DaoException(string message) : base(message) { }
        public DaoException(string message, Exception inner) : base(message, inner) { }
        protected DaoException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
