using fyp.Models;
using FirebaseAdmin.Auth;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace fyp.Services
{
    public class FirebaseAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly FirestoreDb _firestoreDb;

        public FirebaseAuthService(IConfiguration configuration, FirestoreDb firestoreDb)
        {
            _httpClient = new HttpClient();
            _firestoreDb = firestoreDb;
            _apiKey = configuration["Firebase:ApiKey"];
        }

        // Auto-generate a random password (8 characters)
        public string GenerateRandomPassword()
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()";
            var random = new Random();
            var chars = new char[8];

            for (int i = 0; i < 8; i++)
            {
                chars[i] = validChars[random.Next(validChars.Length)];
            }

            return new string(chars);
        }

        // Register a new user (admin only)
        public async Task<(string uid, string password)> RegisterUserAsync(RegisterViewModel model)
        {
            // Generate a random password
            string password = GenerateRandomPassword();

            // Create user in Firebase Authentication
            var data = new
            {
                email = model.Email,
                password = password,
                returnSecureToken = true
            };

            var response = await _httpClient.PostAsJsonAsync(
                $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}",
                data);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                var uid = result.GetProperty("localId").GetString();

                // Create user profile
                var userProfile = new UserProfile
                {
                    Uid = uid,
                    Email = model.Email,
                    Username = model.Username,
                    Role = "worker" // Preset role to worker
                };

                // Store in Firestore
                await AddUserToFirestoreAsync(userProfile);

                // Return the UID and generated password
                return (uid, password);
            }

            var error = await response.Content.ReadFromJsonAsync<JsonElement>();
            var message = error.GetProperty("error").GetProperty("message").GetString();
            throw new Exception($"Firebase registration error: {message}");
        }

        public async Task<UserProfile> LoginAsync(LoginViewModel model)
        {
            try
            {
                Console.WriteLine($"Attempting login for: {model.Email}");

                var data = new
                {
                    email = model.Email,
                    password = model.Password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}",
                    data);

                Console.WriteLine($"Firebase login response status: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Response content: {responseContent}");

                    var result = JsonDocument.Parse(responseContent).RootElement;

                    if (result.TryGetProperty("localId", out var localIdElement))
                    {
                        var uid = localIdElement.GetString();
                        Console.WriteLine($"User authenticated with UID: {uid}");

                        // Look up by authentication UID instead of hardcoded "US1001"
                        var userProfile = await GetUserProfileAsync(uid);

                        if (userProfile == null)
                        {
                            Console.WriteLine("User profile not found in Firestore");
                            throw new Exception("User profile not found");
                        }

                        Console.WriteLine($"User found with role: {userProfile.Role}");
                        return userProfile;
                    }
                    else
                    {
                        Console.WriteLine("localId property not found in response");
                        throw new Exception("User ID not found in authentication response");
                    }
                }
                else
                {
                    // Error handling remains the same
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error response from Firebase: {errorContent}");

                    try
                    {
                        var error = JsonDocument.Parse(errorContent).RootElement;
                        if (error.TryGetProperty("error", out var errorElement) &&
                            errorElement.TryGetProperty("message", out var messageElement))
                        {
                            throw new Exception($"Firebase login error: {messageElement.GetString()}");
                        }
                        else
                        {
                            throw new Exception($"Firebase login error: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Firebase login error: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during login: {ex.Message}");
                throw;
            }
        }

        // Updated to look in both 'users' and 'Users' collections
        public async Task<UserProfile> GetUserProfileAsync(string uid)
        {
            try
            {
                if (string.IsNullOrEmpty(uid))
                {
                    Console.WriteLine("Error: UID is null or empty");
                    return null;
                }

                Console.WriteLine($"Getting user profile for UID: {uid}");

                // Try lowercase 'users' collection first
                var userDoc = await _firestoreDb.Collection("users").Document(uid).GetSnapshotAsync();

                if (userDoc.Exists)
                {
                    Console.WriteLine("User profile found in lowercase 'users' collection");
                    try
                    {
                        var profile = userDoc.ConvertTo<UserProfile>();
                        // Ensure no properties are null
                        profile.Email = profile.Email ?? "";
                        profile.Username = profile.Username ?? "";
                        profile.Role = profile.Role ?? "worker";
                        return profile;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting document: {ex.Message}");
                        // Manual conversion as fallback
                        var data = userDoc.ToDictionary();
                        return new UserProfile
                        {
                            Uid = uid,
                            Email = data.ContainsKey("Email") ? data["Email"]?.ToString() ?? "" : "",
                            Username = data.ContainsKey("Name") ? data["Name"]?.ToString() ?? "" : "",
                            Role = data.ContainsKey("Role") ? data["Role"]?.ToString() ?? "worker" : "worker"
                        };
                    }
                }

                // Try uppercase 'Users' collection
                userDoc = await _firestoreDb.Collection("Users").Document(uid).GetSnapshotAsync();

                if (userDoc.Exists)
                {
                    Console.WriteLine("User profile found in uppercase 'Users' collection");
                    try
                    {
                        var profile = userDoc.ConvertTo<UserProfile>();
                        // Ensure no properties are null
                        profile.Email = profile.Email ?? "";
                        profile.Username = profile.Username ?? "";
                        profile.Role = profile.Role ?? "worker";
                        return profile;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error converting document: {ex.Message}");
                        // Manual conversion as fallback
                        var data = userDoc.ToDictionary();
                        return new UserProfile
                        {
                            Uid = uid,
                            Email = data.ContainsKey("Email") ? data["Email"]?.ToString() ?? "" : "",
                            Username = data.ContainsKey("Name") ? data["Name"]?.ToString() ?? "" : "",
                            Role = data.ContainsKey("Role") ? data["Role"]?.ToString() ?? "worker" : "worker"
                        };
                    }
                }

                Console.WriteLine("User profile not found in any collection");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user profile: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return null;
            }
        }

        public async Task AddUserToFirestoreAsync(UserProfile user)
        {
            var usersCollection = _firestoreDb.Collection("users");
            await usersCollection.Document(user.Uid).SetAsync(user);
        }

        public async Task<List<UserProfile>> GetAllUsersAsync()
        {
            var usersSnapshot = await _firestoreDb.Collection("users").GetSnapshotAsync();
            return usersSnapshot.Documents.Select(d => d.ConvertTo<UserProfile>()).ToList();
        }

        public async Task UpdateUserProfileAsync(UserProfile user)
        {
            var userRef = _firestoreDb.Collection("users").Document(user.Uid);
            await userRef.SetAsync(user, SetOptions.MergeAll);
        }
    }
}