namespace FSUI.Difference

open System.Collections.Generic

module Difference =
    type Tag =
        | Last = 0
        | Next = 1
        | Both = 2

    type Changes<'T> =
        { removed : array<'T>      
          created : array<'T>
          // content : array<'T>
        }
        static member Empty =
            { removed = Array.empty
              created = Array.empty
            }

    let inline empty< ^a when 'a : (static member Empty : 'a)> () = (^a : (static member Empty : 'a) ())
    let inline length x = (^a : (member Length : int) x)
    let inline compute< ^a, 'b
                            when 'a : (member Length : int)
                             and 'a :> seq<'b>
                             and 'b : equality >
        (last: 'a)
        (next: 'a)
            : Option<Changes<'b> >
        =
        let count = length next + length last
        let tags = Dictionary count

        let mutable removedCount = 0
        let mutable createdCount = 0
        // let mutable contentCount = 0

        for item in last do
            if tags.TryAdd(item, Tag.Last) then
                removedCount <- removedCount + 1

        for item in next do
            match tags.TryGetValue item with
            | true, x ->
                if x = Tag.Last then
                    tags[item] <- Tag.Both
                    removedCount <- removedCount - 1
                    // contentCount <- contentCount + 1
            | _ ->
                tags[item] <- Tag.Next
                createdCount <- createdCount + 1

        if removedCount = 0 && createdCount = 0 then
            None
        else
            let mutable removed = Array.zeroCreate removedCount
            let mutable created = Array.zeroCreate createdCount
            // let mutable content = Array.zeroCreate contentCount

            // Repurpose counts as indices
            removedCount <- 0
            createdCount <- 0
            // contentCount <- 0

            for item in tags do
                if item.Value = Tag.Last then
                    removed[removedCount] <- item.Key
                    removedCount <- removedCount + 1
                elif item.Value = Tag.Next then
                    created[createdCount] <- item.Key
                    createdCount <- createdCount + 1
                // else
                //     content[contentCount] <- item.Key
                //     contentCount <- contentCount + 1

            Some {
                removed = removed
                created = created
                // content = content
            }

type Compute =
    static member difference (last: 'T array, next: 'T array) =
        Difference.compute last next
    static member difference (last: 'T list, next: 'T list) =
        Difference.compute last next
