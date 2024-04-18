using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusBasicGradWork
{
    internal class Order
    {
        public enum OrdState
        {
            Initial = 1,
            Named = 2,
            LoadedA = 3,
            LoadedB = 4,
            BalanceLo = 5,
            BalanceOk = 6,
            Running = 7,
            Paused = 8,
            Deleted = 9
        }
        public long Id { get; }
        public long UserId { get; } = int.MinValue;
        public int VoteOrder {get; private set ;} = int.MinValue;
        public int VoteActual { get; } = int.MinValue;
        public double VoteA {  get; } = double.MinValue;
        public double VoteB { get; } = double.MinValue;
        public OrdState State { get; set; }

        public Order(long userId)
        {
            Id = long.Parse(DateTime.UtcNow.Month.ToString() + DateTime.UtcNow.Day.ToString() 
                            + DateTime.UtcNow.Hour.ToString() + DateTime.UtcNow.Minute.ToString()); //12312359
            UserId = userId;
            State = OrdState.Initial;
        }

        public void MakeChoiseA (double voteRate)
        {

        }
    }
}
