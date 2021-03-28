using System.Collections.Generic;
using System.IO;
using System.Linq;
using ModManagement;
using UnityEngine;

public class Tester : MonoBehaviour
{
    [SerializeField] ModManager.Settings _modSettings;
    [SerializeField] List<Texture2D> _modTextures = null;

    void Start()
    {
        var jsonExtension = ".json";

        _modSettings.path = Path.Combine(UnityEngine.Application.persistentDataPath, _modSettings.path);
        var modManager = new ModManager(_modSettings);

        UnityEngine.Debug.Log($"Found {modManager.mods.Length} mods: {string.Join(", ", modManager.mods.Select(x => x.name))}");
        
        foreach(var mod in modManager.mods) 
        {
            UnityEngine.Debug.Log($"==== {mod.name} ====");
            UnityEngine.Debug.Log($"Json-files: {string.Join(", ", mod.ListText(jsonExtension))}");
            UnityEngine.Debug.Log($"Texture-files: {string.Join(", ", mod.ListTextures())}");

            foreach(var jsonName in mod.ListText(jsonExtension))
            {
                var text = mod.LoadText(jsonExtension, jsonName);
                Debug.Log(text);

            }

            foreach(var texName in mod.ListTextures())
            {
                _modTextures.Add(mod.LoadTexture(texName));
            }
        }

        modManager.Dispose();
    }
}