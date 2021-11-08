using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.DataAccessLayer
{
    public interface IDao<TModel>
    {
        TModel Create(TModel model);
        IEnumerable<TModel> Read(Func<TModel, bool> predicate = null);
        bool Update(TModel model);
        bool Delete(TModel model);
    }
}
