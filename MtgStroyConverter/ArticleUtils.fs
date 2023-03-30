module MtgStroyConverter.ArticleUtils
open System
open System.IO
open System.Text
open System.Text.RegularExpressions
open System.Threading
open FSharp.Data

type Chapter = { title: string; url: string }
type Article = {
    mutable index: int 
    mutable chapter: string 
    subtitle: string
    body: string
    url: string
}

let printChapter chapter =
    Console.WriteLine $"{{ title: {chapter.title}, url: {chapter.url} }}"
    
let currentTime = "2023/03/07 12:00:00"
let urlRoot = "https://mtg-jp.com/reading/ur/"
let indexUrl = "https://mtg-jp.com/reading/ur/"
let rawDirectory = "./output/raw/"
let bodyDirectory = "./output/本文"
let tocFileName = "./output/toc.yaml"
let regexs = [
   (Regex("<a.*?>"), "");
   (Regex("</a>"), "")
]

let createArticleYaml (article: Article): string  =
   let builder = StringBuilder()
   builder.Append("---\r\n")
       .Append($"index: \"{article.index}\"\r\n")
       .Append($"href: \"https://ncode.syosetu.com/n7777gg/\"\r\n")
       .Append($"chapter: {article.chapter}\r\n")
       .Append($"subchapter: \"\"\r\n")
       .Append($"subtitle: {article.subtitle}\r\n")
       .Append($"file_subtitle: {article.subtitle}\r\n")
       .Append($"subdate: \"{currentTime}\"\r\n")
       .Append($"subupdate: \"{currentTime}\"\r\n")
       .Append($"element:\r\n")
       .Append($"  data_type: html\r\n")
       .Append($"  introduction: \"\"\r\n")
       .Append($"  postscript: \"\"\r\n")
       .Append($"  body: |-\r\n")
   |> ignore
   let filtered = article.body.Split "\r\n"
   for line in filtered do
     builder.Append $"    {line}\r\n" |> ignore
   builder.ToString()
         
let saveArticle (article: Article): unit =
    let baseDir = bodyDirectory
    let name = $"{baseDir}/{article.index} {article.subtitle}.yaml"
    if File.Exists name then File.Delete name
    if not (Directory.Exists baseDir) then Directory.CreateDirectory baseDir |> ignore
    let file = new StreamWriter(name)
    file.Write (createArticleYaml article)
    file.Dispose()
    ()
    
let saveTocYaml (articles: Article list) : unit =
    if File.Exists tocFileName then File.Delete tocFileName
    use writer = new StreamWriter(tocFileName)
    writer.WriteLine "---"
    writer.WriteLine "title: MTG 背景世界ストーリー"
    writer.WriteLine "author: Wizards of the Coast"
    writer.WriteLine "toc_url: https://ncode.syosetu.com/n7777gg/"
    writer.WriteLine "story: something something something..."
    writer.WriteLine "subtitles:"
    
    for art in articles do
        writer.WriteLine $"- index: '{art.index}'"
        writer.WriteLine $"  href: /n7777gg/{art.index}/"
        writer.WriteLine $"  chapter: {art.chapter}"
        writer.WriteLine $"  subchapter: ''"
        writer.WriteLine $"  subtitle: {art.subtitle}"
        writer.WriteLine $"  file_subtitle: {art.subtitle}"
        writer.WriteLine $"  subdate: 2023/03/07 12:00"
        writer.WriteLine $"  subupdate: 2023/03/07 12:00"
        writer.WriteLine $"  download_time: 2023-03-08 12:00:00.000000000 +09:00"
        
let escapeUrl (url: string) : string =
    let trimmed = url.Replace(urlRoot, "root_").Replace("/", "_")
    trimmed    

let loadHtmlFromCache (file: string) : HtmlDocument =
    use reader = new StreamReader(file)
    HtmlDocument.Load reader

let loadHtml (url: string) : HtmlDocument =
    let cacheFile = rawDirectory + (escapeUrl url) + ".html"
    if File.Exists cacheFile then
        Console.WriteLine($" => Load {cacheFile} from cache...")
        loadHtmlFromCache cacheFile
    else
        Thread.Sleep(1000)
        if not (Directory.Exists rawDirectory) then Directory.CreateDirectory rawDirectory |> ignore
        Console.WriteLine($" => Load {url} from remote...save as {cacheFile}")
        let result = HtmlDocument.Load url
        use writer = new StreamWriter(cacheFile) 
        writer.Write(result.ToString())
        result

let applyReplacement (state: string) ((elem: Regex), (replacement: string)) =
    elem.Replace(state, replacement)

let extractParagraph (para: string) : string =
    let text = List.fold applyReplacement para regexs
    text
    
    