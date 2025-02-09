using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace KontrolApp.Services
{
    [Table("users_admin")]
    public class UserAdmin : BaseModel
    {
        [PrimaryKey("id")]
        public int Id { get; set; }

        [Column("user_name")]
        public string Name { get; set; } = string.Empty;

        [Column("user_password")]
        public string Password { get; set; } = string.Empty;
    }
    internal class SupabaseService
    {
        private static readonly string SupabaseUrl = "https://ztfvlymtvlexfvltpewp.supabase.co";
        private static readonly string SupabaseKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp0ZnZseW10dmxleGZ2bHRwZXdwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkwNTY1ODUsImV4cCI6MjA1NDYzMjU4NX0.Ui8Q0QsB7tJEDZdMHYIfH5-xg-ADdQTT6Fnv0cEPBDE";
        private static Client? _supabaseClient;

        public SupabaseService()
        {
            if (_supabaseClient == null)
            {
                _supabaseClient = new Client(SupabaseUrl, SupabaseKey);
                _supabaseClient.InitializeAsync().Wait();
            }
        }

        public async Task<bool> ValidateUserAsync(string username, string password)
        {
            if (_supabaseClient == null)
                throw new Exception("Supabase istemcisi başlatılamadı!");

            var response = await _supabaseClient
                .From<UserAdmin>()
                .Filter("user_name", Supabase.Postgrest.Constants.Operator.Equals, username)
                .Filter("user_password", Supabase.Postgrest.Constants.Operator.Equals, password)
                .Get();

            return response.Models.Any();
        }
    }
}
