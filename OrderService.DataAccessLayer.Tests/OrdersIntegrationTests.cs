using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrderService.Database;
using OrderService.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Transactions;

namespace OrderService.DataAccessLayer.Tests
{
    [TestClass]
    public class OrdersIntegrationTests
    {
        private readonly string _connectionString = @$"Data Source=(localdb)\mssqllocaldb; Initial Catalog=OrderService_OrdersIntegrationTests_{DateTime.Now.Ticks}; Integrated Security=true";
        private IDao<Order> _orderDao;
        private IDataContext _dataContext;

        #region Scaffolding

        [TestInitialize]
        public void Init()
        {
            _dataContext = new SqlServerDataContext(_connectionString);

            DataBaseVersion.Upgrade(_connectionString);

            _orderDao = DaoFactory.Create<Order>(_dataContext);
        }

        [TestCleanup]
        public void Cleanup()
        {
            //DataBaseVersion.Drop(_connectionString);
        }
        #endregion

        [TestMethod]
        public void ShouldCreateOrderWithOrderLines()
        {
            Order order = new()
            {
                CustomerName = "Rick Sanchez",
                Orderlines = new List<Orderline>()
                {
                    new Orderline{ Product = "Americano", Quantity = 3, UnitPrice = 19.99 },
                    new Orderline{ Product = "Cappuccino", Quantity = 2, UnitPrice = 22.99 }
                }
            };
            Order createdOrder = _orderDao.Create(order);

            Assert.IsTrue(createdOrder.Id.HasValue);
            Assert.IsTrue(createdOrder.Orderlines.Count == 2);
            Assert.IsTrue(createdOrder.Orderlines[0].Id.HasValue);
            Assert.IsTrue(createdOrder.Orderlines[1].Id.HasValue);
            Assert.IsTrue(createdOrder.Orderlines.Sum(ol => ol.Quantity) == 5);
        }

        [TestMethod]
        public void ShouldNotCreateOrderDueToMissingProductName()
        {
            Order order = new()
            {
                CustomerName = "Reverse Giraffe",
                Orderlines = new List<Orderline>()
                {
                    new Orderline{ Product = null, Quantity = 2, UnitPrice = 22.99 },
                    new Orderline{ Product = "Cappuccino", Quantity = 3, UnitPrice = 19.99 },
                }
            };
            Order createdOrder = _orderDao.Create(order);

            Assert.IsFalse(createdOrder.Id.HasValue);
        }

        [TestMethod]
        public void ShouldUpdateOrderAndDeleteOrderline()
        {
            Order order1 = _orderDao.Read().First();

            order1.Orderlines.RemoveAt(0);

            //Thread.Sleep(20000);

            bool test = _orderDao.Update(order1);

            Assert.IsTrue(test);

            Order order2 = _orderDao.Read(o => o.Id == order1.Id).Single();

            Assert.IsTrue(order2.Orderlines.Count == 1);
        }

        [TestMethod]
        public void ShouldUpdateOrderAndAddOrderline()
        {
            TransactionOptions options = new()
            {
                IsolationLevel = IsolationLevel.Serializable
            };
            using TransactionScope scope = new(TransactionScopeOption.Required, options);

            Order order1 = _orderDao.Read(o => o.Id == 1).Single();

            order1.Orderlines.Add(new Orderline { Product = "Americano", UnitPrice = 19.99, Quantity = 2 });

            bool test = _orderDao.Update(order1);

            scope.Complete();

            Assert.IsTrue(test);

            Order order2 = _orderDao.Read(o => o.Id == order1.Id).Single();

            Assert.IsTrue(order2.Orderlines.Count == 3);
        }

        [TestMethod]
        public void ShouldNotUpdateOrderWhenVersionConflicts()
        {
            Order order1 = _orderDao.Read(o => o.Id == 1).Single();
            Order order2 = _orderDao.Read(o => o.Id == 1).Single();

            order2.CustomerName = "UpdatedFirst";

            bool test1 = _orderDao.Update(order2);

            Assert.IsTrue(test1);

            order1.CustomerName = "UpdatedSecond";

            bool test2 = _orderDao.Update(order1);

            Assert.IsFalse(test2);
        }
    }
}
