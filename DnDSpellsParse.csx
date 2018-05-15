#! "netcoreapp2.0"
#r "nuget: AngleSharp, *"
#r "nuget: Newtonsoft.Json, *"

using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Dom.Html;
using Newtonsoft.Json;

class DndSpell
{
    public string Name { get; set; }
    public string Url { get; set; }
    public string Level { get; set; }
    public string School { get; set; }
    public string CastingTime { get; set; }
    public string Ritual { get; set; }
    public string Concentration { get; set; }
    public string Source { get; set; }
    public string Components { get; set; }
    public string Duration { get; set; }
    public string Range { get; set; }
    public string Description { get; set; }
    public List<string> Classes { get; set; }

    public override string ToString() => Name;
}

class DndSpellsParser
{
    readonly IBrowsingContext browser;

    public DndSpellsParser()
    {
        browser = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
    }

    public async Task<IEnumerable<DndSpell>> GetAllSpells()
    {
        var page = await browser.OpenAsync("https://www.dnd-spells.com/spells");
        var table = page.QuerySelectorAll("table");
        var tbodies = table.SelectMany(t => t.Children.Where(c => c.NodeName == "TBODY"));
        var rows = tbodies.SelectMany(t => t.Children.Where(c => c.NodeName == "TR"));
        return rows.Select(RowToSpell);
    }

    private DndSpell RowToSpell(IElement row)
    {
        return new DndSpell
        {
            Name = row.Children[1].FirstElementChild.TextContent,
            Url = row.Children[1].FirstElementChild.Attributes["href"].Value,
            Level = row.Children[2].TextContent,
            School = row.Children[3].TextContent,
            CastingTime = row.Children[4].TextContent,
            Ritual = row.Children[5].TextContent,
            Concentration = row.Children[6].TextContent,
            Classes = row.Children[7].TextContent.Split(new [] {"\n", " "}, StringSplitOptions.RemoveEmptyEntries).ToList(),
            Source = row.Children[8].TextContent
        };
    }

    public async Task<DndSpell> Populate(DndSpell spell)
    {
        var spellPage = await browser.OpenAsync(spell.Url);
        var p = spellPage.QuerySelectorAll("p")
            .Where(pp => pp.TextContent.Contains("Level:"));
        var nodes = p.SelectMany(pp => pp.ChildNodes);
        var components = nodes.Where(n => n.TextContent.ToLower().Contains("components"))?.FirstOrDefault()?.NextSibling?.TextContent;
        var duration = nodes.Where(n => n.TextContent.ToLower().Contains("duration"))?.FirstOrDefault()?.NextSibling?.TextContent;
        var range = nodes.Where(n => n.TextContent.ToLower().Contains("range"))?.FirstOrDefault()?.NextSibling?.TextContent;
        var hr = spellPage.QuerySelectorAll("hr").First();
        var description = "";
        while(hr.NextElementSibling.NodeName != "HR")
        {
            var text = hr.NextElementSibling.TextContent.Trim();
            if(!string.IsNullOrWhiteSpace(text))
                description += text + "\n";
            hr = hr.NextElementSibling;
        }
        spell.Components = components;
        spell.Duration = duration;
        spell.Range = range;
        spell.Description = description;
        return spell;
    }
}
var parser = new DndSpellsParser();
var spells = await parser.GetAllSpells();
spells = spells.Select(async s => await parser.Populate(s)).Select(s => s.Result).ToArray();
var serz = JsonConvert.SerializeObject(spells);

File.WriteAllText("./spells.json", serz);
//var spells = JsonConvert.DeserializeObject<List<DndSpell>>(File.ReadAllText("./spells.json"));
Debugger.Break();
