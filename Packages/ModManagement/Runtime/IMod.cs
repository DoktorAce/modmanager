using System;
using UnityEngine;

namespace ModManagement
{
    public interface IMod : IDisposable
    {
        string name { get; }

        void Reload();

        string[] ListText(string extension);
        bool HasText(string extension, string name);
        string LoadText(string extension, string name);

        string[] ListTextures();
        bool HasTexture(string name);
        Texture2D LoadTexture(string name);
        Texture2D LoadTexture(string name, ModManager.TextureSettings settings);
    }
}