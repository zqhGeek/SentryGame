using System.IO;
using System.Linq;
using System.Xml.Linq;
using UnityEditor.Android;

namespace SentryGame.Editor
{
    public sealed class AndroidManifestOrientationPostProcessor : IPostGenerateGradleAndroidProject
    {
        private const string ManifestRelativePath = "src/main/AndroidManifest.xml";
        private const string UnityGameActivityName = "com.unity3d.player.UnityPlayerGameActivity";
        private const string ReversePortrait = "reversePortrait";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Unity 6000 的 PlayerSettings 方向枚举在本项目中生成了 reverseLandscape，
            // 因此在 Gradle 工程生成后直接修补最终参与合并的 unityLibrary Manifest。
            var manifestPath = Path.Combine(path, ManifestRelativePath);
            var document = XDocument.Load(manifestPath);
            var android = XNamespace.Get("http://schemas.android.com/apk/res/android");
            var activity = document.Descendants("activity")
                .FirstOrDefault(element => (string)element.Attribute(android + "name") == UnityGameActivityName);
            if (activity == null)
            {
                throw new FileNotFoundException($"未找到 Android Activity：{UnityGameActivityName}", manifestPath);
            }

            activity.SetAttributeValue(android + "screenOrientation", ReversePortrait);
            document.Save(manifestPath);
        }
    }
}
