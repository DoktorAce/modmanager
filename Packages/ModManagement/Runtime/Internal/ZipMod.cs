using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;

namespace ModManagement.Internal
{
    public class ZipMod : IMod
    {
        private string _name;
        private string _path;
        ModManager.TextureSettings _defaultTextureSettings;

        private Dictionary<string, ModFileList<int, string>> _texts = new Dictionary<string, ModFileList<int, string>>();
        private ModFileList<int, Texture2D> _textures;

        public ZipMod(string name, string path, string[] textExtensions, ModManager.TextureSettings defaultTextureSettings)
        {
            _name = name;
            _path = path;
            _defaultTextureSettings = defaultTextureSettings;

            foreach(var ext in textExtensions)
            {
                _texts.Add(ext, new ModFileList<int, string>(LoadBytes, Utilities.LoadTextFile));
            }

            _textures = new ModFileList<int, Texture2D>(LoadBytes, LoadTexture);
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

            if(LoadZipFile(_path, out var output))
            {
                foreach(var entry in output)
                {
                    if(_texts.ContainsKey(entry.extension))
                    {
                        _texts[entry.extension].Add(entry.name, entry.index);
                    }
                    else 
                    {
                        switch(entry.extension)
                        {
                            case ".png":
                            case ".jpg":
                            case ".jpeg":
                                _textures.Add(entry.name, entry.index);
                                break;
                        }
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

        bool LoadZipFile(string path, out (string name, string extension, int index)[] output)
        {
            if(System.IO.File.Exists(path) == false) throw new FileNotFoundException($"Could not find file at {path}");

            FileStream fileStream = null;
            try
            {
                fileStream = new FileStream(path, FileMode.Open);
            }
            catch
            {
                Debug.Log($"Could not load file at {path}");
            }

            if(fileStream == null) 
            {
                output = null;
                return false;
            }

            var zipFile = new ZipFile(fileStream);
            bool wasSuccessful;
                                
            if(zipFile.TestArchive(true, TestStrategy.FindAllErrors, (TestStatus status, string message) => 
            {
                if(status.ErrorCount > 0) Debug.Log($"Zip file error: {message}");
            }
            )) {
                var list = new List<(string, string, int)>();

                foreach( ZipEntry zipEntry in zipFile )
                {
                    // Ignore directories
                    if( !zipEntry.IsFile )
                        continue;        
                    
                    var entryFileName = zipEntry.Name;

                    // Skip .DS_Store files (these appear on OSX)
                    if( entryFileName.Contains( "DS_Store" ) || entryFileName.Contains( "_MACOSX" ) )
                        continue;


                    var name =  Path.GetFileNameWithoutExtension(entryFileName);
                    var extension =  Path.GetExtension(entryFileName).ToLower();
                    var index = (int) zipEntry.ZipFileIndex;

                    list.Add((name, extension, index));
                }

                output = list.ToArray();
                wasSuccessful = true;
            }
            else 
            {
                output = null;
                wasSuccessful = false;
            }

            zipFile.IsStreamOwner = false;
            zipFile.Close();
            fileStream.Close();

            return wasSuccessful;
        }

        byte[] LoadBytes(int fileIndex)
        {
            byte[] result = null;

            FileStream fileStream = new FileStream(_path, FileMode.Open);
            var zipFile = new ZipFile(fileStream);
            var zipStream = zipFile.GetInputStream(fileIndex);

            using(var memoryStream = new MemoryStream())
            {
                zipStream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
                memoryStream.Close();
            }

            zipFile.IsStreamOwner = false;
            zipFile.Close();
            fileStream.Close();

            return result;
        }
    }
}