module FSUI.Renderer.Tests

open Expecto

open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Elements.Views
open FSUI.Renderer.Element

let pos = Root
let env = Env()

type Model =
    { name : string
      pic : string
    }

module TestView =
    let view model =
        div [
            text [] $"Hi {model.name}"
            image [] model.pic
        ] []

    let model =
        {
            name = "foo"
            pic = "foo.webp"
        }

    let rootElement = view model env pos

    [<Tests>]
    let tests =
        testList "View once" [
            testCase "container elements correct" <| fun _ ->
                let visualRoot = rootElement :?> VisualCollection
                match visualRoot.Children with
                | [el1; el2] ->
                    let textElement = el1 :?> VisualText
                    let imageElement = el2 :?> VisualImage

                    Expect.equal textElement.Content $"Hi {model.name}" "Text content"
                    Expect.equal imageElement.Path model.pic "Image path"
                | _ ->
                    Tests.failtest "Root contains wrong number of children"
        ]

module TestRenderer =
    open FSUI.Elements.Views

    type Msg =
        | Click

    let view model dispatch =
        div [
            // button []
            //     ( "Click"
            //     , fun _ -> dispatch Click
            //     )
        ]

[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
