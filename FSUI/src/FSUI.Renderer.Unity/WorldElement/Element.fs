module FSUI.Renderer.Unity.WorldElement.Element

open UnityEngine
open FSUI.Renderer.Cache
open FSUI.Renderer.Unity.WorldElement.Hooks

let create newGameObject (swappers: Swappers) =
    let cache = swappers.Create GameObject.Destroy
    fun props name pos ->
        let (exists, last) = cache.Stale.Remove pos
        if exists then
            let (props', data', visual') = last
            let detachProps = Hooks.update props' props visual'
            cache.Fresh.Add (pos, (detachProps, name, visual'))
            visual'
        else
            let visual = newGameObject name
            let detachProps = visual |> Hooks.create props
            cache.Fresh.Add (pos, (detachProps, name, visual))
            visual
