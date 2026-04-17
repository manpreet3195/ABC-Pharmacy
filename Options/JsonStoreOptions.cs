namespace ABCPharmacy.Options;

public sealed class JsonStoreOptions
{
    public const string SectionName = "JsonStore";
    public string MedicinesFilePath { get; set; } = "App_Data/medicines.json";
    public string SalesFilePath { get; set; } = "App_Data/sales.json";
}
