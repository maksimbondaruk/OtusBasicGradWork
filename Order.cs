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
            ToDelete = 9
        }
        public long Id { get; internal set; }
        private string _name;
        public string Name 
        {   
            get => _name; 
            set
            {
                if (State == OrdState.Initial)
                _name = value;
            } 
        }
        public long UserId { get; } = 0;
        public int VoteOrder {get; internal set ;} = 0;
        public int VoteActual { get; } = 0;
        public double VoteA {  get; } = 0.0;
        public double VoteB { get; } = 0.0;
        public OrdState State { get; set; }

        public Order(long userId)
        {
            //Id = long.Parse(DateTime.UtcNow.Month.ToString() + DateTime.UtcNow.Day.ToString() 
            //                + DateTime.UtcNow.Hour.ToString() + DateTime.UtcNow.Minute.ToString()); //12312359
            UserId = userId;
            State = 0; //OrdState.Initial
        }

        public void MakeChoiseA (double voteRate)
        {

        }
    }
}
