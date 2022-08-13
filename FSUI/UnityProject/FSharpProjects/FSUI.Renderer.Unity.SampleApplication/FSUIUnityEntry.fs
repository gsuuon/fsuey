namespace FSUI.Renderer.Unity.SampleApplication.Entry

open UnityEngine
open UnityEngine.UIElements

open FSUI.Types
open FSUI.Elements.Views
open FSUI.Renderer.Unity

open FSUI.Renderer.Unity.Views
open FSUI.Renderer.Unity.Flow
open FSUI.Renderer.Unity.ScreenElement.Props
open FSUI.Renderer.Unity.WorldElement.Behaviors

open type FSUI.Renderer.Unity.WorldElement.Hooks.Props
open type FSUI.Renderer.Unity.SampleApplication.AppViews // just for poly

[<AutoOpen>]
module Util =
    let dlog x = Debug.Log x

    let printToUnity () = // TODO stringbuilder to not add newline on each write
        System.Console.SetOut
            { new System.IO.StringWriter() with
                member _.Write (msg: string) = Debug.Log msg
            }

module App =
    module ItemComponent =
        type ItemChoice =
            | ItemA
            | ItemB

        let selectItem render resolve =
            let itemButton item =
                button []
                    ( text [] (sprintf "Choose: %A" item)
                    , does ("select item", fun () ->
                        dlog <| sprintf "Clicked item %A" item
                        resolve item
                      )
                    )

            render
             <| div [] [
                    text [] "Pick a thing:"
                    itemButton ItemA
                    itemButton ItemB
                ]

    type Renders<'env, 'node> = ('env -> Position -> 'node) -> unit

    // Usage of certain elements requires us to pin our provider type to UnityProvider (prefab specifically)
    let main (render: Renders<UnityProvider, VisualElement>) =
        flow {
            render
             <| div [] [
                    text
                        [ Class "title"
                          Class "transitions"
                          Class "pre-show"
                        ]
                        "foo"
                    ]
            yield (WaitForEndOfFrame () )

            render
             <| div [] [
                    text
                        [ Class "title"
                          Class "transitions"
                        ]
                        "foo"
                     ]

            yield (WaitForSeconds 2.f)
            let! selectedItem = ItemComponent.selectItem render

            yield (WaitForEndOfFrame() :> YieldInstruction) // TODO why is this necessary here?
            render
             <| div [] [
                    at "tell" <| text [] (sprintf "You picked %A" selectedItem)
                    at "grats" <| text [] "good job!"
                ]
            yield (WaitForSeconds 2.f)
            render
             <| div [] [ // would expect items to stay in same positions
                         // no -- div re-adds the elements in their new order
                    at "grats" <| text [] "good job!!!!"
                    at "tell" <| text [] (sprintf "Your item %A" selectedItem)
                ]
            yield (WaitForSeconds 2.f)

            do! fun resolve ->
                render
                 <| div [] [
                        join [] [
                            prefab "Thingy"
                                [ on<Update> ("rotates up", fun g -> g.transform.Rotate(1f, 0f, 0f))
                                  on<Start>  <| fun _ -> dlog "Thingy -- start"
                                  effect     <| fun _ -> dlog "Rendered Thingy"
                                ]

                            gameObject 
                                [ on<Start> (fun _ -> dlog "test obj 1 -- start")
                                ]
                                "test obj 1"
                        ]
                        text [] (sprintf "The item: %A" selectedItem)
                        button []
                            ( poly "Confirm" // child
                            , does ("continue", fun () ->
                                  resolve ()
                                  dlog "clicked"
                              )
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
                            [ on<Start> (fun _ -> dlog "test obj 1 -- start") // this actually gets reattached here because the function type has a different name
                                                                              // since it's on a different line - i was hoping fsc would compile these as the same
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

        let render = Renderer.mount<UnityProvider> this.document

        App.main render
