using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using YooAsset.Editor;

namespace Game.Editor
{
    public class BuildTool
    {
        // YooAsset 构建
        private static void BuildYooAssets()
        {
            SaveScene();

            string packageName = "DefaultPackage";

            // 内置着色器资源包名称
            var builtinShaderBundleName = GetBuiltinShaderBundleName(packageName);

            ScriptableBuildParameters buildParameters = new ScriptableBuildParameters
            {
                BuildOutputRoot = AssetBundleBuilderHelper.GetDefaultBuildOutputRoot(),
                BuildinFileRoot = AssetBundleBuilderHelper.GetStreamingAssetsRoot(),
                BuildPipeline = EBuildPipeline.ScriptableBuildPipeline.ToString(),
                BuildTarget = EditorUserBuildSettings.activeBuildTarget,
                PackageName = packageName
            };
            // 获取当前时间
            DateTimeOffset now = DateTimeOffset.UtcNow;
            // 获取 Unix 时间戳（秒数）
            long unixTimestamp = now.ToUnixTimeSeconds();
            string packageVersion = unixTimestamp.ToString();
            buildParameters.PackageVersion = packageVersion;
            buildParameters.VerifyBuildingResult = true;
            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyOption = EBuildinFileCopyOption.ClearAndCopyAll;
            buildParameters.BuildinFileCopyParams = "";
            buildParameters.EncryptionServices = new FileOffsetEncryption();
            buildParameters.CompressOption = ECompressOption.LZ4;


            buildParameters.BuildBundleType = (int)EBuildBundleType.AssetBundle;

            buildParameters.EnableSharePackRule = true;

            buildParameters.FileNameStyle = EFileNameStyle.HashName;
            buildParameters.BuildinFileCopyParams = string.Empty;
            buildParameters.CompressOption = ECompressOption.LZ4;
            buildParameters.ClearBuildCacheFiles = false;
            buildParameters.UseAssetDependencyDB = true;
            buildParameters.BuiltinShadersBundleName = builtinShaderBundleName;
            buildParameters.EncryptionServices = new FileStreamEncryption();

            ScriptableBuildPipeline pipeline = new ScriptableBuildPipeline();
            var buildResult = pipeline.Run(buildParameters, true);

            if (buildResult.Success)
            {
                Debug.Log($"构建成功 : {buildResult.OutputPackageDirectory}");

                string destPath = Path.Combine("../Release", $"Bundles/CDN/{buildParameters.BuildTarget}/1.0.0");
                string sourcePath = buildResult.OutputPackageDirectory;

                // EditorTools.CopyDirectory(sourcePath, Path.GetFullPath(destPath));
            }
            else
            {
                Debug.LogError($"构建失败 : {buildResult.ErrorInfo}");
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 内置着色器资源包名称
        /// 注意：和自动收集的着色器资源包名保持一致！
        /// </summary>
        private static string GetBuiltinShaderBundleName(string packageName)
        {
            var uniqueBundleName = AssetBundleCollectorSettingData.Setting.UniqueBundleName;
            var packRuleResult = DefaultPackRule.CreateShadersPackRuleResult();
            return packRuleResult.GetBundleName(packageName, uniqueBundleName);
        }

        private static void SaveScene()
        {
            var scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("Please save the scene first.");
                return;
            }

            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Scene saved: {scene.path}");
        }
    }
}