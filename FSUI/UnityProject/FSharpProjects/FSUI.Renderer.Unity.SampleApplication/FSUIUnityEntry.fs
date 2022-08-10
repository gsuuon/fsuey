namespace FSUI.Renderer.Unity.SampleApplication.Entry

open UnityEngine
open UnityEngine.UIElements

open FSUI.Elements.Views
open FSUI.Renderer.Element
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.Views

open FSUI.Renderer.Unity.WorldElement.Behaviors
open type FSUI.Renderer.Unity.WorldElement.Hooks.Props

open FSUI.Renderer.Unity.SampleApplication
open FSUI.Renderer.Unity.SampleApplication.Eventually

open type AppViews // just for poly

[<AutoOpen>]
module Util =
    type UnitySequence() =
        inherit Eventually.Computation.EventuallyBuilder() 
        member _.Run x = asEnumerator x

    let dlog x = Debug.Log x
    let unitySequence = UnitySequence()

    let printToUnity () = // TODO this seems to split messages up into multiple lines
        System.Console.SetOut
            { new System.IO.StringWriter() with
                member _.Write (msg: string) = Debug.Log msg
            }

module FUI =
    open FSUI.Renderer.Cache

    let inline swap provider =
        ( ^T : (member Cache : Swappers) (provider)).Swap()

    let inline mount (document: UIDocument) provider view =
        let render =
            let env = provider
            fun view ->
                view env Root |> document.rootVisualElement.Add
                swap provider

        view render

module ItemComponent =
    type ItemChoice =
        | ItemA
        | ItemB

    let selectItem render resolve =
        let itemButton item =
            button []
                ( text [] (sprintf "Choose: %A" item)
                , fun () ->
                    dlog <| sprintf "Clicked item %A" item
                    resolve item )

        render
         <| div [] [
                text [] "Pick a thing:"
                itemButton ItemA
                itemButton ItemB
            ]

module App =
    // Usage of certain elements requires us to pin our provider type to UnityProvider (prefab specifically)
    let main (render: (UnityProvider -> Position -> VisualElement) -> unit) =
        unitySequence {
            render <| div [] [ text [] "foo" ]
            yield (WaitForSeconds 1.5f)

            let! selectedItem = ItemComponent.selectItem render

            do! fun resolve ->
                render
                 <| div [] [
                        join [] [
                            prefab "Thingy"
                                [ on<Update> ("rotates up", fun g -> g.transform.Rotate(1f, 0f, 0f))
                                  on<Start> <| fun _ -> dlog "Thingy -- start"
                                  effect <| fun _ -> dlog "Rendered Thingy"
                                ]

                            gameObject 
                                [ on<Start> (fun _ -> dlog "test obj 1 -- start")
                                ]
                                "test obj 1"
                        ]
                        text [] (sprintf "The item: %A" selectedItem)
                        button []
                            ( poly "Confirm" // child
                            , fun () ->
                                  resolve ()
                                  dlog "clicked"
                            )
                    ] 

            render
             <| div [] [
                    join [] [
                        prefab "Thingy"
                            [ on<Update> ("rotates down", fun g -> g.transform.Rotate(-1f, 0f, 0f))
                              // The component holding the 'on<Start>' gets removed on this render
                            ]

                        gameObject // This game object doesn't get re-created
                            [ on<Start> (fun _ -> dlog "test obj 1 -- start") // okay, it actually does here because the function type has a different name
                                                                              // it's on a different line - i was hoping fsc would compile these as the same
                            ]
                            "test obj 1"
                    ]
                    text [] "You did it!"
                ]

            return "okay that's it"
        }

type FSUIUnityEntry() =
    inherit MonoBehaviour()

    [<DefaultValue>]
    val mutable document : UIDocument
    member this.Awake () =
        this.document <- this.gameObject.AddComponent<UIDocument>()
        this.document.panelSettings <- Resources.Load<PanelSettings> "FSUIPanelSettings"

    member this.Start () =
        printToUnity()

        FUI.mount this.document (UnityProvider()) App.main
