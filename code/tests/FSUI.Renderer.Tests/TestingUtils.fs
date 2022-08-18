module FSUI.Renderer.Tests.TestingUtils

open System.Diagnostics
open Swensen.Unquote.Assertions
open Swensen.Unquote.Operators
open Expecto.Expect

open FSUI.Types
open FSUI.Test.Host

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

[<StackTraceHidden>]
let layoutEquals getContent render  =
    fun msg layout expectedContents ->
        let (renderedLayout: Visual) = render layout
        equal (getContent renderedLayout) expectedContents msg

let mkRender env =
    fun layout ->
        let visual = layout env Root
        (env :> IProvider).ProviderState.Tick()
        visual
