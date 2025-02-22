using KontrolApp.Services;
using Microsoft.Extensions.Logging;

namespace KontrolApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            AppContext.SetSwitch("System.Reflection.NullabilityInfoContext.IsSupported", true);
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
            builder.Services.AddSingleton<SupabaseAuthService>();
            builder.Services.AddSingleton<BoldIfDevamEdiyor>();
            builder.Services.AddSingleton<OrangeIfDevamEdiyor>();
#endif

            return builder.Build();
        }
    }
}
