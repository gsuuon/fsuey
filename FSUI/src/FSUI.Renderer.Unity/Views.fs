module FSUI.Renderer.Unity.Views

open FSUI.Renderer.Unity

let prefab name props =
    fun (env: UnityProvider) pos ->
        env.Prefab props name pos
