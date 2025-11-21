using System.ComponentModel.DataAnnotations;

namespace StartSmartDelivery.Core.DataLayer.Models
{
    // Introduced after reading: https://code-corner.dev/2023/11/04/Understanding-Flag-Enums-in-C/

    [Flags]
    public enum UserPermissions
    {
        None = 0,
        View = 1 << 0,
        Edit = 1 << 1,
        Delete = 1 << 2,
        Add = 1 << 3,
        Admin = 1 << 4
    }

    // Example usage:
    // UserPermissions allPermissions =
    // UserPermissions.View |
    // UserPermissions.Edit |
    // UserPermissions.Delete |
    // UserPermissions.Add;

    public class User
    {
        [Key]
        public required int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Username { get; set; }

        [Required]
        [MaxLength(100)]
        public required string Password { get; set; }

        public UserPermissions UserPermissions { get; set; } = UserPermissions.None;
    }
}
