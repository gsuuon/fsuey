module FSUI.Renderer.Unity.SampleApplication.ButtonFix


open System
open FSUI.Types
open FSUI.Elements.Views
open FSUI.Elements.Interfaces
open FSUI.Make.LayoutStoreView
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.Views


type ButtonView =
    static member inline button (x: string) =
        fun action -> button [] ( text [] x, Keyed (x, Action action) )

    // static member inline button (x, key) =
    //     fun action -> button [] ( text [] x, Keyed (string key, action ) )
    //         // TODO key is converted to string because we can't be generic on the IButton interface
    // static member inline button (x: Renders<_,_>, key) =
    //     fun action -> button [] ( x, Keyed (key, action ) )
    
open type Elements<ScreenProp>
open type ButtonView

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
