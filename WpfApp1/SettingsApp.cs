namespace WpfApp1
{
    // класс для хранения настроек
    public class SettingsApp
    {
        public string Background { get; set; } = "#f6ecda";
        public string Foreground { get; set; } = "#000000";
        public double FontSize { get; set; } = 18;
        public double MainWindowWidth { get; set; } = 600;
        public double MainWindowHeight { get; set; } = 500;
        public double MainWindowLeft { get; set; } = 100;
        public double MainWindowTop { get; set; } = 100;
        public bool MainWindowIsMaximized { get; set; } = false;
        public double SecondWindowWidth { get; set; } = 600;
        public double SecondWindowHeight { get; set; } = 500;
        public double SecondWindowLeft { get; set; } = 100;
        public double SecondWindowTop { get; set; } = 100;
        public bool SecondWindowIsMaximized { get; set; } = false;
    }
}
