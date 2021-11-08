using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DataAccessLayer
{
    public interface IDataContext
    {
    }

    public interface IDataContext<TConnection> : IDataContext
    {
        TConnection Open();
    }
}
