module FSUI.Renderer.Unity.SampleApplication.ButtonFix


open System
open FSUI.Types
open FSUI.Elements.Views
open FSUI.Elements.Interfaces
open FSUI.Make.LayoutStoreView
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.Views


open type Elements<ScreenProp>
// open type ButtonElement

let show x = ()
let noop _ = ()

let make render =
    let rec setA () =
        div [
            div [ text "A" ]
            button "Aa" <| fun _ ->
                printfn "clicked Aa"
                render (setA())

            button "Ab" <| fun _ ->
                printfn "clicked Ab"
                render (setB())
        ]

    and setB () =
        div [
            div [ text "B" ]
            button "Ba" <| fun _ ->
                printfn "clicked Ba"
                render (setA())

            button "Bb" <| fun _ ->
                printfn "clicked Bb"
                render (setB())
        ]

    render (setA () )
