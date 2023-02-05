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

            // get page with models
            IDocument document = await context.OpenAsync(address);

            //get div with model elements
            string cellSelector = "div.List div.List";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);

            // get menu - information about model and country
            string menuSelector = "#MainMenu li";
            var menu = document.QuerySelectorAll(menuSelector);

            var catalog = menu.FirstOrDefault(x => x.TextContent.Contains("Catalog:"));
            var catalogText = catalog.TextContent[(catalog.TextContent.IndexOf(':') + 2)..];

            var region = menu.FirstOrDefault(x => x.TextContent.Contains("Region:"));
            var regionText = region.TextContent[(region.TextContent.IndexOf(':') + 2)..];

            var children = cells.Select(m => m.Children);


            var models = new List<Model>();
            // get rows model
            foreach (IHtmlCollection<IElement> child in children)
            {
                var header = child.Where(x => x.ClassName == "Header").FirstOrDefault();

                var lists = child.Where(x => x.ClassName == "List ");

                if (lists != null)
                {
                    // get data current model
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

                        //add list
                        models.Add(model);
                    }
                }
            }
            //add dbset
            await _context.Models.AddRangeAsync(models);
            await _context.SaveChangesAsync();
        }

        //check - presence of elements
        if (!_context.Complectations.Any())
        {
            string addressComplectation = "https://www.ilcats.ru/toyota/?function=getComplectations&market=EU&model=281220&startDate=198210&endDate=198610&language=en";
            IDocument documentComplectation = await context.OpenAsync(addressComplectation);

            string tableSelector = "tbody";
            var htmlElement = documentComplectation.QuerySelectorAll(tableSelector).FirstOrDefault();

            var complactationList = htmlElement.Children.Skip(1);

            //get menu item
            string mainMenuSelector = "#MainMenu";
            var mainMenuElement = documentComplectation.QuerySelectorAll(mainMenuSelector).FirstOrDefault();
            var currentModel = new Model();
            if (mainMenuElement != null)
            {
                
                var lastChild = mainMenuElement.Children.LastOrDefault();
                var text = lastChild.TextContent.Substring(lastChild.TextContent.IndexOf('(') + 1);
                text = text.TrimEnd(')'); // id model

                currentModel = _context.Models.FirstOrDefault(x => x.ModelCode == text); //find model into db
            }

            List<Complectation> complectations = new List<Complectation>();

            foreach (var complactation in complactationList)
            {
                var newComplect = new Complectation();
                foreach (var item in complactation.Children)
                {
                    ///get current data complactation
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
                    // binding to car model
                    newComplect.ModelId = currentModel.Id;
                }

                complectations.Add(newComplect);
            }


            await _context.Complectations.AddRangeAsync(complectations);
            await _context.SaveChangesAsync();
        }

        if (!_context.Groups.Any())
        {
            string addressSelectGroupParts = "https://www.ilcats.ru/toyota/?function=getGroups&market=EU&model=281220&modification=CV10L-UEMEXW&complectation=001&group=1&language=en";
            IDocument documentGroup = await context.OpenAsync(addressSelectGroupParts);
           
            string className = "name";
            var groupElements = documentGroup.GetElementsByClassName(className);

            ////////
            //get menu item
            string mainMenuSelector = "MainMenu";
            var mainMenuElement = documentGroup.GetElementById(mainMenuSelector);

            if (mainMenuElement != null)
            {

                var lastChild = mainMenuElement.Children.LastOrDefault();
                var nameComplectation = lastChild.TextContent[(lastChild.TextContent.IndexOf(':') + 2)..];

                var CurrentComplectation = _context.Complectations.FirstOrDefault(x => x.Name == nameComplectation);
                if (CurrentComplectation != null)
                {
                    List<Group> groups = new();
                    foreach (var item in groupElements)
                    {
                        Group group = new()
                        {
                            Name = item.TextContent,
                            ComplectationId = CurrentComplectation.Id
                        };

                        groups.Add(group);
                    }
                    await _context.Groups.AddRangeAsync(groups);
                    _ = await _context.SaveChangesAsync();
                }
               
            }
            /////////

            

            
            
            Console.Write("");

        }

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

