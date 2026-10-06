using System.Collections;
using System.IO;
using System.Linq;
using Game2Week.Core;
using Game2Week.Flow;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Game2Week.Tests
{
    public class StageSelectVisibilityTests
    {
        [SetUp] public void SetUp() => TestSave.Begin();
        [TearDown] public void TearDown() => TestSave.End();

        [UnityTest]
        public IEnumerator NewGame_StageListRendersWithoutCaptureRebuild()
        {
            SceneManager.LoadScene(SceneNames.MainMenu);
            yield return SceneFlowTests.WaitForScene(SceneNames.MainMenu);
            Object.FindAnyObjectByType<MainMenuController>().Choose(MainMenuController.NewGameIndex);
            yield return SceneFlowTests.WaitForScene(SceneNames.StageSelect);
            for (int frame = 0; frame < 5; frame++) yield return null;
            var select = Object.FindAnyObjectByType<StageSelectController>();
            Assert.AreEqual(1, select.Items.Count);
            var item = Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None)
                .Single(t => t.name == "Item_0");
            Debug.Log($"Stage item: text={item.text}; rect={item.rectTransform.rect}; vertices={item.textInfo.meshInfo[0].vertexCount}; culled={item.canvasRenderer.cull}; canvas={item.canvas.renderMode}");
            Assert.Greater(item.rectTransform.rect.width, 0);
            Assert.Greater(item.rectTransform.rect.height, 0);
            Assert.Greater(item.textInfo.meshInfo[0].vertexCount, 0);
            Assert.IsFalse(item.canvasRenderer.cull);
            // The list must stay below the title even in a short, wide Game view.
            var canvasRect = ((RectTransform)item.canvas.transform).rect;
            var listRect = (RectTransform)item.transform.parent;
            Assert.That(listRect.anchorMax.y, Is.LessThanOrEqualTo(0.75f));
            var corners = new Vector3[4];
            item.rectTransform.GetWorldCorners(corners);
            foreach (var corner in corners)
            {
                var point = item.canvas.transform.InverseTransformPoint(corner);
                Assert.That(point.x, Is.InRange(canvasRect.xMin, canvasRect.xMax));
                Assert.That(point.y, Is.InRange(canvasRect.yMin, canvasRect.yMax));
            }
            if (!Application.isBatchMode)
            {
                yield return new WaitForEndOfFrame();
                var screenshot = ScreenCapture.CaptureScreenshotAsTexture();
                Assert.IsNotNull(screenshot);
                File.WriteAllBytes(Path.Combine(Application.dataPath, "../Logs/stage_select_overlay.png"), screenshot.EncodeToPNG());
                Object.Destroy(screenshot);
            }
        }
    }
}
