// See https://aka.ms/new-console-template for more information
using FastTree;
using System.Diagnostics;
using System.Text;

Console.WriteLine("Hello, World!");

var file = File.OpenRead("..\\..\\..\\..\\areas\\area_code_2024.csv");
StreamReader streamReader = new StreamReader(file, Encoding.UTF8);
var areas = new List<Area>();
while (streamReader.ReadLine() is string line)
{
    var array = line.Split(',');
    areas.Add(new Area
    {
        Code = Convert.ToInt64(array[0]),
        Name = array[1].Trim('\''),
        Level = Convert.ToInt32(array[2]),
        Pcode = Convert.ToInt64(array[3]),
        Category = Convert.ToInt32(array[4]),
    });
}
file.Dispose();
areas.Add(new Area { Code = 0, Pcode = -1, Level = 0, Name = "中国", });
Console.WriteLine($"count:{areas.Count}");
TreeDefault.MinChildrenCapacity = 20;
TreeDefault.IdMemberName = nameof(Area.Code);
TreeDefault.ParentIdMemberName = nameof(Area.Pcode);
TreeDefault.ChildrenMemberName = nameof(Area.Children);
//TreeDefault.ChildrenMemberName = "_Children2_";
TreeDefault.ParentMemberName = nameof(Area.Parent) + "123";
TreeDefault.ParentMemberName = nameof(Area.Parent);
Stopwatch stopwatch = new Stopwatch();
stopwatch.Start();
var treeResult = areas.Build(options =>
{
    options
    .ConfigureRootId(0)
    //.ConfigureId(e => e.Code)
    //.ConfigureParentId(e => e.Pcode)
    ////.ConfigureParent(e => e.Parent)
    ////.ConfigureParent(e => e.Pcode, e => e.Parent)
    //.ConfigureChildren(e => e.Children)
    //.ConfigureChildrenType<List<Area>>()
    //.ConfigureChildrenType<TreeReadOnlyCollection<Area>>()//错误示范
    .ConfigureChildrenTypeAsReadOnlyCollection()
    .ConfigureChildrenTypeAsList()
    
    //.ConfigureChildrenTypeAsHashSet()
    //.ConfigureChildrenTypeAsCollection()
    //.ConfigureChildrenTypeAsReadOnlyCollection()
    //.ConfigureChildrenTypeAsImmutableList()
    ;
    options.MinChildrenCapacity = 20;
});
Console.WriteLine(stopwatch.Elapsed);
Console.WriteLine(treeResult.Root);
//stopwatch.Restart();
//var treeResult2 = areas.Build(options =>
//{
//    options
//    .SetRootId(0)
//    .ConfigureChildrenTypeAsHashSet()
//    ;
//    options.MinChildrenCapacity = 20;
//});
//Console.WriteLine(stopwatch.Elapsed);
//stopwatch.Restart();
//var treeResult3 = areas.Build(options =>
//{
//    options
//    .SetRootId(0)
//    .ConfigureChildrenTypeAsHashSet()
//    ;
//    options.MinChildrenCapacity = 20;
//});
//Console.WriteLine(stopwatch.Elapsed);
var qbj = treeResult.Find(510113000000);
var qbjAllDescendants = treeResult.GetAllDescendants(qbj);
Console.ReadKey();




public class Area : Tree<Area, long>
{
    public long Code { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public long Pcode { get; set; }

    public int Category { get; set; }

    public Area Parent { get; set; }

    //public ReadOnlyCollection<Area> Children { get; set; }
    //public ImmutableList<Area> Children { get; set; }
    public ICollection<Area> Children { get; set; }
    //public List<Area> Children { get; set; }

    public override string ToString()
    {
        return Name;
    }
}