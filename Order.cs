using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusBasicGradWork
{
    internal class Order
    {
        public string Id { get; }
        public int UserId { get; } = int.MinValue;
        public int VoteOrder {get; private set ;} = int.MinValue;
        public int VoteActual { get; } = int.MinValue;
        public double VoteA {  get; } = double.MinValue;
        public double VoteB { get; } = double.MinValue;

        public Order(int userId)
        {
            Id = DateTime.Now.ToShortDateString();
            UserId = userId;
        }
    }
}
