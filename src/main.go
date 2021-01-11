package main

import (
	"encoding/json"
	"livematch/utils"
	"log"
	"net/http"
	"regexp"
	"strings"

	"github.com/gocolly/colly/v2"
)

// LinkDetails will be used to hold all metadata
type LinkDetails struct {
	Title    string
	Time     string
	Language string
}

// LinkModel holds all live link data
type LinkModel struct {
	LinkDetails
	Links []string
}

func handler(w http.ResponseWriter, r *http.Request) {
	URL := "http://livetv.sx/enx/livescore/"

	c := colly.NewCollector(
		colly.CacheDir("./cache"),
	)

	linkCollector := colly.NewCollector(
		colly.CacheDir("./cache"),
		colly.Async(true),
	)

	linkCollector.Limit(&colly.LimitRule{
		Parallelism: 4,
	})

	p := utils.NewConcurrentSlice()

	// count links
	c.OnHTML("a[href]", func(e *colly.HTMLElement) {
		if e.Text == "Video" {
			link := e.Request.AbsoluteURL(e.Attr("href"))
			linkCollector.Visit(link)
		}
	})

	linkCollector.OnHTML("html", func(e *colly.HTMLElement) {
		if title := e.ChildText("h1[itemprop=name] b"); title != "" {
			ll := utils.NewConcurrentSlice()
			e.ForEach("a[href]", func(_ int, el *colly.HTMLElement) {
				link := el.Request.AbsoluteURL(el.Attr("href"))
				if strings.Contains(link, "cdn.livetv375.me/webplayer") {
					ll.Append(link)
					e.Request.Visit(link)
				}
			})
			items := make([]string, 0)
			for v := range ll.Iter() {
				items = append(items, v.(string))
			}
			if len(items) > 0 {
				linkinfo := new(LinkModel)
				linkinfo.Title = e.ChildText("h1[itemprop=name] b")
				linkinfo.Time = e.ChildAttr("meta[itemprop=startDate]", "content")
				linkinfo.Links = items
				p.Append(linkinfo)
			}
		}
	})

	linkCollector.OnHTML("iframe[src]", func(e *colly.HTMLElement) {
		e.Request.Visit(e.Request.AbsoluteURL(e.Attr("src")))
	})

	linkCollector.OnHTML("video", func(e *colly.HTMLElement) {
		if strings.Contains(e.Text, ".m3u8") {
			log.Println(ParseM3u8(e.Text))
		}
	})

	linkCollector.OnHTML("script", func(e *colly.HTMLElement) {
		if strings.Contains(e.Text, ".m3u8") {
			log.Println(ParseM3u8(e.Text))
		}
	})

	c.Visit(URL)
	linkCollector.Wait()

	matchInfos := make([]LinkModel, 0)
	for content := range p.Iter() {
		matchInfos = append(matchInfos, *content.(*LinkModel))
	}

	// dump results
	b, err := json.Marshal(matchInfos)
	if err != nil {
		log.Println("failed to serialize response:", err)
		return
	}
	w.Header().Add("Content-Type", "application/json")
	w.Write(b)
}

func ParseM3u8(content string) string {
	re := regexp.MustCompile(`(http(s)?://)([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?`)
	return re.FindString(content)
}

func main() {
	http.HandleFunc("/link", handler)
	log.Fatal(http.ListenAndServe(":8080", nil))
}
