using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace ModManagement.Internal
{
    public class DirectoryMod : IMod
    {
        private string _name;
        private string _path;
        ModManager.TextureSettings _defaultTextureSettings;

        private Dictionary<string, ModFileList<string, string>> _texts = new Dictionary<string, ModFileList<string, string>>();
        private ModFileList<string, Texture2D> _textures;

        public DirectoryMod(string name, string path, string[] textExtensions, ModManager.TextureSettings defaultTextureSettings)
        {
            _name = name;
            _path = path;
            _defaultTextureSettings = defaultTextureSettings;

            foreach(var ext in textExtensions)
            {
                _texts.Add(ext, new ModFileList<string, string>(LoadBytes, Utilities.LoadTextFile));
            }

            _textures = new ModFileList<string, Texture2D>(LoadBytes, LoadTexture);
        }

        public string name => _name;

        public void Dispose()
        {
            foreach(var kvp in _texts) kvp.Value.Dispose();
            _texts.Clear();
            _textures.Dispose();        
        }

        public void Reload()
        {
            foreach(var kvp in _texts) kvp.Value.Clear();
            _textures.Clear();

            var output = LoadFolder(_path);
            foreach(var entry in output)
            {
                if(_texts.ContainsKey(entry.extension))
                {
                    _texts[entry.extension].Add(entry.name, entry.path);
                }
                else 
                {
                    switch(entry.extension)
                    {
                        case ".png":
                        case ".jpg":
                        case ".jpeg":
                            _textures.Add(entry.name, entry.path);
                            break;
                    }
                }
            }
        }

        public string[] ListText(string extension) => _texts[extension].List();
        public bool HasText(string extension, string name) => _texts[extension].Has(name);
        public string LoadText(string extension, string name) => _texts[extension].Get(name);

        public string[] ListTextures() => _textures.List();
        public bool HasTexture(string name) => _textures.Has(name);
        public Texture2D LoadTexture(string name) => _textures.Get(name);

        public Texture2D LoadTexture(string name, ModManager.TextureSettings settings)
        {
            var defaultSettings = _defaultTextureSettings;
            _defaultTextureSettings = settings;
            var texture = _textures.Get(name);
            _defaultTextureSettings = defaultSettings;
            return texture;
        }

        public Texture2D LoadTexture(byte[] bytes) => Utilities.LoadTexture(bytes, _defaultTextureSettings);

        (string name, string extension, string path)[] LoadFolder(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            if(!dir.Exists) throw new FileNotFoundException($"Could not find directory at {path}");

            var list = new List<(string, string, string)>();

            FileInfo[] allFiles = dir.GetFiles();
            foreach(FileInfo file in allFiles)
            {
                if( file.FullName.Contains( "DS_Store" ) || file.FullName.Contains( "_MACOSX" ) )
                    continue;

                list.Add((Path.GetFileNameWithoutExtension(file.Name), file.Extension.ToLower(), file.FullName));
            }

            return list.ToArray();
        }

        byte[] LoadBytes(string path)
        {
            if(System.IO.File.Exists(path) == false) throw new FileNotFoundException($"Could not find file at {path}");

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open);
            }
            catch
            {
                Debug.Log($"Could not open file at {path}");
            }

            if(fileStream == null) return null;

            byte[] result;

            using(var memoryStream = new MemoryStream())
            {
                fileStream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
                memoryStream.Close();
            }

            fileStream.Close();

            return result;
        }
    }
}