using System.IO;
using System.Linq;
using System.Collections.Generic;
using ModManagement.Internal;
using System;
using UnityEngine;

namespace ModManagement
{
    public class ModManager : IModManager
    {
        [Serializable]
        public struct Settings
        {
            public string path;
            public string[] extensions;
            public string[] textExtensions;
            public TextureSettings defaultTextureSettings;
        }

        [Serializable]
        public struct TextureSettings
        {
            public FilterMode filterMode;
            public bool useMipMaps;
            public bool useCompression;
        }

        private List<string> _names = new List<string>();
        private IMod[] _mods = null;
        private Settings _settings;

        public ModManager(Settings settings)
        {
            _settings = settings;
            Reload(true);
        }

        public IMod[] mods => _mods;

        public void Reload(bool reloadDirectory = true)
        {
            if(reloadDirectory)
            {
                var list = new List<IMod>();

                var directory = new DirectoryInfo(_settings.path);

                //var path = Path.Combine(UnityEngine.Application.persistentDataPath, modFolderName);
                if(!directory.Exists) Directory.CreateDirectory(_settings.path);
                var info = new DirectoryInfo(_settings.path);

                // Load directory mods
                foreach(var subDir in directory.GetDirectories())
                {
                    list.Add(new DirectoryMod(subDir.Name, subDir.FullName, _settings.textExtensions, _settings.defaultTextureSettings));
                }

                // Load zipped mods
                var zippedMods = info.GetFiles()
                .Where(x => _settings.extensions.Contains(x.Extension.ToLower()))
                .Select(x => new ZipMod(Path.GetFileNameWithoutExtension(x.FullName), x.FullName, _settings.textExtensions, _settings.defaultTextureSettings))
                .ToArray();
                list.AddRange(zippedMods);

                _mods = list.ToArray();

                _names.Clear();
                _names.AddRange(_mods.Select(x => x.name));
            }

            // Reload all mods
            foreach(var mod in _mods) mod.Reload();
        }

        public bool HasMod(string name) => _names.Contains(name);

        public IMod GetMod(string name) => _mods[_names.IndexOf(name)];

        public void Dispose()
        {
            foreach(var mod in _mods) mod.Dispose();
            _mods = null;
            _names.Clear();
            _names = null;
        }
    }
}