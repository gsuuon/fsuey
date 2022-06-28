namespace Scratch.ObjectExpression

module TypeScratch2 =
    module StaticGeneric =
        type IFoo =
            abstract member Do : int -> Unit

        type MyThing<'A>() =
            static member foo () = Unchecked.defaultof<'A>
            interface IFoo with
                member _.Do x = ()

        open type MyThing<int> // whaaaat

        let x = foo()

