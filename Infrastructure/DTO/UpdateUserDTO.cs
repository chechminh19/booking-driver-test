﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DTO
{
    public class UpdateUserDTO
    {
        public long Id { get; set; }
        public string? UserName { get; set;}
        public string? Email {  get; set;}
        public string? Password { get; set;}
    }
}
