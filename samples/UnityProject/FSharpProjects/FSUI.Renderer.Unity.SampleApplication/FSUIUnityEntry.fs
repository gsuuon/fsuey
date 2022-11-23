namespace FSUI.Renderer.Unity.SampleApplication.Entry

open UnityEngine
open UnityEngine.UIElements

open FSUI.Types
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.SampleApplication

type FSUIUnityEntry() =
    inherit MonoBehaviour()

    [<DefaultValue>]
    val mutable document : UIDocument
    member this.Awake () =
        this.document <- this.gameObject.AddComponent<UIDocument>()
        this.document.panelSettings <- Resources.Load<PanelSettings> "FSUIPanelSettings"

    member this.Start () =
        Util.printToUnity()

        let renderApp () =
            let mainComponent = 
                Renderer.mount<UnityProvider> this.document |> App.make

            mainComponent.Render()

        let renderButtonFix () =
            Renderer.mount<UnityProvider> this.document |> ButtonFix.make

        renderButtonFix()
