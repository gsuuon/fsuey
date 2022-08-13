using System.Collections;

using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

public class UITestBehaviour : MonoBehaviour, IMonoBehaviourTest
{
    bool isFinished = false;
    public bool IsTestFinished => isFinished;

    public UIDocument document;

    public void Awake() {
        document = gameObject.AddComponent<UIDocument>();
        document.panelSettings = Resources.Load<PanelSettings>("PanelSettings");
    }

    public void BuildVisualTree() {
        var el = new VisualElement();

        el.Add(new Label("Hi"));
        el.Add(new Label("world"));

        document.rootVisualElement.Add(el);
        // NOTE this doesn't include the repaint which would occur next frame

        isFinished = true;
    }
}

