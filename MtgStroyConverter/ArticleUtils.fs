module MtgStroyConverter.ArticleUtils
open System
open System.IO
open System.Text

type Chapter = { title: string; url: string }
type Article = {
    index: int 
    chapter: string 
    subtitle: string
    body: string
    url: string
}


let printChapter chapter =
    Console.WriteLine($"{{ title: {chapter.title}, url: {chapter.url} }}")
    
let currentTime = "2023/03/07/ 12:00:00"

let createArticleYaml (article: Article): string  =
   let builder = StringBuilder()
   builder.Append("---\r\n")
       .Append($"index: \"{article.index}\"\r\n")
       .Append($"href: \"{article.url}\"\r\n")
       .Append($"chapter: \"{article.chapter}\"\r\n")
       .Append($"subchapter: \"\"\r\n")
       .Append($"subtitle: {article.subtitle}\r\n")
       .Append($"file_subtitle: {article.subtitle}\r\n")
       .Append($"subdate: \"{currentTime}\"\r\n")
       .Append($"subupdate: \"{currentTime}\"\r\n")
       .Append($"element:\r\n")
       .Append($"\tdata_type: html\r\n")
       .Append($"\tintroduction: \"\"\r\n")
       .Append($"\tpostscript: \"\"\r\n")
       .Append($"\tbody: |-\r\n")
   |> ignore       
   for line in article.body.Split("\r\n") do
     builder.Append($"\t{line}\r\n") |> ignore
   builder.ToString()
         
let saveArticle (article: Article): unit =
    let baseDir = "output/本文"
    let name = $"{baseDir}/{article.index} {article.subtitle}.yml"
    if File.Exists(name) then File.Delete(name)
    if not (Directory.Exists(baseDir)) then Directory.CreateDirectory(baseDir) |> ignore
    let file = new StreamWriter(name)
    file.Write(createArticleYaml article)
    file.Dispose()
    ()
    