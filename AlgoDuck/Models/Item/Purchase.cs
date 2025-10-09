using System;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Modules.User.Models.ApplicationUser;
using UserNamescape = AlgoDuck.Modules.User.Models;

namespace AlgoDuck.Modules.Item.Models
{
    public class Purchase
    {
        [ForeignKey("Item")]
        public Guid ItemId { get; set; }
        public required Item Item { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required ApplicationUser User { get; set; }

        public bool Selected { get; set; }
    }
}