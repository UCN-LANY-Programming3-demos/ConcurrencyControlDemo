using Dapper;
using OrderService.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Transactions;

namespace OrderService.DataAccessLayer.Daos
{
    class SqlServerOrderDao : BaseDao<IDbConnection>, IDao<Order>
    {
        public SqlServerOrderDao(IDataContext connection) : base(connection as IDataContext<IDbConnection>)
        {
        }

        public Order Create(Order model)
        {
            return CreateWithNoTransaction(model);
        }

        // creates an order without transaction
        private Order CreateWithNoTransaction(Order model)
        {
            try
            {
                using IDbConnection connection = DataContext.Open();

                // inserting order
                string insertOrderSql = "INSERT INTO Orders (CustomerName) VALUES (@CustomerName); SELECT SCOPE_IDENTITY();";
                int orderId = connection.ExecuteScalar<int>(insertOrderSql, model);

                // inserting orderlines
                string insertOrderlineSql = "INSERT INTO Orderlines (OrderId, Product, UnitPrice, Quantity) VALUES (@OrderId, @Product, @UnitPrice, @Quantity); SELECT SCOPE_IDENTITY();";
                foreach (Orderline orderline in model.Orderlines)
                {
                    orderline.Id = connection.ExecuteScalar<int>(insertOrderlineSql
                        , new
                        {
                            OrderId = orderId,
                            orderline.Product,
                            orderline.UnitPrice,
                            orderline.Quantity
                        });
                }
                model.Id = orderId;
            }
            catch (Exception)
            {
                // just let the exceptions disappear in cyberspace...
            }
            return model;
        }

        // creates an order using an implicit transaction
        private Order CreateWithImplicitTransaction(Order model)
        {
            using TransactionScope scope = new();
            try
            {
                using IDbConnection connection = DataContext.Open();

                // inserting order
                string insertOrderSql = "INSERT INTO Orders (CustomerName) VALUES (@CustomerName); SELECT SCOPE_IDENTITY();";
                int orderId = connection.ExecuteScalar<int>(insertOrderSql, model);

                // inserting orderlines
                string insertOrderlineSql = "INSERT INTO Orderlines (OrderId, Product, UnitPrice, Quantity) VALUES (@OrderId, @Product, @UnitPrice, @Quantity); SELECT SCOPE_IDENTITY();";
                foreach (Orderline orderline in model.Orderlines)
                {
                    orderline.Id = connection.ExecuteScalar<int>(insertOrderlineSql
                        , new
                        {
                            OrderId = orderId,
                            orderline.Product,
                            orderline.UnitPrice,
                            orderline.Quantity
                        });
                }
                model.Id = orderId;
                scope.Complete();
            }
            catch (Exception)
            {
                // just let the exceptions disappear in cyberspace...
            }
            return model;
        }

        public bool Delete(Order model)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Order> Read(Func<Order, bool> predicate = null)
        {
            string selectOrdersSql = "SELECT * FROM Orders";

            using IDbConnection connection = DataContext.Open();
            IEnumerable<Order> orders;

            if (predicate == null)
            {
                orders = connection.Query<Order>(selectOrdersSql);
            }
            else
            {
                orders = connection.Query<Order>(selectOrdersSql).Where(predicate);
            }

            string selectOrderLinesSql = "SELECT * FROM Orderlines WHERE OrderId = @OrderId";

            foreach (Order order in orders)
            {
                order.Orderlines = connection.Query<Orderline>(selectOrderLinesSql, new { OrderId = order.Id }).ToList();
            }
            return orders;
        }

        public bool Update(Order model)
        {
            return UpdateWithNoConcurrencyControl(model);
        }

