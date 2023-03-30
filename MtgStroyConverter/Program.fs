open System
open FSharp.Data
open MtgStroyConverter.ArticleUtils

let parseBody (articleUrl: string) : string =
    let doc = loadHtml articleUrl
    let bodyNode = doc.Descendants ["div"] |> Seq.filter (fun n -> n.HasClass "detail") |> Seq.head
    let paragraphs = bodyNode.Descendants ["p"] |> List.ofSeq
    let paraList = paragraphs |> List.map (fun p -> p.ToString()) |> List.map extractParagraph
    let body = String.Join("\r\n", paraList)
    body

let extractArticle (chapter: string) (articleNode: HtmlNode): Article =
    let href = (articleNode.Descendants ["a"] |> Seq.head).AttributeValue("href")
    let subtitle = (articleNode.Descendants ["p"] |> Seq.item 1).InnerText()
    Console.WriteLine $"extract {subtitle}..."
    let body = parseBody href
    Console.WriteLine " => OK."
    { index = 0; subtitle = subtitle; chapter = chapter; body = body; url = href }

let parseChapterIndex (chapter:string) (doc: HtmlDocument) : Article list =
    let articlesSection = doc.Descendants ["section"]
                          |> Seq.filter (fun n -> n.HasClass "list-article")
                          |> Seq.head
    let articlesList = articlesSection.Descendants ["ul"] |> Seq.head
    let articles = articlesList.Descendants ["li"] |> List.ofSeq |> List.rev
    articles |> List.map (extractArticle chapter)

let createArticleIndex (url: string) (chapter: string): Article list =
    Console.WriteLine $"Loading articles in {chapter}..."
    let articleIndexHtml = loadHtml url
    parseChapterIndex chapter articleIndexHtml 

/// Create a Chapter object from a [li] HtmlNode in the index page.
let extractListItem (node: HtmlNode) : Chapter =
    let titleNode = node.Descendants ["h3"] |> Seq.head
    let title = titleNode.InnerText()
    let urlNode = node.Descendants [ "a" ] |> Seq.head
    let url = urlNode.AttributeValue "href"
    { title = title; url = url }

/// Return sequence of Chapters with parsing HtmlDocument.
let parseIndex (doc: HtmlDocument) : Chapter list =
    Console.Write "Loading chapters..."
    let articlesUl = doc.Descendants ["ul"]
                     |> Seq.filter (fun ul -> ul.HasClass "backnumber-list-img")
                     |> Seq.head
    let parts = articlesUl.Descendants ["li"] |> List.ofSeq |> List.rev
    let index = parts |> List.map extractListItem
    Console.WriteLine $"{Seq.length index} chapter(s) loaded."
    for part in index do
        Console.WriteLine $" {part.title} => {part.url}"
    index

/// Return sequence of Chapters from the url of index page.
let createIndex (url: string): Chapter list =
    let indexHtml = loadHtml url
    parseIndex indexHtml    

[<EntryPoint>]
let main _ =
    Console.WriteLine "Load index..."
    let index = (createIndex indexUrl) |> List.ofSeq
    let articles = index |> List.collect (fun c -> createArticleIndex c.url c.title |> List.ofSeq)
    let list = List.zip articles [1..articles.Length]

    let mutable currentChapter = ""
    
    for (art, index) in list do
        art.index <- index
        
        if art.chapter <> currentChapter then
            currentChapter <- art.chapter
        else
            art.chapter <- "''"
        
        saveArticle art
        
    Console.WriteLine "Output toc.yml..."
    saveTocYaml articles
    Console.WriteLine "done."
    0