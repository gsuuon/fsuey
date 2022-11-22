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

[<AutoOpen>]
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
module LayoutModel =
    open GameModel

    type Msg =
        | IncreaseHP of ItemKey
        
    type Main =
        | Items
        | Item of Item

open type Elements<ScreenProp>

open FSUI.Make.LayoutStoreView

let viewDetail (vm: View<_,_,_>) =
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

let viewMain (v: View<_,_,_>) =
    function
    | Items ->
        let xs =
            v.State.items |>
                Seq.map ( fun (KeyValue(ItemKey(key), item) ) ->
                    button (item.name, key) <| fun _ ->
                        Item item |> v.Layout
                )
        div [Class "foo"] ( xs |> Seq.toList )
    | Item item ->
        div [
            text item.name
            text $"hp: {item.hp}"
            viewDetail v item.detail
            button "back" <| fun _ -> v.Layout Items
        ]

let initialModel : World =
    {
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

let make renderer =
    let store =
        mkStoreByIngest <| fun msg update ->
            match msg with
            | IncreaseHP key ->
                update <| fun world ->
                    if Map.containsKey key world.items then
                        { world with
                            items = world.items |> Map.change key (
                                function
                                | Some item -> Some { item with hp = item.hp + 5 }
                                | None -> None
                            )
                        }
                         |> Update
                    else
                        NoUpdate

    make Items (store initialModel) viewMain renderer
