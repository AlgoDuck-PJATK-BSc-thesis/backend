using System.ComponentModel.DataAnnotations.Schema;
using UserNamespace = WebApplication1.Modules.UserModule.Models;

namespace WebApplication1.Modules.DuelModule.Models
{
    public class DuelParticipant
    {
        [ForeignKey("Duel")]
        public Guid DuelId { get; set; }
        public required Duel Duel { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required UserNamespace.User User { get; set; }
    }
}