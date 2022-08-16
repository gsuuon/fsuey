module FSUI.Renderer.Tests.TestingUtils

open System.Diagnostics
open Swensen.Unquote.Assertions
open Swensen.Unquote.Operators
open Expecto.Expect

open FSUI.Types
open FSUI.Test.Host
open FSUI.Renderer.Provider

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
