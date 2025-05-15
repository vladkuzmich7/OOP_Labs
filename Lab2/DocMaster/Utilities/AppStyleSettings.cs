
namespace DocMaster.Utilities
{
    public sealed class AppStyleSettings
    {
        private static readonly Lazy<AppStyleSettings> _instance =
            new Lazy<AppStyleSettings>(() => new AppStyleSettings());

        public static AppStyleSettings Instance => _instance.Value;

        // Add background color 
        public ConsoleColor BackgroundColor { get; private set; }

        private AppStyleSettings()
        {
            // Default colors
            BackgroundColor = ConsoleColor.Black;
            ApplyColors();
        }

        public void ChangeColors(ConsoleColor background)
        {
            BackgroundColor = background;
            ApplyColors();
        }

        private void ApplyColors()
        {
            Console.BackgroundColor = BackgroundColor;
            Console.Clear(); // Clear for apply background
        }
        
    }
}
