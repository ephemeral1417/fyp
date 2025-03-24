using System.ComponentModel.DataAnnotations;
using Google.Cloud.Firestore;

namespace fyp.Models
{

    [FirestoreData]
    public class UserProfile
    {
        [FirestoreProperty]
        public string Uid { get; set; } = string.Empty;

        [FirestoreProperty("Email")]
        public string Email { get; set; } = string.Empty;

        [FirestoreProperty("Name")]
        public string Username { get; set; } = string.Empty;

        [FirestoreProperty("Role")]
        public string Role { get; set; } = "worker";
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;
    }
}