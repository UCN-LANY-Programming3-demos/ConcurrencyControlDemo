using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DataAccessLayer.Daos
{
    abstract class BaseDao<TConnection> 
    {

        public BaseDao(IDataContext<TConnection> dataConext)
        {
            DataContext = dataConext ?? throw new ArgumentNullException(nameof(dataConext));
        }

        public IDataContext<TConnection> DataContext { get; }
    }
}
