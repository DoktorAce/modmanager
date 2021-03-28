using UnityEngine;

namespace ModManagement.Internal
{
    public static class Utilities
    {
        public static string LoadTextFile(byte[] bytes) => System.Text.Encoding.UTF8.GetString(bytes);

        public static Texture2D LoadTexture(byte[] bytes, ModManager.TextureSettings settings)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, settings.useMipMaps);
            texture.LoadImage(bytes);
            texture.Apply(settings.useMipMaps);
            texture.filterMode = settings.filterMode;
            if(settings.useCompression) texture.Compress(true);
            return texture;
        }
    }
}