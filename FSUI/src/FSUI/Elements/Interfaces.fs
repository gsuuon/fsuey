namespace FSUI.Elements.Interfaces

open FSUI.Renderer.Element

type IContainer<'props, 'visual, 'node> =
    abstract member Container: Element<'node list, 'props, 'visual, 'node>

type IText<'props, 'visual, 'node> =
    abstract member Text: Element<string, 'props, 'visual, 'node>

type IImage<'props, 'visual, 'node> =
    abstract member Image: Element<string, 'props, 'visual, 'node>
