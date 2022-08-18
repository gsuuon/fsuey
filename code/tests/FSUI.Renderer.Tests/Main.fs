module FSUI.Renderer.Tests.Main

open Expecto
open Expecto.Expect

open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Test.Host.Content
open FSUI.Elements.Views

open FSUI.Renderer.Tests.TestingUtils


[<Tests>]
let testsData =
    testList "renders data" [
        test "single ordinal text collection" {
            let env = Env()
            let rendersContent = env |> mkRender |> layoutEquals getContentAddChanges

            rendersContent "with no mutations"
             <| div [] [
                    text [] "foo"
                    text [] "bar"
                ]
             <| div' 0 [
                    text' 0 "foo"
                    text' 0 "bar"
                ]

            rendersContent "content is unchanged and no mutations"
             <| div [] [
                    text [] "foo"
                    text [] "bar"
                ]
             <| div' 0 [ 
                    text' 0 "foo"
                    text' 0 "bar"
                ]

            "No new divs or texts were created" |> equal
                [ env.NewContainers
                  env.NewTexts ]
                [ 1
                  2 ]

            rendersContent "first text changed"
             <| div [] [
                    text [] "foofoo"
                    text [] "bar"
                ]
             <| div' 0 [
                    text' 1 "foofoo"
                    text' 0 "bar"
                ]

            rendersContent "second text changed"
             <| div [] [
                    text [] "foofoo"
                    text [] "barbar"
                ]
             <| div' 0 [
                    text' 1 "foofoo"
                    text' 1 "barbar"
                ]

            "No new divs or texts were created" |> equal
                [ env.NewContainers
                  env.NewTexts ]
                [ 1
                  2 ]

            rendersContent "text element added"
             <| div [] [
                    text [] "foofoo"
                    text [] "barbar"
                    text [] "baz"
                ]
             <| div' 1 [
                    text' 1 "foofoo"
                    text' 1 "barbar"
                    text' 0 "baz"   
                ]

            "One new text was created" |> equal
                [ env.NewContainers
                  env.NewTexts ]
                [ 1
                  3 ]

            rendersContent "text element changed"
             <| div [] [
                    text [] "foofoo"
                    text [] "barbar"
                    text [] "bazbaz"
                ]
             <| div' 1 [
                    text' 1 "foofoo"
                    text' 1 "barbar"
                    text' 1 "bazbaz"
                ]

            "No new divs or texts were created" |> equal
                [ env.NewContainers
                  env.NewTexts ]
                [ 1
                  3 ]

            rendersContent "text element removed"
             <| div [] [
                    text [] "barbar"
                    text [] "bazbaz"
                ]
             <| div' 2 [
                    text' 2 "barbar"
                    text' 2 "bazbaz"
                ]

            "No new divs or texts were created" |> equal
                [ env.NewContainers
                  env.NewTexts ]
                [ 1
                  3 ]
        }

        test "single ordinal and nominal text collection" {
            let rendersContent = Env() |> mkRender |> layoutEquals getContentAddChanges

            rendersContent "with string key"
             <| div [] [
                    at "foo" (text [] "foo")
                    text [] "bar"
                ]
             <| div' 0 [
                    text' 0 "foo"
                    text' 0 "bar"
                ]

            rendersContent "reordered string key changes insertion order but doesn't mutate"
             <| div [] [
                    text [] "bar"
                    at "foo"  <| text [] "foo"
                ]
             <| div' 1 [
                  text' 0 "bar"
                  text' 0 "foo"
                ]

            rendersContent "changing string keyed element and reordered"
             <| div [] [
                    at "foo" (text [] "foofoo")
                    text [] "bar"
                ]
             <| div' 2 [
                  text' 1 "foofoo"
                  text' 0 "bar"
                ]

            rendersContent "mutating keyed element when re-ordered"
             <| div [] [
                    text [] "bar"
                    at "foo" (text [] "foofoofoo")
                ]
             <| div' 3 [
                    text' 0 "bar"
                    text' 2 "foofoofoo"
                ]
        }

        test "nested collections of text" {
            let rendersContent = Env() |> mkRender |> layoutEquals getContentAddChanges
            
            rendersContent "with one nested text"
             <| div [] [
                    div [] [
                        text [] "foo"
                    ]
                ]
             <| div' 0 [
                    div' 0 [
                        text' 0 "foo"
                    ]
                ]

            rendersContent "changing nested text"
             <| div [] [
                    div [] [
                        text [] "foofoo"
                    ]
                ]
             <| div' 0 [
                    div' 0 [
                        text' 1 "foofoo"
                    ]
                ]
        }

        test "single ordinal mixed collection" {
            let rendersContent = Env() |> mkRender |> layoutEquals getContentAddChanges

            let click = fun () -> ()

            rendersContent "with different elements"
             <| div [] [
                    text [] "foo"
                    button []
                        ( text [] "click"
                        , does ("click", click)
                        )
                ]
             <| div' 0 [
                    text' 0 "foo"
                    button' 0
                        ( text' 0 "click"
                        , click
                        )
                ]

            let click2 = fun () -> ()

            rendersContent "updating button handler without updating key makes no changes"
             <| div [] [
                    text [] "foo"
                    button []
                        ( text [] "click"
                        , does ("click", click2)
                        )
                ]
             <| div' 0 [
                    text' 0 "foo"
                    button' 0
                        ( text' 0 "click"
                        , click
                        )
                ]

            rendersContent "updating button handler key updates the handler"
             <| div [] [
                    text [] "foo"
                    button []
                        ( text [] "click"
                        , does ("click2", click2)
                        )
                ]
             <| div' 0 [
                    text' 0 "foo"
                    button' 1
                        ( text' 0 "click"
                        , click2
                        )
                ]

            rendersContent "updating button child"
             <| div [] [
                    text [] "foo"
                    button []
                        ( text [] "click here"
                        , does ("click", click2)
                        )
                ]
             <| div' 0 [
                    text' 0 "foo"
                    button' 2
                        ( text' 1 "click here"
                        , click2
                        )
                ]
        }
    ]

[<Tests>]
let testsProps =
    testList "renders props" [
        test "single text element" {
            let rendersContent = Env() |> mkRender |> layoutEquals getContentAddClasses

            rendersContent "with props"
             <| text [Class "foo"] "bird"
             <| text' (Set ["foo"]) "bird"

            rendersContent "changing props"
             <| text [Class "bar"] "bird"
             <| text' (Set ["bar"]) "bird"

            rendersContent "adding props"
             <| text [Class "bar"; Class "baz"] "bird"
             <| text' (Set ["bar"; "baz"]) "bird"
        }

        test "single text element prop mutations" {
            let rendersContent = Env() |> mkRender |> layoutEquals (getContentAdd (fun v -> v.MutationLog, v.ClassNames ) )

            rendersContent "with props"
             <| text [Class "foo"] "bird"
             <| text'
                  ([AddClass "foo"]
                  , Set ["foo"])
                  "bird"

            rendersContent "changing props"
             <| text [Class "bar"] "bird"
             <| text'
                  ([AddClass "foo"
                    RemoveClass "foo"
                    AddClass "bar"]
                  , Set ["bar"])
                  "bird"

            rendersContent "adding props"
             <| text [Class "bar"; Class "baz"] "bird"
             <| text'
                  ([AddClass "foo"
                    RemoveClass "foo"
                    AddClass "bar"
                    AddClass "baz"]
                  , Set ["bar"; "baz"])
                  "bird"
        }
    ]


[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
