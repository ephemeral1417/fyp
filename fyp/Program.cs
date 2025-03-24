using fyp.Services;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Credentials path
var credentialPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "firebase-private-key.json");
Console.WriteLine($"Looking for credentials at: {credentialPath}");

if (!File.Exists(credentialPath))
{
    Console.WriteLine("ERROR: Credentials file not found!");
    return; // Stop the application if credentials aren't found
}

// Set environment variable for Google credentials
Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", credentialPath);

// Initialize Firebase Admin SDK
try
{
    if (FirebaseApp.DefaultInstance == null)
    {
        FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(credentialPath),
            ProjectId = "inventoryfyp-6d9a0"
        });
        Console.WriteLine("Firebase initialized successfully");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Firebase initialization error: {ex.Message}");
    return; // Stop the application if Firebase initialization fails
}

// Add Firebase API key to configuration
builder.Configuration["Firebase:ApiKey"] = "AIzaSyB18pzPJZgonnb28oHW7wQQbOQwGJeky4k";

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Register FirestoreDb as a singleton with explicit credential
// Register FirestoreDb as a singleton with explicit credential
try
{
    var credential = GoogleCredential.FromFile(credentialPath);
    var firestoreDb = FirestoreDb.Create("inventoryfyp-6d9a0");

    builder.Services.AddSingleton(firestoreDb);
    Console.WriteLine("Firestore initialized successfully");
}
catch (Exception ex)
{
    Console.WriteLine($"Firestore initialization error: {ex.Message}");
    return; // Stop the application if Firestore initialization fails
}

// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddDistributedMemoryCache();

// Add custom services
builder.Services.AddSingleton<FirebaseAuthService>();
builder.Services.AddSingleton<FirestoreService>();
builder.Services.AddHttpContextAccessor(); // Add this line

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Enable session middleware
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();