using System;
using System.ComponentModel.DataAnnotations.Schema;
using ApplicationUser = AlgoDuck.Models.User.ApplicationUser;

namespace AlgoDuck.Models.Item
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