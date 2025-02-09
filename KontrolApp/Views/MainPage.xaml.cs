using Supabase;
using Microsoft.Maui.Controls;
using System;

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
            await Navigation.PushAsync(new HatTakipPage());
        }

        private async void OnKesimTakipClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new KesimTakipPage());
        }

        private async void OnOtomatTakipClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new OtomatTakipPage());
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
