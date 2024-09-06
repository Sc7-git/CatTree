using demo.entities;
using FastTree;
using System.Text;
using Masuit.Tools;
using Masuit.Tools.Models;
using System.Linq;
using System.Diagnostics;

namespace demo
{
    /*
     * 约定：
     *      1、Id和ParentId不能相同，必要的检查。
     *      2、根节点必须存在于集合中，集合中只能有一个根节点，如果集合中出现了根节点的兄弟节点，则默认会导致异常(可以通过派生TreeOptionsDefault<TEntity, TIdType>重写UnknownParentHandler来忽略异常)。这也是为什么行政区划案例代码中要添加中国到行政区划中。
     *      3、Id不能为Null,且只有Root的ParentId可以为Null。三个为什么这么做的理由：
     *          Null无法被索引，虽然可以通过类/结构隐式转换来包装Null，达到索引的效果，但会对整个集合造成不必要的隐式转换,增大开销。
     *          并且真实业务中也不建议用Null作为标识字段，更不可以用Null做主键。
     *          再者Id为Null，ParentId也是Null吗？
     *      4、节点可以没有Parent成员，即使配置了全局Parent（TreeDefault.ParentMemberName）也不会导致异常
     *      5、由于只读集合和不可变集合是只读的，所以Children是默认配置为ConfigureChildrenTypeAsList，然后包装成只读或不可变集合，会多一次循环(对性能损耗不大、开销不大)
     *      6、TreeResult.Find可以匹配集合中所有项，不只是从根节点向下查找。
     *      7、Children的类型可以和ConfigureChildrenTypeAsXXXX配置的类型不一致，但后者必须派生自前者
     *      
     *      Net40、45版本后续会提供。
     */
    internal class Program
    {
        static void Main1(string[] args)
        {
            Console.WriteLine("Hello, World!");


            // var areas = GetAreas();

            /*
            var areaTree = areas.BuildTree(options =>
            {
                options
                .ConfigureRootId(0)//配置RootId
                .ConfigureId(e => e.Code)//配置Id
                .ConfigureParentId(e => e.Pcode)//配置ParentId
                //.ConfigureParentId(e => e.Parent.Code)//配置ParentId
                .ConfigureChildren(e => e.Children)//配置Children

                #region 配置ChildrenType，以最后一次配置为最终配置
                //.ConfigureChildrenTypeAsList()//ChildrenType配置为List
                //.ConfigureChildrenTypeAsHashSet()//ChildrenType配置为HashSet
                //.ConfigureChildrenTypeAsReadOnlyCollection()//ChildrenType配置为ReadOnlyCollection
                //.ConfigureChildrenTypeAsImmutableList()//ChildrenType配置为ImmutableList
                ////.ConfigureChildrenType<List<Area>>()//自定义配置
                ////.ConfigureChildrenType<TreeReadOnlyCollection<Area>>()//错误示范,只读集合不能配置为ChildrenType
                #endregion
                ;
                options.MinChildrenCapacity = 20;
            });
            */

            //TreeDefault.IdMemberName = nameof(Area.Code);
            //TreeDefault.ParentIdMemberName = nameof(Area.Pcode);
            //TreeDefault.ChildrenMemberName = nameof(Area.Children);
            //TreeDefault.ParentMemberName = nameof(Area.Parent);
            //TreeDefault.MinChildrenCapacity = 20;

            //var areaTree = areas.BuildTree(0);

            //var areaEntities2 = GetAreaEntities2();
            //Console.WriteLine(DateTime.Now);
            //var reulst = areaEntities2.ToTree(e => e.Id, e => e.ParentId, 0);
            //Console.WriteLine(DateTime.Now);

            var areaEntities = GetAreas();
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var areaTree = areaEntities.BuildTree(options =>
            {
                options
                .ConfigureRootId(0)//配置RootId
                .ConfigureId(e => e.Code)//配置Id
                .ConfigureParentId(e => e.Pcode)//配置ParentId
                //.ConfigureParentId(e => e.Parent.Code)//配置ParentId
                .ConfigureChildren(e => e.Children)//配置Children

                .ConfigureChildrenTypeAsHashSet()//ChildrenType配置为HashSet
                #region 配置ChildrenType，以最后一次配置为最终配置
                //.ConfigureChildrenTypeAsList()//ChildrenType配置为List
                //.ConfigureChildrenTypeAsHashSet()//ChildrenType配置为HashSet
                //.ConfigureChildrenTypeAsReadOnlyCollection()//ChildrenType配置为ReadOnlyCollection
                //.ConfigureChildrenTypeAsImmutableList()//ChildrenType配置为ImmutableList
                ////.ConfigureChildrenType<List<Area>>()//自定义配置
                ////.ConfigureChildrenType<TreeReadOnlyCollection<Area>>()//错误示范,只读集合不能配置为ChildrenType
                #endregion
                ;
                options.MinChildrenCapacity = 20;
            });
            var qbj2 = areaTree.Find(510113000000);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine("当前 节点:");
            Console.WriteLine(qbj2.Node);

            Console.WriteLine("所有父节点:");
            var ps2 = qbj2.GetAllAncestors();
            Console.WriteLine(string.Join(',', ps2.Select(e => $"{e.Code}|{e.Name}")));

            Console.WriteLine("所有子节点:");
            var ds2 = qbj2.GetAllDescendants();
            Console.WriteLine(string.Join(',', ds2.Select(e => $"{e.Code}|{e.Name}")));

            Console.ReadKey();

        }

