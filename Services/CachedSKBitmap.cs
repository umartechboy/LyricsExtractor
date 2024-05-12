using SkiaSharp;

namespace LyricsExtractor.Services
{
    public class CachedSKBitmap
    {
        string source;
        SKBitmap cache;
        public async Task<SKBitmap> Get()
        {
            if (cache == null)
            {
                cache = SKBitmap.Decode(source);
            }
            return cache;
        }

		public void FreeALL()
		{
            if (cache != null)
                cache.Dispose();
            cache = null;
		}

		public CachedSKBitmap(string source)
        {
            this.source = source;
        }
    }
}
