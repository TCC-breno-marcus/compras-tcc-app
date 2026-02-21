using PdfSharpCore.Fonts;

namespace Services;

internal static class PdfFontResolver
{
    private static readonly object Sync = new();
    private static bool _initialized;

    public static void EnsureInitialized()
    {
        if (_initialized)
        {
            return;
        }

        lock (Sync)
        {
            if (_initialized)
            {
                return;
            }

            // PdfSharpCore pode inicializar um resolver padrão internamente.
            // Forçamos o resolver embutido para não depender de fontes do SO/container.
            GlobalFontSettings.FontResolver = new EmbeddedFontResolver();

            _initialized = true;
        }
    }

    private sealed class EmbeddedFontResolver : IFontResolver
    {
        private const string FamilyName = "AppSans";
        private const string RegularFace = "AppSans#Regular";
        private const string BoldFace = "AppSans#Bold";

        private static readonly byte[] RegularFontBytes = LoadFont("DejaVuSans.ttf");
        private static readonly byte[] BoldFontBytes = LoadFont("DejaVuSans-Bold.ttf");

        public string DefaultFontName => FamilyName;

        public byte[]? GetFont(string faceName)
        {
            return faceName switch
            {
                RegularFace => RegularFontBytes,
                BoldFace => BoldFontBytes,
                _ => RegularFontBytes,
            };
        }

        public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
        {
            return new FontResolverInfo(isBold ? BoldFace : RegularFace);
        }

        private static byte[] LoadFont(string fileName)
        {
            var path = Path.Combine(AppContext.BaseDirectory, "Fonts", fileName);
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(
                    $"Fonte PDF não encontrada em '{path}'. Verifique a cópia para output/publish."
                );
            }

            return File.ReadAllBytes(path);
        }
    }
}
