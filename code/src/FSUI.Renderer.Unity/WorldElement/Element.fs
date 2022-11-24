module FSUI.Renderer.Unity.WorldElement.Element

open UnityEngine
open FSUI.Renderer.Cache

let create newGameObject (swappers: Swappers) =
    let cache = swappers.Create GameObject.Destroy

    fun props name pos ->
        match cache.Stale.Remove pos : bool * _ with
        | true, (props', data', visual') ->
            let detachProps = Hooks.update props' props visual'
            cache.Fresh.Add (pos, (detachProps, name, visual'))
            visual'
        | false, _ ->
            let visual = newGameObject name
            let detachProps = visual |> Hooks.create props
            cache.Fresh.Add (pos, (detachProps, name, visual))
            visual
