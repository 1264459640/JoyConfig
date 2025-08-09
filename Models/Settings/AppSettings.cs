namespace JoyConfig.Models.Settings
{
    /// <summary>
    /// Represents the application settings that are persisted.
    /// </summary>
    public class AppSettings
    {
        // Database Configuration
        public string? AttributeDatabasePath { get; set; }
        public string? GameplayEffectDatabasePath { get; set; }

        // Appearance
        public string Theme { get; set; } = "Light"; // Default to Light theme
        public string Language { get; set; } = "en-US"; // Default to English

        // Editor Behavior
        public bool IsAutosaveEnabled { get; set; } = false;
        public int AutosaveInterval { get; set; } = 5; // In minutes
        public bool LoadLastDatabaseOnStartup { get; set; } = true;
    }
}
