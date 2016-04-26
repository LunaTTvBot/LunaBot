using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IRCConnectionTest
{
    internal class User
    {
        [Key]
        public string Username { get; set; }
    }
}
