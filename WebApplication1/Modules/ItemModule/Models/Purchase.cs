using System;
using System.ComponentModel.DataAnnotations.Schema;
using UserNamescape = WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.ItemModule.Models
{
    public class Purchase
    {
        [ForeignKey("Item")]
        public Guid ItemId { get; set; }
        public required Item Item { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required UserNamescape.ApplicationUser User { get; set; }

        public bool Selected { get; set; }
    }
}