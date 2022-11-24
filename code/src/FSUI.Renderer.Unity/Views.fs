module FSUI.Renderer.Unity.Views

open FSUI.Types
open FSUI.Renderer.Unity

let prefab path name props children (env: UnityProvider) pos =
    let nodes =
        children |> List.mapi (fun idx fnode -> fnode env (Ordinal (pos, idx)))

    let cacheName = path + ":" + name

    env.Prefab path name props nodes (pos.Named cacheName)
