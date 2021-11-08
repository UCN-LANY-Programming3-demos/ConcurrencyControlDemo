using OrderService.DataAccessLayer.Daos;
using OrderService.Model;
using System;

namespace OrderService.DataAccessLayer
{
    public static class DaoFactory
    {
        public static IDao<TModel> Create<TModel>(IDataContext dataContext)
        {
            return typeof(TModel) switch
            {
                var dao when dao == typeof(Order) => new SqlServerOrderDao(dataContext) as IDao<TModel>,
                _ => throw new DaoException($"Unknown model: {typeof(TModel).FullName}")
            };
        }
    }
}
