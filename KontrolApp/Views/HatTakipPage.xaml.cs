using System.Collections.ObjectModel;
using System.Text;
using System;
using Supabase;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using KontrolApp.Services;

namespace KontrolApp;

public partial class HatTakipPage : ContentPage
{
    private readonly SupabaseService _supabaseService;
    public HatTakipPage()
	{
        InitializeComponent();
        _supabaseService = new SupabaseService();
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UserEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Hata", "Kullanýcý adý ve þifre boþ olamaz!", "Tamam");
            return;
        }

        bool isValid = await _supabaseService.ValidateUserAsync(username, password);

        if (isValid)
        {
            await DisplayAlert("Baþarýlý", "Giriþ baþarýlý!", "Tamam");
            await Navigation.PushAsync(new MainPage());
        }
        else
        {
            await DisplayAlert("Hata", "Geçersiz kullanýcý adý veya þifre!", "Tamam");
        }
    }
}