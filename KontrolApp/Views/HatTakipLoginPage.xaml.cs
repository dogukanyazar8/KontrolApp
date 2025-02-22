using System.Collections.ObjectModel;
using System.Text;
using System;
using Supabase;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using KontrolApp.Services;
using System.Net.Http.Headers;
using System.Text.Json;
using Supabase.Gotrue;
using System.Text.Json.Serialization;

namespace KontrolApp;

public partial class HatTakipLoginPage : ContentPage
{
    private SupabaseAuthService _authService = new SupabaseAuthService();

    private const string TableName = "users_hatTakip";
    private const string UsernameColumn = "user_name";
    private const string PasswordColumn = "user_password";
    public HatTakipLoginPage()
	{
        InitializeComponent();
    }
    private async void OnLoginClicked(object sender, EventArgs e)
    {
        string username = UsernameEntry.Text;
        string password = PasswordEntry.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            LoginErrorLabel.Text = "Kullanýcý adý ve þifre boþ olamaz.";
            LoginErrorLabel.IsVisible = true;
            return;
        }

        bool loginSuccess = await _authService.LoginAsync(TableName, UsernameColumn, PasswordColumn, username, password);

        if (loginSuccess)
        {
            await DisplayAlert("Baþarýlý", "Giriþ baþarýlý!", "Tamam");
            await Navigation.PushAsync(new OtomatTakipPage());
        }
        else
        {
            LoginErrorLabel.Text = "Hatalý kullanýcý adý veya þifre.";
            LoginErrorLabel.IsVisible = true;
        }
    }
}
