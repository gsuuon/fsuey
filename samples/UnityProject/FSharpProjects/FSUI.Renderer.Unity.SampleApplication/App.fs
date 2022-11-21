module FSUI.Renderer.Unity.SampleApplication.App

open UnityEngine
open UnityEngine.UIElements

open FSUI.Types
open FSUI.Elements.Views
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.Views
open FSUI.Renderer.Unity.Flow
open FSUI.Renderer.Unity.ScreenElement.Props
open FSUI.Renderer.Unity.WorldElement.Behaviors

open type FSUI.Renderer.Unity.WorldElement.Hooks.Props
open type FSUI.Renderer.Unity.SampleApplication.AppViews // just for poly

[<AutoOpen>]
module Util =
    let dlog x = Debug.Log x

    let printToUnity () = // TODO stringbuilder to not add newline on each write
        System.Console.SetOut
            { new System.IO.StringWriter() with
                member _.Write (msg: string) = Debug.Log msg
            }

module ItemComponent =
    type ItemChoice =
        | ItemA
        | ItemB

    let selectItem render resolve =
        let itemButton item =
            button []
                ( text [] (sprintf "Choose: %A" item)
                , does ("select item", fun () ->
                    dlog <| sprintf "Clicked item %A" item
                    resolve item
                  )
                )

        render
         <| div [] [
                text [] "Pick a thing:"
                itemButton ItemA
                itemButton ItemB
            ]

module GameModel =
    type FooItem =
        { color : string
          smell : string
        }

    type BarItemSize =
        | Small
        | Big

    type BarItem =
        { size : BarItemSize
          description : string
        }
         
    type ItemDetail =
        | Foo of FooItem
        | Bar of BarItem

    type Item =
        { name : string
          detail : ItemDetail
          hp : int
        }

    type ItemKey = ItemKey of int

    type World =
        { items : Map<ItemKey, Item> }

[<AutoOpen>]
module ViewModel =
    open GameModel

    type Main =
        | Items
        | Item of Item

type Renders<'env, 'node> = ('env -> Position -> 'node) -> unit

type ViewMethods<'View, 'State> =
    { update : 'View -> unit
      state : 'State
    }

open GameModel
let viewDetail vm =
    function
    | Foo foo ->
        div [] [
            text [] $"Color: {foo.color}"
            text [] $"Smell: {foo.smell}"
            button [] ( text [] "back", fun _ ->
                vm.update Items
            )
        ]
    | Bar bar ->
        div [] [
            text [] $"Size: {bar.size}"
            text [] $"Description: {bar.description}"
        ]

open ViewModel
// Usage of certain elements requires us to pin our provider type to UnityProvider (prefab specifically)
let main vm =
    function
    | Items ->
        let xs =
            vm.state.items |>
                Seq.map ( fun (KeyValue(ItemKey(key), item) ) ->
                    button []
                        ( text [] item.name
                        , does (string key, fun _ -> Item item |> vm.update
                        ) )
                )
        div [] ( xs |> Seq.toList )
    | Item item ->
        div [] [
            text [] item.name
            viewDetail vm item.detail
        ]
