using System.Xml;

namespace NetUpdaterClone.Core;

public static class UpdateTargetsParser
{
    public static List<ProductInfo> Parse(string xmlText)
    {
        var doc = new XmlDocument();
        try
        {
            doc.LoadXml(xmlText);
        }
        catch (XmlException ex)
        {
            throw new InvalidOperationException("Invalid Targets File: " + ex.Message);
        }

        var root = doc.DocumentElement;
        if (root == null || root.Name != "Config")
            throw new InvalidOperationException("Invalid element encountered. Expecting Config");

        var result = new List<ProductInfo>();

        foreach (XmlElement brand in Children(root, "Brand"))
        {
            var brandName = Attr(brand, "Name", "");
            var brandId = brandName == "Lexicon" ? 1 : brandName == "DigiTech" ? 2 : 0;

            foreach (XmlElement family in Children(brand, "Family"))
            {
                var familyId = ParseHex(Attr(family, "FamilyID", "0x7F"));

                foreach (XmlElement product in Children(family, "Product"))
                {
                    var p = new ProductInfo
                    {
                        BrandName = brandName,
                        FamilyName = Attr(family, "Name", ""),
                        Name = Attr(product, "Name", "Unspecified"),
                        ProductId = ParseHex(Attr(product, "ProductID", "0x7F")),
                        FamilyId = familyId,
                        BrandId = brandId,
                        SpecialPreInstructions = Attr(product, "SpecialPreInstructions", ""),
                    };

                    var upd = Attr(product, "UpdateProductID", null);
                    p.UpdateProductId = string.IsNullOrEmpty(upd) ? p.ProductId : ParseHex(upd);

                    var revs = Child(product, "Revisions");
                    if (revs != null)
                    {
                        foreach (XmlElement rev in Children(revs, "Revision"))
                        {
                            p.Revisions.Add(new UpdateConfig
                            {
                                Version = Attr(rev, "Version", "Unspecified"),
                                ReleaseDate = Attr(rev, "ReleaseDate", "Unspecified"),
                                ImageUrl = Attr(rev, "ImageUrl", ""),
                                ReleaseNotesUrl = Attr(rev, "ReleaseNotesUrl", ""),
                                SpecialPostInstructions = Attr(rev, "SpecialPostInstructions", ""),
                            });
                        }
                    }

                    var ports = Child(product, "Ports");
                    if (ports != null)
                    {
                        foreach (XmlElement port in Children(ports, "Port"))
                        {
                            p.Ports.Add(new PortInfo
                            {
                                Name = Attr(port, "Name", ""),
                                Count = ParseInt(Attr(port, "Count", "1")),
                            });
                        }
                    }

                    result.Add(p);
                }
            }
        }

        return result;
    }

    private static IEnumerable<XmlElement> Children(XmlElement parent, string name)
    {
        foreach (XmlNode node in parent.ChildNodes)
            if (node is XmlElement el && el.Name == name)
                yield return el;
    }

    private static XmlElement Child(XmlElement parent, string name)
    {
        foreach (XmlNode node in parent.ChildNodes)
            if (node is XmlElement el && el.Name == name)
                return el;
        return null;
    }

    private static string Attr(XmlElement el, string name, string fallback)
    {
        var a = el.GetAttribute(name);
        return string.IsNullOrEmpty(a) && fallback != null ? fallback : a;
    }

    // Mirrors sub_488B30: accumulates every hex digit found anywhere in the string,
    // skipping the rest ("0x5E" -> 0x5E, "5E" -> 0x5E).
    private static int ParseHex(string s)
    {
        var v = 0;
        foreach (var c in s)
        {
            var d = HexDigit(c);
            if (d >= 0)
                v = (v << 4) | d;
        }
        return v;
    }

    private static int HexDigit(char c)
    {
        if (c >= '0' && c <= '9') return c - '0';
        if (c >= 'a' && c <= 'f') return c - 'a' + 10;
        if (c >= 'A' && c <= 'F') return c - 'A' + 10;
        return -1;
    }

    private static int ParseInt(string s)
    {
        if (int.TryParse(s, out var v)) return v;
        return 1;
    }
}
