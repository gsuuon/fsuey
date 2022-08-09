module FSUI.Renderer.Unity.Views

open FSUI.Renderer.Element
open FSUI.Renderer.Unity
open FSUI.Renderer.Unity.WorldElement

let prefab name props =
    fun (env: UnityProvider) pos ->
        env.Prefab props name pos
