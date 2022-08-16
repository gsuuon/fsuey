module FSUI.Renderer.Tests

open Expecto

open FSUI.Types
open FSUI.Renderer.Provider
open FSUI.Test.Provider
open FSUI.Test.Host
open FSUI.Elements.Views
open FSUI.Renderer.Element


type Model =
    { name : string
    }

let pos = Root
let env = Env()

module Utils =
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

module TestView =
    open Utils

    open Expecto.Expect

    [<Tests>]
    let scratch =
        test "basic" {
            that "it renders build items" <@ 1 + 2 = 1 @>
            // "render builds items" |> equal (1+2) 1
        }


    [<Tests>]
    let tests =
        let view model =
            div [] [
                text [] "Hi"
                text [] model.name
            ]

        let model =
            {
                name = "foo"
            }

        let rootElement = view model env pos

        let swap (provider: #IProvider) = provider.Cache.Swap()
        let children (v: Visual) = (v :?> Collection).Children

        let failsChildren elName =
            Tests.failtest (sprintf "%s contains wrong number or type of children" elName)

        testList "Basic view" [
            // FIXME tests should generally be independent or in a testSequenced
            test "first render" {
                match children rootElement with
                | [ :? Text as text1 
                    :? Text as text2
                  ] ->
                    "no mutations in text1" |> equal (mutations text1) 0
                    "no mutations in text2" |> equal (mutations text2) 0

                | _ ->
                    failsChildren "Root"
            }

            test "second render, no changes" {
                swap env
                match view model env pos |> children with
                | [ :? Text as text1
                    :? Text as text2
                  ] ->
                    "no mutations in text1" |> equal (mutations text1) 0
                    "no mutations in text2" |> equal (mutations text2) 0
                | _ ->
                    failsChildren "Root"

            }
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