        static void Main(string[] args)
        {
            
            var list = new List<Tree>() {
                new() { Id = "1", ParentId = null },
                new() { Id = "2", ParentId = "1" },
                new() { Id = "3", ParentId = "2" },
                new() { Id = "4", ParentId = "3" },
                new() { Id = "5", ParentId = "4" },
                new() { Id = "6", ParentId = "2" },
                new() { Id = "7", ParentId = "3" },
                new() { Id = "8", ParentId = "4" },
                new() { Id = "9", ParentId = "1" },
                new() { Id = "10", ParentId = "1" },
            };
            var lu = list.ToLookup(e => e.ParentId);
            var lu2 =lu as System.Linq.Lookup<string,Tree>;
            var lu3 = list.GroupBy(e => e.ParentId);

            var counts=  lu2.ApplyResultSelector((key,val)=> val.Count());
            Console.WriteLine(string.Join(',',counts));

            var areas = GetAreas();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Restart();
            var dict = areas.ToDictionary(e => e.Code);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);

            stopwatch.Restart();
            var lookup = areas.ToLookup(e => e.Pcode);
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);



            stopwatch.Restart();
            var aLs = areas.ToLookup(e => e.Pcode);
            foreach (var area in areas)
            {
                if (aLs.Contains(area.Code))
                {
                    area.Children = aLs[area.Code];
                    foreach (var child in area.Children)
                    {
                        child.Parent = area;
                    }
                }

            }

            var qbj = areas.Where(e => e.Code == 510113000000).First();
            //var qbj = aLs[510113000000].First().Parent;
            stopwatch.Stop();
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            Console.WriteLine("当前 节点:");
            Console.WriteLine(qbj.Name);

            Console.WriteLine("所有父节点:");
            var ps2 = qbj.Parent;
            while (ps2 != null)
            {
                Console.WriteLine(ps2.Name);
                ps2 = ps2.Parent;
            }

            Console.WriteLine("所有子节点:");
            var ds2 = qbj.Children;
            Console.WriteLine(string.Join(',', ds2.Select(e => $"{e.Code}|{e.Name}")));
            Console.ReadKey();
        }

        /// <summary>
        /// 真实行政区划数据
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Area> GetAreas()
        {
            using var file = File.OpenRead("..\\..\\..\\..\\areas\\area_code_2024.csv");
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
            areas.Add(new Area { Code = 0, Pcode = -1, Level = 0, Name = "中国", });
            Console.WriteLine($"count:{areas.Count}");
            return areas;
        }

        /// <summary>
        /// 真实行政区划数据
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<AreaEntity> GetAreaEntities()
        {
            using var file = File.OpenRead("..\\..\\..\\..\\areas\\area_code_2024.csv");
            StreamReader streamReader = new StreamReader(file, Encoding.UTF8);
            var areas = new List<AreaEntity>();
            while (streamReader.ReadLine() is string line)
            {
                var array = line.Split(',');
                areas.Add(new AreaEntity
                {
                    Id = Convert.ToInt64(array[0]),
                    Name = array[1].Trim('\''),
                    Level = Convert.ToInt32(array[2]),
                    ParentId = Convert.ToInt64(array[3]),
                    Category = Convert.ToInt32(array[4]),
                });
            }
            areas.Add(new AreaEntity { Id = 0, ParentId = -1, Level = 0, Name = "中国", });
            Console.WriteLine($"count:{areas.Count}");
            return areas;
        }

        /// <summary>
        /// 真实行政区划数据
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<AreaEntity2> GetAreaEntities2()
        {
            using var file = File.OpenRead("..\\..\\..\\..\\areas\\area_code_2024.csv");
            StreamReader streamReader = new StreamReader(file, Encoding.UTF8);
            var areas = new List<AreaEntity2>();
            while (streamReader.ReadLine() is string line)
            {
                var array = line.Split(',');
                areas.Add(new AreaEntity2
                {
                    Id = Convert.ToInt64(array[0]),
                    Name = array[1].Trim('\''),
                    Level = Convert.ToInt32(array[2]),
                    ParentId = Convert.ToInt64(array[3]),
                    Category = Convert.ToInt32(array[4]),
                });
            }
            areas.Add(new AreaEntity2 { Id = 0, ParentId = -1, Level = 0, Name = "中国", });
            Console.WriteLine($"count:{areas.Count}");
            return areas;
        }
    }


}
