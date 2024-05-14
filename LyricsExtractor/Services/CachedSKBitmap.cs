using SkiaSharp;

namespace LyricsExtractor.Services
{
    public class CachedSKImage
    {
        string source;
        SKImage cache;
        public async Task<SKImage> Get()
        {
            if (cache == null)
            {
                cache = SKImage.FromEncodedData(source);
            }
            return cache;
        }

		public void FreeALL()
		{
            if (cache != null)
                cache.Dispose();
            cache = null;
		}

		public CachedSKImage(string source)
        {
            this.source = source;
        }
    }
}
