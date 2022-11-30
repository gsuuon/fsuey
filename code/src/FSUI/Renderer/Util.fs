module FSUI.Renderer.Util

open FSUI.Renderer.Cache

let inline gate pred fn v =
    if pred then fn v else v

let inline gateOpt opt fn v =
    match opt with
    | Some x -> fn x v
    | None -> v

let inline save (cache: Swapper<_,_,_,_>) props data pos visual =
    try
        cache.Fresh.Add (pos, (props, data, visual) )
    with
    | :? System.ArgumentException -> // TODO Is an element existing in Fresh at this position an error?
        eprintfn "Fresh cache already contained an element at %s" (string pos)
    visual
