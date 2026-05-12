using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

public class FixMaterialsToURP
{
    [MenuItem("Tools/Fix NPC Materials to URP")]
    public static void Execute()
    {
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            Debug.LogError("URP Lit shader not found!");
            return;
        }

        // Find all materials in the npc_casual_set_00 folders (both Materials and MaterialsUPR)
        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { "Assets/npc_casual_set_00/Materials" });
        
        int fixedCount = 0;
        
        foreach (string guid in materialGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            
            if (mat == null) continue;
            
            // Fix materials that have broken/error shaders or Standard shader
            if (mat.shader == null || mat.shader.name == "Hidden/InternalErrorShader" || 
                mat.shader.name == "Standard" || mat.shader.name.Contains("Error") ||
                !mat.shader.name.Contains("Universal"))
            {
                // Save properties before switching
                Texture mainTex = mat.HasProperty("_MainTex") ? mat.GetTexture("_MainTex") : null;
                Color color = mat.HasProperty("_Color") ? mat.GetColor("_Color") : Color.white;
                Texture bumpMap = mat.HasProperty("_BumpMap") ? mat.GetTexture("_BumpMap") : null;
                float bumpScale = mat.HasProperty("_BumpScale") ? mat.GetFloat("_BumpScale") : 1f;
                Texture metallicMap = mat.HasProperty("_MetallicGlossMap") ? mat.GetTexture("_MetallicGlossMap") : null;
                float metallic = mat.HasProperty("_Metallic") ? mat.GetFloat("_Metallic") : 0f;
                float smoothness = mat.HasProperty("_Glossiness") ? mat.GetFloat("_Glossiness") : 0.5f;
                Texture emissionMap = mat.HasProperty("_EmissionMap") ? mat.GetTexture("_EmissionMap") : null;
                Color emissionColor = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor") : Color.black;
                Texture occlusionMap = mat.HasProperty("_OcclusionMap") ? mat.GetTexture("_OcclusionMap") : null;
                float cutoff = mat.HasProperty("_Cutoff") ? mat.GetFloat("_Cutoff") : 0.5f;

                bool isAlphaClip = mat.IsKeywordEnabled("_ALPHATEST_ON");
                bool isTransparent = mat.IsKeywordEnabled("_ALPHABLEND_ON") || mat.IsKeywordEnabled("_ALPHAPREMULTIPLY_ON");
                bool hasEmission = mat.IsKeywordEnabled("_EMISSION");

                string oldShader = mat.shader.name;
                
                // Switch shader
                mat.shader = urpLit;

                // Apply properties to URP Lit
                mat.SetTexture("_BaseMap", mainTex);
                mat.SetColor("_BaseColor", color);

                if (bumpMap != null)
                {
                    mat.SetTexture("_BumpMap", bumpMap);
                    mat.SetFloat("_BumpScale", bumpScale);
                    mat.EnableKeyword("_NORMALMAP");
                }

                if (metallicMap != null)
                {
                    mat.SetTexture("_MetallicGlossMap", metallicMap);
                    mat.EnableKeyword("_METALLICSPECGLOSSMAP");
                }
                mat.SetFloat("_Metallic", metallic);
                mat.SetFloat("_Smoothness", smoothness);

                if (occlusionMap != null)
                    mat.SetTexture("_OcclusionMap", occlusionMap);

                if (hasEmission)
                {
                    mat.SetTexture("_EmissionMap", emissionMap);
                    mat.SetColor("_EmissionColor", emissionColor);
                    mat.EnableKeyword("_EMISSION");
                    mat.globalIlluminationFlags = MaterialGlobalIlluminationFlags.BakedEmissive;
                }

                // Surface type
                if (isAlphaClip)
                {
                    mat.SetFloat("_AlphaClip", 1f);
                    mat.SetFloat("_Cutoff", cutoff);
                    mat.EnableKeyword("_ALPHATEST_ON");
                    mat.SetFloat("_Surface", 0);
                    mat.renderQueue = (int)RenderQueue.AlphaTest;
                }
                else if (isTransparent)
                {
                    mat.SetFloat("_Surface", 1);
                    mat.SetFloat("_Blend", 0);
                    mat.renderQueue = (int)RenderQueue.Transparent;
                }
                else
                {
                    mat.SetFloat("_Surface", 0);
                    mat.renderQueue = (int)RenderQueue.Geometry;
                }

                EditorUtility.SetDirty(mat);
                fixedCount++;
                Debug.Log($"Fixed material: {path} (was: {oldShader})");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Done! Fixed {fixedCount} materials to URP Lit.");
    }
}
