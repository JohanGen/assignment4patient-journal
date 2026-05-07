using System.Windows;

namespace PatientJournal.Desktop.Models;

public class EventLogItem
{
    public string TimeStr { get; set; } = "";
    public string Summary { get; set; } = "";
    public string? Warning { get; set; }
    public Visibility WarningVisibility => string.IsNullOrEmpty(Warning) ? Visibility.Collapsed : Visibility.Visible;
}
