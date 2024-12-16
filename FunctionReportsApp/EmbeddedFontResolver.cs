using GrapeCity.ActiveReports;
using GrapeCity.Documents.Text;
using System.Reflection;

internal sealed class EmbeddedFontResolver : IFontResolver
{
    internal static readonly IFontResolver Instance = new EmbeddedFontResolver();
    internal static readonly Font Ipag;
    static EmbeddedFontResolver()
    {
        Ipag = Font.FromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("FunctionReportsApp.Fonts.ipag.ttf"));
    }
    internal EmbeddedFontResolver() { }
    public FontCollection GetFonts(string familyName, bool isBold, bool isItalic)
    {
        var fonts = new FontCollection();
        fonts.AppendFonts(new Font[] { Ipag }, true);
        return fonts;
    }
}