/*
 *  TexturePacker Importer
 *  (c) CodeAndWeb GmbH, Saalbaustraße 61, 89233 Neu-Ulm, Germany
 *
 *  Use this script to import sprite sheets generated with TexturePacker.
 *  For more information see https://www.codeandweb.com/texturepacker/unity
 *
 */

using UnityEngine;
using UnityEditor;

// Note: TexturePacker Importer with Unity 2021.2 (or newer) requires the "Sprite 2D" package,
//       please make sure that it is part of your Unity project. You can install it using
//       Unity's package manager.

#if UNITY_2021_2_OR_NEWER
using UnityEditor.U2D.Sprites;
using System.Collections.Generic;
#endif

namespace TexturePackerImporter {
public class SpritesheetImporter : AssetPostprocessor {
    void OnPreprocessTexture() {
        var importer = assetImporter as TextureImporter;
        var sheet = TexturePackerImporter.getSheetInfo(importer);
        if (sheet != null) {
            Dbg.Log("Updating sprite sheet " + importer.assetPath);
#if UNITY_2021_2_OR_NEWER
            updateSprites(importer, sheet);
#else
                importer.spritesheet = sheet.metadata;
#endif
        }
    }

#if UNITY_2021_2_OR_NEWER
    static void updateSprites(TextureImporter importer, SheetInfo sheet) {
        var dataProvider = GetSpriteEditorDataProvider(importer);
        var spriteNameFileIdDataProvider =
            dataProvider.GetDataProvider<ISpriteNameFileIdDataProvider>();

        var oldIds = spriteNameFileIdDataProvider.GetNameFileIdPairs();
        var rects = sheetInfoToSpriteRects(sheet);
        var ids = generateSpriteIds(oldIds, rects);

        dataProvider.SetSpriteRects(rects);
        spriteNameFileIdDataProvider.SetNameFileIdPairs(ids);
        dataProvider.Apply();
        EditorUtility.SetDirty(importer);
    }

    static ISpriteEditorDataProvider GetSpriteEditorDataProvider(TextureImporter importer) {
        var dataProviderFactories = new SpriteDataProviderFactories();
        dataProviderFactories.Init();
        var dataProvider = dataProviderFactories.GetSpriteEditorDataProviderFromObject(importer);
        dataProvider.InitSpriteEditorDataProvider();
        return dataProvider;
    }

    static SpriteRect[] sheetInfoToSpriteRects(SheetInfo sheet) {
        var spriteCount = sheet.metadata.Length;
        var rects = new SpriteRect[spriteCount];

        for (var i = 0; i < spriteCount; i++) {
            var sr = rects[i] = new SpriteRect();
            var smd = sheet.metadata[i];

            sr.name = smd.name;
            sr.rect = smd.rect;
            sr.pivot = smd.pivot;
            sr.border = smd.border;
            sr.alignment = (SpriteAlignment)smd.alignment;

            // sr.spriteID not yet initialized, this is done in generateSpriteIds()
        }

        return rects;
    }

    static SpriteNameFileIdPair[] generateSpriteIds(IEnumerable<SpriteNameFileIdPair> oldIds,
        SpriteRect[] sprites) {
        var newIds = new SpriteNameFileIdPair[sprites.Length];

        for (var i = 0; i < sprites.Length; i++) {
            sprites[i].spriteID = idForName(oldIds, sprites[i].name);
            newIds[i] = new SpriteNameFileIdPair(sprites[i].name, sprites[i].spriteID);
        }

        return newIds;
    }

    static GUID idForName(IEnumerable<SpriteNameFileIdPair> oldIds, string name) {
        foreach (var old in oldIds) {
            if (old.name == name) {
                return old.GetFileGUID();
            }
        }

        return GUID.Generate();
    }
#endif
}
}
