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

// Link match link
type Link struct {
	URLLink  string
	M3U8Link string
}

// LinkModel holds all live link data
type LinkModel struct {
	LinkDetails
	Links []Link
}

func handler(w http.ResponseWriter, r *http.Request) {
	URL := "http://livetv.sx/enx/livescore/"

	c := colly.NewCollector()

	linkCollector := colly.NewCollector(
		colly.Async(true),
	)

	linkCollector.Limit(&colly.LimitRule{
		Parallelism: 4,
	})

	p := utils.NewConcurrentSlice()
	m3u8mapper := utils.NewConcurrentMap()

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
				if strings.Contains(link, "/webplayer") {
					matchlink := new(Link)
					matchlink.URLLink = link
					ll.Append(matchlink)
					e.Request.Ctx.Put("url", link)
					e.Request.Visit(link)
				}
			})
			items := make([]Link, 0)
			for v := range ll.Iter() {
				items = append(items, *v.(*Link))
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
			m3u8mapper.Set(e.Request.Ctx.Get("url"), parseM3u8(e.Text))
		}
	})

	linkCollector.OnHTML("script", func(e *colly.HTMLElement) {
		if strings.Contains(e.Text, ".m3u8") {
			m3u8mapper.Set(e.Request.Ctx.Get("url"), parseM3u8(e.Text))
		}
	})

	c.Visit(URL)
	linkCollector.Wait()

	matchInfos := make([]LinkModel, 0)
	for content := range p.Iter() {
		linkmodel := *content.(*LinkModel)
		for i := 0; i < len(linkmodel.Links); i++ {
			link := &linkmodel.Links[i]
			if m3u8, found := m3u8mapper.Get(link.URLLink); found {
				link.M3U8Link = m3u8.(string)
			}
		}
		matchInfos = append(matchInfos, linkmodel)
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

func parseM3u8(content string) string {
	re := regexp.MustCompile(`(http(s)?://)([\w-]+\.)+[\w-]+(/[\w- ;,./?%&=]*)?`)
	return re.FindString(content)
}

func main() {
	http.HandleFunc("/link", handler)
	log.Fatal(http.ListenAndServe(":8080", nil))
}
