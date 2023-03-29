open FSharp.Data
open MtgStroyConverter.ArticleUtils

let parseBody (articleUrl: string) : string = $"this is dummy no text ({articleUrl})"

let extractArticle  (index: int) (chapter: string) (articleNode: HtmlNode): Article =
    let href = (articleNode.Descendants ["a"] |> Seq.head).AttributeValue("href")
    let subtitle = (articleNode.Descendants ["p"] |> Seq.item 1).InnerText()
    let body = parseBody href
    { index = index; subtitle = subtitle; chapter = chapter; body = body; url = href }

let parseChapterIndex  (startIndex: int) (chapter:string) (doc: HtmlDocument) : Article seq =
    let articlesSection = doc.Descendants ["section"]
                          |> Seq.filter (fun n -> n.HasClass("list-article"))
                          |> Seq.head
    let articlesList = articlesSection.Descendants ["ul"] |> Seq.head
    let articles = articlesList.Descendants ["li"] |> Seq.rev
    // TODO: remove "take 1"
    articles |> Seq.take 1 |> Seq.map (extractArticle startIndex chapter)

let createArticleIndex (url: string) (chapter: string): Article seq =
    let articleIndexHtml = HtmlDocument.Load(url)
    parseChapterIndex 0 chapter articleIndexHtml 

/// Create a Chapter object from a [li] HtmlNode in the index page.
let extractListItem (node: HtmlNode) : Chapter =
    let titleNode = node.Descendants ["h3"] |> Seq.head
    let title = titleNode.InnerText()
    let urlNode = node.Descendants [ "a" ] |> Seq.head
    let url = urlNode.AttributeValue("href")
    { title = title; url = url }

/// Return sequence of Chapters with parsing HtmlDocument.
let parseIndex (doc: HtmlDocument) : Chapter seq =
    let articlesUl = doc.Descendants ["ul"]
                     |> Seq.filter (fun ul -> ul.HasClass("backnumber-list-img"))
                     |> Seq.head
    let parts = articlesUl.Descendants ["li"] |> Seq.rev
    parts |> Seq.map extractListItem

/// Return sequence of Chapters from the url of index page.
let createIndex (url: string): Chapter seq =
    let indexHtml = HtmlDocument.Load(url)
    parseIndex indexHtml    

[<EntryPoint>]
let main _ =
    let index = (createIndex "https://mtg-jp.com/reading/ur/") |> List.ofSeq
    let test = index |> List.item 0
    let articles = createArticleIndex test.url test.title
    for art in articles do
        saveArticle art
    0