﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.MVVM.Model
{
    class UserModel
    {
        public string Username { get; set; }
        public string Uid { get; set; }
        public UserModel(string username, string uid)
        {
            Username = username;
            Uid = uid;
        }
    }
}
