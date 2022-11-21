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

type ViewMethods<'View, 'State> =
    { update : 'View -> unit
      state : 'State
    }

open GameModel

open type Elements<ScreenProp>

let viewDetail vm =
    function
    | Foo foo ->
        div [
            join [ prefab "item/foo" [] ]
            text $"Color: {foo.color}"
            text $"Smell: {foo.smell}"
        ]
    | Bar bar ->
        div [
            join [ prefab "item/bar" [] ]
            text $"Size: {bar.size}"
            text $"Description: {bar.description}"
        ]

open ViewModel
// Usage of certain elements requires us to pin our provider type to UnityProvider (prefab specifically)

let viewMain vm =
    function
    | Items ->
        let xs =
            vm.state.items |>
                Seq.map ( fun (KeyValue(ItemKey(key), item) ) ->
                    button (item.name, key) <| fun _ ->
                        Item item |> vm.update
                )
        div [Class "foo"] ( xs |> Seq.toList )
    | Item item ->
        div [
            text item.name
            text $"hp: {item.hp}"
            viewDetail vm item.detail
            button "back" <| fun _ -> vm.update Items
        ]

let mkRun view initModel initState render =
    let mutable state = initState

    let rec vm = {
        update = fun viewModel ->
            view vm viewModel |> render

        state = state
    }

    view vm initModel |> render

type Entry<'provider, 'visual> = (Renders<'provider, 'visual> -> unit) -> unit

let runMain : Entry<UnityProvider, ScreenElement> =
    mkRun viewMain Items {
        items = Map [
            ItemKey 0, {
                name = "apple"
                hp = 100
                detail = Foo {
                    color = "red"
                    smell = "nice"
                }
            }
            ItemKey 1, {
                name = "bar"
                hp = 90
                detail = Bar {
                    size = Big
                    description = "a big bar"
                }
            }
            ItemKey 2, {
                name = "smol bar"
                hp = 20
                detail = Bar {
                    size = Small
                    description = "a small bar"
                }
            }
        ]
    }
