using System.Windows;

namespace PatientJournal.Desktop;

public partial class App : Application
{
    public static string ApiBaseUrl { get; set; } = "http://localhost:5200";
}