        // updating an order without any concurrency control
        private bool UpdateWithNoConcurrencyControl(Order model)
        {
            try
            {
                using IDbConnection connection = DataContext.Open();

                Order oldOrder = Read(o => o.Id == model.Id).Single();

                string updateOrderSql = "UPDATE Orders SET CustomerName = @CustomerName WHERE Id = @Id";
                string updateOrderlineSql = "UPDATE Orderlines SET OrderId = @OrderId, Product = @Product, UnitPrice = @UnitPrice, Quantity = @Quantity  WHERE Id = @Id";
                string insertOrderlineSql = "INSERT INTO Orderlines (OrderId, Product, UnitPrice, Quantity) VALUES (@OrderId, @Product, @UnitPrice, @Quantity); SELECT SCOPE_IDENTITY();";
                string deleteOrderlineSql = "DELETE FROM Orderlines WHERE Id = @Id";
                // updating order
                int rowsAffected = connection.Execute(updateOrderSql, model);

                if (rowsAffected != 1)
                    return false;

                // deleting removed orderlines
                IEnumerable<Orderline> orderLinesToDelete = oldOrder.Orderlines.Where(oldOl => !model.Orderlines.Any(newOl => newOl.Id == oldOl.Id));
                foreach (Orderline orderline in orderLinesToDelete)
                {
                    rowsAffected = connection.Execute(deleteOrderlineSql, orderline);
                    if (rowsAffected != 1)
                        return false;
                }

                // updating orderlines
                foreach (Orderline orderline in model.Orderlines)
                {
                    if (orderline.Id.HasValue)
                    {
                        // update existing
                        rowsAffected = connection.Execute(updateOrderlineSql, new
                        {
                            orderline.Id,
                            OrderId = model.Id,
                            orderline.Product,
                            orderline.UnitPrice,
                            orderline.Quantity
                        });
                    }
                    else
                    {
                        // adding new
                        rowsAffected = connection.Execute(insertOrderlineSql, new
                        {
                            OrderId = model.Id,
                            orderline.Product,
                            orderline.UnitPrice,
                            orderline.Quantity
                        });
                    }
                    if (rowsAffected != 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // dont let the exceptions disappear in cyberspace...
                throw new DaoException($"An error ocurred while updating {model}", ex);
            }
        }

        // updating an order using optimistic concurrency control
        private bool UpdateWithOptimisticConcurrencyControl(Order model)
        {
            try
            {
                using IDbConnection connection = DataContext.Open();

                Order oldOrder = Read(o => o.Id == model.Id).Single();

                string updateOrderSql = "UPDATE Orders SET CustomerName = @CustomerName WHERE Id = @Id AND Version = @Version";
                string updateOrderlineSql = "UPDATE Orderlines SET OrderId = @OrderId, Product = @Product, UnitPrice = @UnitPrice, Quantity = @Quantity  WHERE Id = @Id";
                string insertOrderlineSql = "INSERT INTO Orderlines (OrderId, Product, UnitPrice, Quantity) VALUES (@OrderId, @Product, @UnitPrice, @Quantity); SELECT SCOPE_IDENTITY();";
                string deleteOrderlineSql = "DELETE FROM Orderlines WHERE Id = @Id";
                // updating order
                int rowsAffected = connection.Execute(updateOrderSql, model);

                if (rowsAffected != 1)
                    return false;

                // deleting removed orderlines
                IEnumerable<Orderline> orderLinesToDelete = oldOrder.Orderlines.Where(oldOl => !model.Orderlines.Any(newOl => newOl.Id == oldOl.Id));
                foreach (Orderline orderline in orderLinesToDelete)
                {
                    rowsAffected = connection.Execute(deleteOrderlineSql, orderline);
                    if (rowsAffected != 1)
                        return false;
                }

                // updating orderlines
                foreach (Orderline orderline in model.Orderlines)
                {
                    if (orderline.Id.HasValue)
                    {
                        // update existing
                        rowsAffected = connection.Execute(updateOrderlineSql, new
                        {
                            orderline.Id,
                            OrderId = model.Id,
                            orderline.Product,
                            orderline.UnitPrice,
                            orderline.Quantity
                        });
                    }
                    else
                    {
                        // adding new
                        rowsAffected = connection.Execute(insertOrderlineSql, new
                        {
                            OrderId = model.Id,
                            orderline.Product,
                            orderline.UnitPrice,
                            orderline.Quantity
                        });
                    }
                    if (rowsAffected != 1)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                // dont let the exceptions disappear in cyberspace...
                throw new DaoException($"An error ocurred while updating {model}", ex);
            }
        }

        // updating an order using pessimistic concurrency control with an explicit transaction
        // NOTE! using the explicit transaction this just for demo purpose, it is much easier to use an implicit transaction
        private bool UpdateWithPessimisticConcurrencyControl(Order model)
        {
            using IDbConnection connection = DataContext.Open();
            using IDbTransaction transaction = connection.BeginTransaction();
            try
            {
                Order oldOrder = Read(o => o.Id == model.Id).Single();

                string updateOrderSql = "UPDATE Orders SET CustomerName = @CustomerName WHERE Id = @Id";
                string updateOrderlineSql = "UPDATE Orderlines SET OrderId = @OrderId, Product = @Product, UnitPrice = @UnitPrice, Quantity = @Quantity  WHERE Id = @Id";
                string insertOrderlineSql = "INSERT INTO Orderlines (OrderId, Product, UnitPrice, Quantity) VALUES (@OrderId, @Product, @UnitPrice, @Quantity); SELECT SCOPE_IDENTITY();";
                string deleteOrderlineSql = "DELETE FROM Orderlines WHERE Id = @Id";

                // updating order
                int rowsAffected = connection.Execute(updateOrderSql, model);

                if (rowsAffected != 1)
                {
                    transaction.Rollback();
                    return false;
                }

                // deleting removed orderlines
                IEnumerable<Orderline> orderLinesToDelete = oldOrder.Orderlines.Where(oldOl => !model.Orderlines.Any(newOl => newOl.Id == oldOl.Id));
                foreach (Orderline orderline in orderLinesToDelete)
                {
                    rowsAffected = connection.Execute(deleteOrderlineSql, orderline);
                    if (rowsAffected != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                // updating orderlines
                foreach (Orderline orderline in model.Orderlines.Where(ol => ol.Id.HasValue))
                {
                    // update existing
                    rowsAffected = connection.Execute(updateOrderlineSql, new
                    {
                        orderline.Id,
                        OrderId = model.Id,
                        orderline.Product,
                        orderline.UnitPrice,
                        orderline.Quantity
                    });
                    if (rowsAffected != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                foreach (Orderline orderline in model.Orderlines.Where(ol => !ol.Id.HasValue))
                {
                    // adding new orderlines
                    rowsAffected = connection.Execute(insertOrderlineSql, new
                    {
                        OrderId = model.Id,
                        orderline.Product,
                        orderline.UnitPrice,
                        orderline.Quantity
                    });
                    if (rowsAffected != 1)
                    {
                        transaction.Rollback();
                        return false;
                    }
                }

                transaction.Commit(); // Committing transaction
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback(); // Rolling back transaction
                // dont let the exceptions disappear in cyberspace...
                throw new DaoException($"An error ocurred while updating {model}", ex);
            }
        }
    }
}
