using System;
using System.Collections.Generic;

namespace OrderService.Model
{
    public class Order
    {
        public int? Id { get; set; }
        public string CustomerName { get; set; }
        public IList<Orderline> Orderlines { get; set; }
        public byte[] Version { get; set; }
    }
}
