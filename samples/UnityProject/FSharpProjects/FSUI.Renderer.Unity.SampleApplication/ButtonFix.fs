module FSUI.Renderer.Unity.SampleApplication.ButtonFix


open System
open FSUI.Types
open FSUI.Elements.Views
open FSUI.Elements.Interfaces
open FSUI.Renderer.Unity
open FSUI.Make.LayoutStoreView

let show x = ()
let noop _ = ()

let but word =
    (fun (e: #IButton<ScreenProp, ScreenElement * Keyed<string, System.Action>, ScreenElement>) pos ->
        let child = text [] word e (Ordinal (pos, 0))

        e.Button [] (child, Keyed(word, Action (fun _ -> printfn $"clicked {word}") ) ) pos
    )

let make render =
    async {
        render
         <| div [] [
                but "a"
                but "b"
            ]
        do! Async.Sleep 1000
        render
         <| div [] [
                but "c"
                but "d"
            ]
    } |> Async.StartImmediate

