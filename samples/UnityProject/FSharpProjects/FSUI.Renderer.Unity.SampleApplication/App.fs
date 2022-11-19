module FSUI.Renderer.Unity.SampleApplication.App

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




