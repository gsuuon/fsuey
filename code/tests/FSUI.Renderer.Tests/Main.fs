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

    let layoutEquals render  =
        fun layout contents msg ->
            let collection = render layout
            let childContentMutationCounts = collection |> children |> List.map contentMutations
            equal childContentMutationCounts contents msg
    
    let mkRender env =
        fun layout ->
            let element = layout env Root
            (env :> IProvider).Cache.Swap()
            element

[<Tests>]
let tests =
    testList "renders" [
        test "single ordinal collection" {
            let rendersContent = Env() |> mkRender |> layoutEquals

            "content with no mutations" |> rendersContent
                (div [] [
                    text [] "foo"
                    text [] "bar"
                ])
                [ Content.Text "foo", 0
                  Content.Text "bar", 0
                ]

            "content is unchanged and no mutations" |> rendersContent
                (div [] [
                    text [] "foo"
                    text [] "bar"
                ])
                [ Content.Text "foo", 0
                  Content.Text "bar", 0 ]

            "first text changed" |> rendersContent
                (div [] [
                    text [] "foofoo"
                    text [] "bar"
                ])
                [ Content.Text "foofoo", 1
                  Content.Text "bar", 0 ]

            "second text changed" |> rendersContent
                (div [] [
                    text [] "foofoo"
                    text [] "barbar"
                ])
                [ Content.Text "foofoo", 1
                  Content.Text "barbar", 1 ]

            "text element added" |> rendersContent
                (div [] [
                    text [] "foofoo"
                    text [] "barbar"
                    text [] "baz"
                ])
                [ Content.Text "foofoo", 1
                  Content.Text "barbar", 1
                  Content.Text "baz"   , 0 ]

            "text element changed" |> rendersContent
                (div [] [
                    text [] "foofoo"
                    text [] "barbar"
                    text [] "bazbaz"
                ])
                [ Content.Text "foofoo", 1
                  Content.Text "barbar", 1
                  Content.Text "bazbaz", 1 ]

            "text element removed" |> rendersContent
                (div [] [
                    text [] "barbar"
                    text [] "bazbaz"
                ])
                [ Content.Text "barbar", 2
                  Content.Text "bazbaz", 2 ]
        }
    ]


[<EntryPoint>]
let main argv =
    Tests.runTestsInAssembly defaultConfig argv
