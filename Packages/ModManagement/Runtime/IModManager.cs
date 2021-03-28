using System;

namespace ModManagement
{
    public interface IModManager : IDisposable
    {
        void Reload(bool reloadDirectory = true);
        IMod[] mods { get; }
        bool HasMod(string name);
        IMod GetMod(string name);
    }
}