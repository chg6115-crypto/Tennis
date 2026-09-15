using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TennisGame.Editor
{
    public static class TennisMenu
    {
        [MenuItem("Tennis/Capture Game View %#k")]
        public static void CaptureGame()
        {
            if (!EditorApplication.isPlaying) return;
            Directory.CreateDirectory("Logs");
            ScreenCapture.CaptureScreenshot("Logs/MainTennisGame.png");
        }
        [MenuItem("Tennis/Open Main Tennis %#m")]
        public static void Open()
        {
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                EditorSceneManager.OpenScene("Assets/01_Scenes/MainTennis.unity");
        }

        [MenuItem("Tennis/Validate Main Tennis %#j")]
        public static void Validate()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before validation.");
            var scene = SceneManager.GetSceneByPath("Assets/01_Scenes/MainTennis.unity");
            if (!scene.isLoaded) { Open(); scene = SceneManager.GetSceneByPath("Assets/01_Scenes/MainTennis.unity"); }
            if (!scene.isLoaded) return;
            var match = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<TennisMatch>()).Single();
            int count = 0;
            void Check(bool ok, string name) { if (!ok) throw new Exception(name); count++; }
            Check(match.balance && match.court && match.player && match.enemy && match.ball, "Serialized match references");
            Check(match.player.visual && match.enemy.visual && match.ball.visual, "Replaceable visual roots");
            Check(scene.GetRootGameObjects().Any(g => g.name == "MainCamera" && g.GetComponent<Camera>()), "Scene camera");
            var court = match.court;
            for (int side = 0; side < 2; side++)
            for (int right = 0; right < 2; right++)
            {
                var p = court.Aim(side, Vector2.zero, true, right == 1);
                Check(court.Inside(p, 1 - side, true, right == 1), "Diagonal service box");
                Check(!court.Inside(p, 1 - side, true, right != 1), "Wrong service box rejected");
            }
            Check(!court.Inside(court.World(new Vector3(5, .12f, 8)), 1, false, true), "Wide shot rejected");
            Check(!court.Inside(court.World(new Vector3(0, .12f, 13)), 1, false, true), "Long shot rejected");
            var temp = new GameObject("Validation ball");
            try
            {
                var ball = temp.AddComponent<TennisBall>();
                int bounces = 0, nets = 0;
                ball.Bounced += (_, n) => bounces = n;
                ball.NetHit += () => nets++;
                ball.transform.position = court.World(new Vector3(2.1f, 1.8f, -12.485f));
                ball.Launch(court.Aim(0, Vector2.zero, true, true), 18, 1.87f, 0);
                for (int i = 0; i < 600 && ball.Moving; i++) ball.Step(1f / 60, court, match.balance);
                Check(nets == 0 && bounces == 2 && !ball.Moving, "Serve clears net then two bounces");
                ball.transform.position = court.World(new Vector3(0, .3f, -2));
                ball.Launch(court.World(new Vector3(0, .12f, 2)), 20, .05f, 0);
                ball.Step(.2f, court, match.balance);
                Check(nets == 1 && !ball.Moving, "Net collision survives large time step");
            }
            finally { UnityEngine.Object.DestroyImmediate(temp); }
            Directory.CreateDirectory("Logs");
            File.WriteAllText("Logs/MainTennisValidation.txt", $"PASS: {count} scene and trajectory checks");
            var camera = scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<Camera>()).Single();
            var texture = new RenderTexture(1280, 800, 24);
            var image = new Texture2D(1280, 800, TextureFormat.RGB24, false);
            var oldTarget = camera.targetTexture;
            var oldActive = RenderTexture.active;
            try
            {
                camera.targetTexture = texture;
                camera.Render();
                RenderTexture.active = texture;
                image.ReadPixels(new Rect(0, 0, 1280, 800), 0, 0);
                image.Apply();
                File.WriteAllBytes("Logs/MainTennisPreview.png", image.EncodeToPNG());
            }
            finally
            {
                camera.targetTexture = oldTarget;
                RenderTexture.active = oldActive;
                texture.Release();
                UnityEngine.Object.DestroyImmediate(texture);
                UnityEngine.Object.DestroyImmediate(image);
            }
            Debug.Log($"PASS: {count} scene and trajectory checks");
        }
    }
}
