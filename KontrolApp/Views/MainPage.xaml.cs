using Supabase;
using Microsoft.Maui.Controls;
using System;
using KontrolApp.Views;

namespace KontrolApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()

        {
            InitializeComponent();
        }

        private async void OnHatTakipClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new HatTakipLoginPage());
        }

        private async void OnKesimTakipClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KesimTakipLoginPage());
        }

        private async void OnOtomatTakipClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OtomatTakipLoginPage());
        }

        private async void OnRaporlarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RaporlarPage());
        }

        private async void OnAyarlarClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AyarlarPage());
        }
    }
}
