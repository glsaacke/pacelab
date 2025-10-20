using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace api.src.models
{
    public class UserRequest
    {
        public int StravaId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
    }
}