module FSUI.Renderer.Tests

open Expecto
open Expecto.Expect

open FSUI.Types
open FSUI.Renderer.Provider
open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Elements.Views
open FSUI.Renderer.Element


[<AutoOpen>]
module internal Utils =
    open System
    open System.Diagnostics
    open Swensen.Unquote.Assertions
    open Swensen.Unquote.Operators

    let chopLast (xs: 'a list) =
        xs[..xs.Length - 2]

    [<StackTraceHidden>]
    let that msg expr =
        try 
            test expr
        with
        | _ ->
            let reduction = // TODO colors?
                expr
                 |> reduceFully
                 |> chopLast
                 |> List.map decompile

            raise <| Expecto.AssertException (msg :: reduction |> String.concat "\n")
            
    let mutations (x: Visual) = x.MutationLog.Length

    let contentMutations<'T when 'T :> Visual> (x: 'T) =
        x.Content, x.MutationLog.Length

    let children (v: Visual) = (v :?> Collection).Children

    [<StackTraceHidden>]
    let layoutEquals getContent render  =
        fun msg layout contents ->
            let (renderedContents: Visual) = render layout
            equal (getContent renderedContents) contents msg
    
    let mkRender env =
        fun layout ->
            let element = layout env Root
            (env :> IProvider).Cache.Swap()
            element

open Content

[<Tests>]
let tests =
    testList "renders" [
        test "single ordinal text collection" {
            let rendersContent = Env() |> mkRender |> layoutEquals getContentAddChanges

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

            rendersContent "text element removed"
             <| div [] [
                    text [] "barbar"
                    text [] "bazbaz"
                ]
             <| div' 2 [
                    text' 2 "barbar"
                    text' 2 "bazbaz"
                ]
        }

        test "single mixed text collection" {
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

        test "nested collections" {
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
        }
    ]


[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
