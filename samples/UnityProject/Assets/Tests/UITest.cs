using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using Unity.PerformanceTesting;

public class UITest
{
    [Test, Performance]
    public void UITestBehaviour()
    {
        var go = new GameObject();
        var testBehaviour = go.AddComponent<UITestBehaviour>();
        go.SetActive(true);

        Measure.Method( () => testBehaviour.BuildVisualTree() ).GC().Run();
    }
}
