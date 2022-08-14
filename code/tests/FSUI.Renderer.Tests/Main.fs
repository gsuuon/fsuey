module FSUI.Renderer.Tests

open Expecto

open FSUI.Types
open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Elements.Views
open FSUI.Renderer.Element

let pos = Root
let env = Env()

type Model =
    { name : string
    }

module TestView =
    let view model =
        div [] [
            text [] "Hi"
        ]

    let model =
        {
            name = "foo"
        }

    let rootElement = view model env pos

    [<Tests>]
    let tests =
        testList "View once" [
            testCase "container elements correct" <| fun _ ->
                let visualRoot = rootElement :?> VisualCollection
                match visualRoot.Children with
                | [el1] ->
                    let textElement = el1 :?> VisualText

                    Expect.equal textElement.Content $"Hi {model.name}" "Text content"
                | _ ->
                    Tests.failtest "Root contains wrong number of children"
        ]

module TestRenderer =
    open FSUI.Elements.Views

    type Msg =
        | Click

    let view model dispatch =
        div [] [] // TODO

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
