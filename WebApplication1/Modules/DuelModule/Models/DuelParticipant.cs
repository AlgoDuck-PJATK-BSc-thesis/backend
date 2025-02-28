using System.ComponentModel.DataAnnotations.Schema;
using WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.DuelModule.Models
{
    public class DuelParticipant
    {
        [ForeignKey("Duel")]
        public Guid DuelId { get; set; }
        public Duel Duel { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User User { get; set; }
    }
}