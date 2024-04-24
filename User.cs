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
        public User(int balance, ChatMode chatMode)
        {
            Balance = balance;
            ChatMode = chatMode;
        }
    }
}