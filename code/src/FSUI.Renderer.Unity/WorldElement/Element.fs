module FSUI.Renderer.Unity.WorldElement.Element

open UnityEngine
open FSUI.Renderer.Cache
open FSUI.Renderer.Unity

// TODO dedup with prefab
let create newGameObject (swappers: Swappers) =
    let cache = swappers.Create GameObject.Destroy

    fun name props children pos ->
        match cache.Stale.Remove pos : bool * _ with
        | true, (props', data', visual') ->
            let detachProps = Hooks.update props' props visual'
            cache.Fresh.Add (pos, (detachProps, children, visual'))
            Graph.addChildren (children, visual')
        | false, _ ->
            let visual : GameObject = newGameObject name
            visual.SetActive false
            let detachProps = visual |> Hooks.create props
            cache.Fresh.Add (pos, (detachProps, children, visual))
            Graph.addChildren (children, visual)
