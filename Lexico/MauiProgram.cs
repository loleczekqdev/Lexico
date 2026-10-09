using Microsoft.Extensions.Logging;

namespace Lexico
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("PlusJakartaSans-Bold.ttf", "JakartaBold");
                    fonts.AddFont("PlusJakartaSans-BoldItalic.ttf", "JakartaBoldItalic");
                    fonts.AddFont("PlusJakartaSans-ExtraBold.ttf", "JakartaExtraBold");
                    fonts.AddFont("PlusJakartaSans-ExtraBoldItalic.ttf", "JakartaExtraBoldItalic");
                    fonts.AddFont("PlusJakartaSans-ExtraLight.ttf", "JakartaExtraLight");
                    fonts.AddFont("PlusJakartaSans-ExtraLightItalic.ttf", "JakartaExtraLightItalic");
                    fonts.AddFont("PlusJakartaSans-Italic.ttf", "JakartaItalic");
                    fonts.AddFont("PlusJakartaSans-Light.ttf", "JakartaLight");
                    fonts.AddFont("PlusJakartaSans-LightItalic.ttf", "JakartaLightItalic");
                    fonts.AddFont("PlusJakartaSans-Medium.ttf", "JakartaMedium");
                    fonts.AddFont("PlusJakartaSans-MediumItalic.ttf", "JakartaMediumItalic");
                    fonts.AddFont("PlusJakartaSans-Regular.ttf", "JakartaRegular");
                    fonts.AddFont("PlusJakartaSans-SemiBold.ttf", "JakartaSemiBold");
                    fonts.AddFont("PlusJakartaSans-SemiBoldItalic.ttf", "JakartaSemiBoldItalic");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
