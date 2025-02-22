using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace KontrolApp.Services
{
    public class SupabaseService
    {
        private readonly string _supabaseUrl = "https://ztfvlymtvlexfvltpewp.supabase.co";
        private readonly string _supabaseAnonKey = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6Inp0ZnZseW10dmxleGZ2bHRwZXdwIiwicm9sZSI6ImFub24iLCJpYXQiOjE3MzkwNTY1ODUsImV4cCI6MjA1NDYzMjU4NX0.Ui8Q0QsB7tJEDZdMHYIfH5-xg-ADdQTT6Fnv0cEPBDE";
        private readonly HttpClient _httpClient;

        public SupabaseService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _supabaseAnonKey);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _httpClient.DefaultRequestHeaders.Add("apikey", _supabaseAnonKey);
        }

        // Genel amaçlı dinamik veri çekme metodu
        public async Task<List<Dictionary<string, object>>> GetDataAsync(string tableName, string idColumn, string[] columns)
        {
            try
            {
                // Verileri ID'ye göre sıralı şekilde çekmek için `order={idColumn}.asc` ekledik
                string url = $"{_supabaseUrl}/rest/v1/{tableName}?select={string.Join(",", columns)}&order={idColumn}.asc";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Veri çekme hatası: {response.ReasonPhrase}");
                    return new List<Dictionary<string, object>>();
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var rawData = JsonSerializer.Deserialize<List<Dictionary<string, JsonElement>>>(jsonResponse);

                List<Dictionary<string, object>> dataList = new List<Dictionary<string, object>>();

                foreach (var item in rawData)
                {
                    var dict = new Dictionary<string, object>();

                    foreach (var column in columns)
                    {
                        if (item.ContainsKey(column))
                        {
                            var value = item[column];

                            if (value.ValueKind == JsonValueKind.Number)
                                dict[column] = value.GetInt32();
                            else if (value.ValueKind == JsonValueKind.String)
                                dict[column] = value.GetString();
                            else if (value.ValueKind == JsonValueKind.Null)
                                dict[column] = null;
                            else
                                dict[column] = value.ToString();
                        }
                    }
                    dataList.Add(dict);
                }

                return dataList;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Veri çekerken hata oluştu: {ex.Message}");
                return new List<Dictionary<string, object>>();
            }
        }

        // Genel amaçlı dinamik güncelleme metodu
        public async Task<bool> UpdateStatusAsync(string tableName, string idColumn, int id, string statusColumn, string newStatus)
        {
            try
            {
                string url = $"{_supabaseUrl}/rest/v1/{tableName}?{idColumn}=eq.{id}";

                var updateData = new Dictionary<string, object>
                {
                    { statusColumn, newStatus }
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(updateData), Encoding.UTF8, "application/json");
                var response = await _httpClient.PatchAsync(url, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    return true;
                }

                Debug.WriteLine($"Durum güncelleme hatası: {response.ReasonPhrase}");
                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Durum güncellenirken hata oluştu: {ex.Message}");
                return false;
            }
        }
    }
}
