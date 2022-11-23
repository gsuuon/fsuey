module FSUI.Renderer.Unity.SampleApplication.App

open System
open UnityEngine
open UnityEngine.UIElements

open FSUI.Types
open FSUI.Elements.Views
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.Views
open FSUI.Renderer.Unity.Flow
open FSUI.Renderer.Unity.ScreenElement.Props
open FSUI.Renderer.Unity.WorldElement.Behaviors
open FSUI.Make.LayoutStoreView

open type FSUI.Renderer.Unity.WorldElement.Hooks.Props
open type FSUI.Renderer.Unity.SampleApplication.AppViews // just for poly

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
        { items : Map<ItemKey, Item>
          tick : int
        }

[<AutoOpen>]
module LayoutModel =
    open GameModel

    type Msg =
        | IncreaseHP of ItemKey
        
    type Main =
        | Items
        | Item of ItemKey

open type Elements<ScreenProp>

let showDetail =
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

let showMain (v: View<_,_,_>) =
    function
    | Items ->
        let showItems =
            v.State.items
             |> Seq.map ( fun ( KeyValue(itemKey, item) ) ->
                    button item.name <| fun _ ->
                        printfn $"> show {itemKey}"
                        Item itemKey |> v.Layout
                )
             |> Seq.toList

        div [Class "foo"] 
            [
                yield text v.State.tick
                yield! showItems
            ]
    | Item itemKey ->
        match v.State.items.TryFind itemKey with
        | Some item ->
            div [
                text item.name
                text $"hp: {item.hp}"
                showDetail item.detail
                button "repair" <| fun _ ->
                        printfn $"> repair {itemKey}"
                        IncreaseHP itemKey |> v.Dispatch
                button "back" <| fun _ ->
                    printfn "> back"
                    v.Layout Items
            ]
        | None ->
            div [
                text "No item"
                button "back" <| fun _ ->
                    printfn "> back None"
                    v.Layout Items
            ]

let initialModel : World =
    {
        tick = 0
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

let updateStore update =
    function
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

let noop _ = ()
let initialize update =
    async {
        while true do
            do! Async.Sleep 1000

            try
                update <| fun world ->
                    Update
                        { world with
                            tick = world.tick + 1
                            items = world.items |> Map.map (fun key item ->
                                { item with hp = item.hp - 1}
                            )
                        }
            with
            | e -> Debug.LogError e.Message
    } |> Async.StartImmediate

let make renderer =
    let store = mkStoreByIngest initialize updateStore initialModel

    let render x =
        printfn "<<rendering>>"
        try
            renderer x
        with
        | e -> printfn $"Error: {e.Message}"

    make Items store showMain render
