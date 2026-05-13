using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;

namespace CaritasCharityPortal.Services
{
    public class FirebaseAuthService
    {
        public FirebaseAuthService()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
#pragma warning disable CS0618
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("caritascharity-firebase-adminsdk.json")
                });
#pragma warning restore CS0618
            }
        }

        public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
        {
            return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }

        public async Task<UserRecord> GetUserByEmailAsync(string email)
        {
            return await FirebaseAuth.DefaultInstance.GetUserByEmailAsync(email);
        }

        public async Task<UserRecord> CreateUserAsync(string email, string password, string displayName)
        {
            var args = new UserRecordArgs
            {
                Email = email,
                Password = password,
                DisplayName = displayName,
                EmailVerified = false
            };
            return await FirebaseAuth.DefaultInstance.CreateUserAsync(args);
        }
    }
}