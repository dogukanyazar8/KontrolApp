using System.Collections.ObjectModel;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using KontrolApp.Views;
using System.Windows.Input;
using KontrolApp.Services;

namespace KontrolApp;

public partial class OtomatTakipPage : ContentPage
{
    private SupabaseService _supabaseService = new SupabaseService();
    private const string TableName = "makineListesi";
    private const string IdColumn = "id";
    private const string NameColumn = "otomat_adi";
    private const string StatusColumn = "otomat_durumu";
    private const string OrderNo = "siparis_no";
    private const string ModelColumn = "model_adi";
    private const string OrderQuantity = "siparis_adet";
    private const string CompletedOrders = "siparis_tamamlanan";

    public OtomatTakipPage()
    {
        InitializeComponent();
        LoadData();
    }
    public async Task RefreshData()
    {
        MachinesList_Refreshing(MachinesList, EventArgs.Empty);
    }

    public async void MachinesList_Refreshing(object sender, EventArgs e)
    {
        await LoadData();
        MachinesList.IsRefreshing = false;
    }
    public async Task LoadData()
    {
        var machines = await _supabaseService.GetDataAsync(TableName, IdColumn, new[] { IdColumn, NameColumn, StatusColumn, OrderNo, ModelColumn, OrderQuantity, CompletedOrders });

        MainThread.BeginInvokeOnMainThread(() =>
        {
        MachinesList.ItemsSource = machines
            .OrderBy(m => m.ContainsKey(IdColumn) && m[IdColumn] != null ? Convert.ToInt32(m[IdColumn]) : int.MaxValue)
            .Select(m =>
            {
                int totalOrders = m.ContainsKey(OrderQuantity) && m[OrderQuantity] != null ? Convert.ToInt32(m[OrderQuantity]) : 0;
                int completedOrders = m.ContainsKey(CompletedOrders) && m[CompletedOrders] != null ? Convert.ToInt32(m[CompletedOrders]) : 0;
                double completionRate = (totalOrders > 0) ? (double)completedOrders / totalOrders : 0.0;
                int completionPercentage = (int)(completionRate * 100);
                double barWidth = 150 * completionRate;

                return new
                {
                    id = m.ContainsKey(IdColumn) && m[IdColumn] != null ? Convert.ToInt32(m[IdColumn]) : 0,
                    otomat_adi = m.ContainsKey(NameColumn) && m[NameColumn] != null ? m[NameColumn].ToString() : "Bilinmiyor",
                    siparis_no = m.ContainsKey(OrderNo) && m[OrderNo] != null ? m[OrderNo].ToString() : "Bilinmiyor",
                    model_adi = m.ContainsKey(ModelColumn) && m[ModelColumn] != null ? m[ModelColumn].ToString() : "Bilinmiyor",
                    siparis_adet = totalOrders,
                    siparis_tamamlanan = completedOrders,
                    otomat_durumu = m.ContainsKey(StatusColumn) && m[StatusColumn] != null ? m[StatusColumn].ToString() : "Bilinmiyor",
                    tamamlanma_orani = completionRate,
                    tamamlanma_yuzdesi = $"{completionPercentage}%",
                    progress_width = barWidth
                };
            })
                 .ToList();
        });
    }

    private async void OnUpdateStatusClicked(object sender, EventArgs e)
    {
        Button button = sender as Button;
        if (button == null || button.CommandParameter == null)
            return;

        int otomatId = Convert.ToInt32(button.CommandParameter);
        var selectedOtomat = MachinesList.ItemsSource.Cast<dynamic>().FirstOrDefault(m => m.id == otomatId);

        if (selectedOtomat != null)
        {
            string otomatName = selectedOtomat.otomat_adi;
            await Navigation.PushAsync(new OtomatTakipDurum(otomatId, otomatName));
        }
    }
    private async void OnOtomatlariKapatClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Otomatlarý Kapat", "Tüm otomatlarý kapatmak ve sipariþ verilerini sýfýrlamak istediðinize emin misiniz?", "Evet", "Hayýr");
        if (!confirm) return;

        var updateData = new Dictionary<string, object>
    {
        { "otomat_durumu", "Kapalý" },
        { "siparis_no", "" },
        { "model_adi", "" },
        { "siparis_adet", 0 },
        { "siparis_tamamlanan", 0 },
        { "renk", "" }
    };

        foreach (var item in MachinesList.ItemsSource)
        {
            var machine = item.GetType().GetProperty("id")?.GetValue(item);
            if (machine != null)
            {
                await _supabaseService.UpdateDataAsync("makineListesi", "id", Convert.ToInt32(machine), updateData);
            }
        }

        await DisplayAlert("Baþarýlý", "Tüm otomatlar kapatýldý ve sipariþ bilgileri sýfýrlandý.", "Tamam");
        await LoadData();
    }

}
