namespace NetUpdaterClone.Core;

public sealed class UpdateConfig
{
    public string Version = "Unspecified";
    public string ReleaseDate = "Unspecified";
    public string ImageUrl = "";
    public string ReleaseNotesUrl = "";
    public string SpecialPostInstructions = "";
    public override string ToString() => $"{Version} - {ReleaseDate}";
}

public sealed class PortInfo
{
    public string Name = "";
    public int Count = 1;
}

public sealed class ProductInfo
{
    public string Name = "Unspecified";
    public int ProductId = 0x7F;
    public int UpdateProductId = 0x7F;
    public int FamilyId = 0x7F;
    public int BrandId = 0;
    public string SpecialPreInstructions = "";
    public string BrandName = "";
    public string FamilyName = "";
    public List<PortInfo> Ports = new();
    public List<UpdateConfig> Revisions = new();
}
