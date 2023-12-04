using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace Inventory.Models
{
    public class UserModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string LastName { get; set; }

        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 4)]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [StringLength(50, MinimumLength = 5)]
        public string Password { get; set; }
    }
}
