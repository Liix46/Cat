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

        string mainMenuSelector = "MainMenu";
        IElement? mainMenuElement = null;

        //check - presence of elements && if there is no data to download - fill in
        if (!_context.Models.Any())
        {
            // adress models
            string address = "https://www.ilcats.ru/toyota/?function=getModels&language=en&market=EU";

            // get page with models
            IDocument document = await context.OpenAsync(address);

            //get div with model elements
            string cellSelector = "div.List div.List";
            IHtmlCollection<IElement> cells = document.QuerySelectorAll(cellSelector);

            // get menu - information about model and country
            string menuSelector = "#MainMenu li";
            var menu = document.QuerySelectorAll(menuSelector);

            //get catalog is mark car
            var catalog = menu.FirstOrDefault(x => x.TextContent.Contains("Catalog:"));
            var catalogText = catalog.TextContent[(catalog.TextContent.IndexOf(':') + 2)..];

            // get region is the continent where the machine is delivered
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
                        //find field by classname
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

        //check - presence of elements && if there is no data to download - fill in
        if (!_context.Complectations.Any())
        {
            string addressComplectation = "https://www.ilcats.ru/toyota/?function=getComplectations&market=EU&model=281220&startDate=198210&endDate=198610&language=en";
            IDocument documentComplectation = await context.OpenAsync(addressComplectation);

            string tableSelector = "tbody";
            var htmlElement = documentComplectation.QuerySelectorAll(tableSelector).FirstOrDefault();

            var complactationList = htmlElement.Children.Skip(1);

            //get menu item
            mainMenuElement = documentComplectation.GetElementById(mainMenuSelector);
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

        //check - presence of elements && if there is no data to download - fill in
        if (!_context.Groups.Any())
        {
            string addressSelectGroupParts = "https://www.ilcats.ru/toyota/?function=getGroups&market=EU&model=281220&modification=CV10L-UEMEXW&complectation=001&group=1&language=en";
            IDocument documentGroup = await context.OpenAsync(addressSelectGroupParts);
           
            string className = "name";
            var groupElements = documentGroup.GetElementsByClassName(className);

            //get menu item
            mainMenuElement = documentGroup.GetElementById(mainMenuSelector);

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

        }

        //check - presence of elements && if there is no data to download - fill in
        if (!_context.Subgroups.Any())
        {


            string addressSubGroups = "https://www.ilcats.ru/toyota/?function=getSubGroups&market=EU&model=281220&modification=CV10L-UEMEXW&complectation=001&group=1&language=en";
            IDocument documentSubGroup = await context.OpenAsync(addressSubGroups);



            string subGroupsClassName = "name";
            var subGroupElements = documentSubGroup.GetElementsByClassName(subGroupsClassName);

            //get menu item

            mainMenuElement = documentSubGroup.GetElementById(mainMenuSelector);

            if (mainMenuElement != null)
            {
                IElement currentComplectation = mainMenuElement.Children[4];
                string textCompectation = currentComplectation.TextContent[(currentComplectation.TextContent.IndexOf(':') + 2)..];

                var currentGroup = mainMenuElement.Children.LastOrDefault();
                string textGroup = currentGroup.TextContent[(currentGroup.TextContent.IndexOf(':') + 2)..];

                var tmpComplectation = _context.Complectations.FirstOrDefault(x => x.Name.ToLower().Equals(textCompectation.ToLower()));

                var tmpGroups = _context.Groups.Where(x => x.Name.ToLower().Equals(textGroup.ToLower()));

                var tmpGroup = tmpGroups.FirstOrDefault(x => x.ComplectationId == tmpComplectation.Id);
                if (tmpGroup != null)
                {
                    List<Subgroup> subgroups = new();
                    foreach (var item in subGroupElements)
                    {
                        Subgroup subgroup = new()
                        {
                            Name = item.TextContent,
                            GroupId = tmpGroup.Id
                        };

                        subgroups.Add(subgroup);
                    }

                    await _context.Subgroups.AddRangeAsync(subgroups);
                    _ = await _context.SaveChangesAsync();
                }

            }


        }

        //check - presence of elements && if there is no data to download - fill in
        if (!_context.Parts.Any())
        {
            string partsAdress = "https://www.ilcats.ru/toyota/?complectation=001&function=getParts&group=1&language=en&market=EU&model=281220&modification=CV10L-UEMEXW&subgroup=0901";
            IDocument partsDocument = await context.OpenAsync(partsAdress);

            string tagName = "table";
            var partsTable = partsDocument.GetElementsByTagName(tagName).FirstOrDefault();
            if (partsTable != null)
            {
                var partsBody = partsTable.Children.FirstOrDefault();
                if (partsBody != null)
                {
                    var childrenBody = partsBody.Children.Skip(1);
                    string tree = "";
                    string treeCode = "";
                    Subgroup? subgroup = null;

                    List<Part> parts = new();


                    mainMenuElement = partsDocument.GetElementById(mainMenuSelector);

                    if (mainMenuElement != null)
                    {
                        var subGroupElement = mainMenuElement.Children.LastOrDefault();
                        if (subGroupElement != null)
                        {
                            var subGroupText = subGroupElement.TextContent.Substring(subGroupElement.TextContent.IndexOf(':') + 2);
                            if (subGroupText.EndsWith("..."))
                            {
                                subGroupText = subGroupText.Substring(0, subGroupText.Length - 3);
                            }

                            var subGroups = _context.Subgroups.Where(x => x.Name.StartsWith(subGroupText)).ToList();
                            if (subGroups != null)
                            {
                                var groupElement = mainMenuElement.Children.FirstOrDefault(x => x.TextContent.StartsWith("Group:"));

                                if (groupElement != null)
                                {
                                    var groupText = groupElement.TextContent.Substring(groupElement.TextContent.IndexOf(':') + 2);

                                    var groups = _context.Groups.Where(x => x.Name == groupText);

                                    var complectationElement = mainMenuElement.Children.FirstOrDefault(x => x.TextContent.StartsWith("Complectation:"));

                                    if (complectationElement != null)
                                    {
                                        var complactationText = complectationElement.TextContent.Substring(complectationElement.TextContent.IndexOf(':') + 2).ToUpper();

                                        var complect = _context.Complectations.FirstOrDefault(x => x.Name.ToUpper().Equals(complactationText));
                                        if (complect != null)
                                        {
                                            var group = groups.FirstOrDefault(x => x.ComplectationId == complect.Id);
                                            if (group != null)
                                            {
                                                subgroup = subGroups.FirstOrDefault(x => x.GroupId == group.Id);
                                            }
                                            

                                        }

                                    }

                                }


                            }
                        }
                    }
                    ///

                    foreach (var child in childrenBody)
                    {
                        if (child.Children.Length == 1)
                        {
                            var treeText = child.TextContent;
                            tree = treeText[(treeText.IndexOf(' ') + 1)..];
                            treeCode = treeText[..treeText.IndexOf(' ')];
                        }
                        else
                        {
                            Part part = new()
                            {
                                Tree = tree,
                                TreeCode = treeCode
                            };

                            var number = child.Children.FirstOrDefault(x => x.Children.FirstOrDefault().ClassName == "number");
                            if (number != null)
                            {
                                if (number.TextContent.Contains("Replaced", StringComparison.CurrentCulture))
                                {
                                    part.Code = number.TextContent.Substring(0, number.TextContent.IndexOf("Replaced"));
                                }
                                else
                                {
                                    part.Code = number.TextContent;
                                }
                                

                            }

                            var replaceNumber = child.Children.FirstOrDefault(x => x.Children.LastOrDefault().ClassName == "replaceNumber");

                            if (replaceNumber != null)
                            {

                                var replaceElement = replaceNumber.Children.LastOrDefault();
                                if (replaceElement != null)
                                {
                                    AngleSharp.Html.Dom.IHtmlAnchorElement? linkElement = (AngleSharp.Html.Dom.IHtmlAnchorElement?)replaceElement.Children.FirstOrDefault(x => x.TagName == "A");
                                    if (linkElement != null)
                                    {
                                        part.LinkPrefTable = linkElement.Href;
                                    }
                                }
                            }

                            var countElement = child.Children.FirstOrDefault(x => x.Children.FirstOrDefault().ClassName == "count");

                            if (countElement != null)
                            {
                                bool success = int.TryParse(countElement.TextContent, out int x);
                                if (success)
                                {
                                    part.Count = int.Parse(countElement.TextContent);
                                }

                            }

                            var dateRangeElement = child.Children.FirstOrDefault(x => x.Children.FirstOrDefault().ClassName == "dateRange");

                            if (dateRangeElement != null)
                            {
                                part.Date = dateRangeElement.TextContent;
                            }

                            var usageElement = child.Children.FirstOrDefault(x => x.Children.FirstOrDefault().ClassName == "usage");
                            if (usageElement != null)
                            {
                                part.Info = usageElement.TextContent;
                            }



                            if (subgroup != null)
                            {
                                part.SubgroupId = subgroup.Id;
                            }

                            parts.Add(part);
                        }
                    }

                    await _context.Parts.AddRangeAsync(parts);
                    await _context.SaveChangesAsync();
                }
                
            }
            

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

