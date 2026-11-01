namespace DocumentProcesor.Models;

public class DrivingLicence
{
    public string DocumentType { get; set; } =
        "Irish Driving Licence";

    public string Country { get; set; } =
        "Ireland";

    public string? Surname { get; set; }

    public string? FirstName { get; set; }

    public string? DateOfBirth { get; set; }

    public string? DateOfIssue { get; set; }

    public string? DateOfExpiry { get; set; }

    public string? LicenceNumber { get; set; }

    public string? IssuingAuthority { get; set; }

    public string? Address { get; set; }

    public string[] LicenceCategories { get; set; } =
        Array.Empty<string>();
}