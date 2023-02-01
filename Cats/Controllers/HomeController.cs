using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Cats.Models;
using AngleSharp;
using AngleSharp.Dom;
using System.Linq;
using System.Xml.Linq;
using Cats.Models.Contexts;
using System.Reflection.Metadata;

namespace Cats.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private Context _context;

    public HomeController(ILogger<HomeController> logger, Context context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<IActionResult> IndexAsync()
    {
        AngleSharp.IConfiguration config = Configuration.Default.WithDefaultLoader();
        IBrowsingContext context = BrowsingContext.New(config);
        if (!_context.Models.Any())
        {
            
            string address = "https://www.ilcats.ru/toyota/?function=getModels&language=en&market=EU";
            
            IDocument document = await context.OpenAsync(address);

            string cellSelector = "div.List div.List";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);

            string menuSelector = "#MainMenu li";
            var menu = document.QuerySelectorAll(menuSelector);

            var catalog = menu.FirstOrDefault(x => x.TextContent.Contains("Catalog:"));
            var catalogText = catalog.TextContent[(catalog.TextContent.IndexOf(':') + 2)..];

            var region = menu.FirstOrDefault(x => x.TextContent.Contains("Region:"));
            var regionText = region.TextContent[(region.TextContent.IndexOf(':') + 2)..];

            var children = cells.Select(m => m.Children);


            var models = new List<Model>();
            foreach (IHtmlCollection<IElement> child in children)
            {
                //var e = child.Select(x => x.HasAttribute("Header"));
                var header = child.Where(x => x.ClassName == "Header").FirstOrDefault();

                var lists = child.Where(x => x.ClassName == "List ");

                if (lists != null)
                {
                    foreach (var list in lists)
                    {
                        var element = list.Children.FirstOrDefault();
                        if (element == null)
                        {
                            continue;
                        }
                        var model = new Model();
                        model.Catalog = catalogText;
                        model.Region = regionText;
                        if (header != null)
                        {
                            model.Name = header.TextContent;
                        }
                        var id = element.Children.FirstOrDefault(x => x.ClassName == "id");
                        if (id != null)
                        {
                            model.ModelCode = id.TextContent;
                        }
                        var date = element.Children.FirstOrDefault(x => x.ClassName == "dateRange");
                        if (date != null)
                        {
                            model.DateManufacture = date.TextContent;
                        }
                        var shortComplactations = element.Children.FirstOrDefault(x => x.ClassName == "modelCode");
                        if (shortComplactations != null)
                        {
                            model.ShortComplactations = shortComplactations.TextContent;
                        }

                        models.Add(model);
                    }
                }
            }
            await _context.Models.AddRangeAsync(models);
            await _context.SaveChangesAsync();
        }

        string addressComplectation = "https://www.ilcats.ru/toyota/?function=getComplectations&market=EU&model=281220&startDate=198210&endDate=198610&language=en";
        IDocument documentComplectation = await context.OpenAsync(addressComplectation);

        //string tableSelector = "#Body";
        //IHtmlCollection<IElement> htmlElement = documentComplectation.QuerySelectorAll(tableSelector);
        string tableSelector = "tbody";
        var htmlElement = documentComplectation.QuerySelectorAll(tableSelector).FirstOrDefault();

        var complactationList = htmlElement.Children.Skip(1);

        string mainMenuSelector = "#MainMenu";
        var mainMenuElement = documentComplectation.QuerySelectorAll(mainMenuSelector).FirstOrDefault();
        var currentModel = new Model();
        if (mainMenuElement != null)
        {
            var lastChild = mainMenuElement.Children.LastOrDefault();
            var text = lastChild.TextContent.Substring(lastChild.TextContent.IndexOf('(') + 1);
            text = text.TrimEnd(')');

            currentModel = _context.Models.FirstOrDefault(x => x.ModelCode == text);
        }

        List<Complectation> complectations = new List<Complectation>();

        foreach (var complactation in complactationList)
        {
            var newComplect = new Complectation();
            foreach (var item in complactation.Children)
            {
                
                var modelCode = item.Children.FirstOrDefault(x => x.ClassName == "modelCode");
                if (modelCode != null)
                {
                    newComplect.Name = modelCode.TextContent;
                }

                var dateRange = item.Children.FirstOrDefault(x => x.ClassName == "dateRange");
                if (dateRange != null)
                {
                    newComplect.Date = dateRange.TextContent;
                }

                var engine = item.Children.FirstOrDefault(x => x.ClassName == "01");
                if (engine != null)
                {
                    newComplect.Engine_1 = engine.TextContent;
                }
                var body = item.Children.FirstOrDefault(x => x.ClassName == "03");
                if (body != null)
                {
                    newComplect.Body = body.TextContent;
                }
                var grade = item.Children.FirstOrDefault(x => x.ClassName == "04");
                if (grade != null)
                {
                    newComplect.Grade = grade.TextContent;
                }
                var transmission = item.Children.FirstOrDefault(x => x.ClassName == "05");
                if (transmission != null)
                {
                    newComplect.Transmission = transmission.TextContent;
                }
                var gearShiftType = item.Children.FirstOrDefault(x => x.ClassName == "06");
                if (gearShiftType != null)
                {
                    newComplect.GearShiftType = gearShiftType.TextContent;
                }
                var driverPosition = item.Children.FirstOrDefault(x => x.ClassName == "07");
                if (driverPosition != null)
                {
                    newComplect.DriverPosition = driverPosition.TextContent;
                }
                var doors = item.Children.FirstOrDefault(x => x.ClassName == "08");
                if (doors != null)
                {
                    newComplect.Doors = doors.TextContent;
                }
                var destination_1 = item.Children.FirstOrDefault(x => x.ClassName == "09");
                if (destination_1 != null)
                {
                    newComplect.Destination_1 = destination_1.TextContent;
                }
            }
            if (currentModel != null)
            {
                newComplect.ModelId = currentModel.Id;
            }
            
            complectations.Add(newComplect);
        }


        await _context.Complectations.AddRangeAsync(complectations);
        await _context.SaveChangesAsync();

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

