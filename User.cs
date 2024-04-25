using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OtusBasicGradWork
{
    internal class User
    {
        public int Balance { get; internal set; }
        public ChatMode ChatMode { get; internal set; }
        public bool RequestToChangeVoteOrder { get; internal set; } = false;
        public double BalToVoteKoef { get; internal set; } = 1.0;
        public User(int balance, ChatMode chatMode)
        {
            Balance = balance;
            ChatMode = chatMode;
        }
    }
}