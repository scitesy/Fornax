#r "../../src/Fornax.Core/bin/Release/netstandard2.0/Fornax.Core.dll"
#r "System.Xml.Linq.dll"
#if !FORNAX
#load "../loaders/postloader.fsx"
#endif

open System
open System.Xml.Linq
open Html

let host = "https://www.example.com"

let generate' (ctx : SiteContents) (_:string) =
    let posts =
        ctx.TryGetValues<Postloader.Post> ()
        |> Option.defaultValue Seq.empty

    let xn = XName.Get
    let elem name (valu:string) = XElement(xn name, valu)
    let attr (name:string) (valu:string) = XAttribute(xn name, valu)
    let elems =
        posts
        |> Seq.toList
        |> List.sortBy(fun p -> p.published)
        |> List.map(fun p ->
                    XElement(xn "item",
                             elem "title" p.title,
                             elem "description" p.content,
                             elem "pubDate" (if p.published.IsSome then
                                               sprintf "%s %s" (p.published.Value.ToLongDateString()) (p.published.Value.ToLongTimeString())
                                             else
                                               ""),
                             elem "link" (sprintf "%s%s" host p.link),
                             elem "guid" (sprintf "%s%s" host p.link),
                             p.tags
                             |> List.map (fun t -> elem "category" t)
                            )
                   )
    let document =
        let atomUrl = "http://www.w3.org/2005/Atom"
        let atomXN = XNamespace.Xmlns + "atom"
        let atomAttr = (XAttribute(atomXN, atomUrl))
        XDocument(
            XDeclaration("1.0", "utf-8", "yes"),
                XElement(xn "rss",
                          atomAttr,
                          attr "version" "2.0",
                          XElement(xn "channel",
                                    elem "title" "blog",
                                    elem "description" "A Blog",
                                    elem "link" host,
                                    elem "language" "en-us",
                                    elem "generator" "Ionide Fornax",
                                    elems)
                         ) |> box)
    document.ToString()

let generate (ctx:SiteContents) (projectRoot:string) (page:string) =
    generate' ctx page
