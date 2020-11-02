using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UsersSubscriptions.Models.DataTransfer
{
    public class EntityCreateResult : IdentityResult
    {
        public EntityCreateResult(string id)
        {
            Succeeded = true;
            EntityId = id;
        }
        public string EntityId { get; set; }
    }
}
