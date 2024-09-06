using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using CatTree;

namespace Test1
{
    internal class Program
    {
        static void Main1(string[] args)
        {
            Console.WriteLine("Hello World!");

            Random random = new Random();
            List<Tree> list = new List<Tree>();
            for (int i = 20; i < 26; i++)
            {
                var shen = new Tree { Id = i.ToString(), Level = 1, Key = "省", ParentId = "0", Name = "省" + i, Order = random.Next(100000000) };
                list.Add(shen);
                for (int s = 20; s < 40; s++)
                {
                    var shi = new Tree { Id = shen.Id + s.ToString(), Level = 2, Key = "市", ParentId = shen.Id, Name = "市" + s, Order = random.Next(100000000) };
                    list.Add(shi);

                    for (int q = 20; q < 40; q++)
                    {
                        var qu = new Tree { Id = shi.Id + q.ToString(), Level = 3, Key = "区", ParentId = shi.Id, Name = "区" + q, Order = random.Next(100000000) };
                        list.Add(qu);

                        for (int j = 20; j < 40; j++)
                        {
                            var jie = new Tree { Id = qu.Id + j.ToString(), Level = 4, Key = "街道", ParentId = qu.Id, Name = "街道" + j, Order = random.Next(100000000) };
                            list.Add(jie);

                            for (int sq = 20; sq < 40; sq++)
                            {
                                var shequ = new Tree { Id = jie.Id + sq.ToString(), Level = 5, Key = "社区", ParentId = jie.Id, Name = "社区" + sq, Order = random.Next(100000000) };
                                list.Add(shequ);
                            }
                        }
                    }
                }
            }

            list.Add(new Tree() { Id = "0", Name = "国家1", Order = random.Next(100000000) });
            Console.WriteLine(list.Count());
            list.Reverse();
            list = list.OrderBy(e => e.Order).ToList();


            TreeDefault.ParentIdMemberName = "Pid";
            Stopwatch stopwatch = new Stopwatch();
            Action<TreeOptions<Tree, string>> optionsAction = options =>
            {
                options
                .ConfigureRootId("0")
                //.ConfigureId(tree => tree.Id)
                .ConfigureParent(tree => tree.ParentId, tree => tree.Parent)
                .ConfigureChildren(tree => tree.Children)
                .ConfigureChildrenTypeAsList()
                .MinChildrenCapacity = 10;
            };
            stopwatch.Start();




            Console.ReadKey();
        }

        static void Main2(string[] args)
        {

            var json = File.ReadAllText("..\\..\\..\\..\\areas\\area_code_2024.json");
            var areaJsonList = JsonSerializer.Deserialize<IEnumerable<Area>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            Console.WriteLine(areaJsonList.Count());
            var areas = areaJsonList.SelectMany(e => AreaTreeToList(e)).ToList(); ;
            foreach (var area in areas)
            {
                area.Parent = null;
                area.Children = null;
            }
            areas.Add(new Area { Code = 0, Pcode = -1, Level = 0, Name = "中国", });
            areas = areas.DistinctBy(e => e.Code).ToList();
            var treeResult = areas.BuildTree(options =>
            {
                options
                .ConfigureRootId(0)
                .ConfigureId(e => e.Code)
                .ConfigureParent(e => e.Pcode, e => e.Parent)
                .ConfigureChildren(e => e.Children)
                .ConfigureChildrenTypeAsList();
            });
            Console.WriteLine(treeResult.Root);

            Console.ReadKey();


        }

        static void Main3(string[] args)
        {
            #region 真实行政区划数据
            var areas = GetAreaEntities() as List<Area>;
            #endregion
            TreeDefault.MinChildrenCapacity = 20;
            TreeDefault.IdMemberName = nameof(Area.Code);
            TreeDefault.ParentIdMemberName = nameof(Area.Pcode);
            TreeDefault.ChildrenMemberName = nameof(Area.Children);
            //TreeDefault.ChildrenMemberName = "_Children2_";
            TreeDefault.ParentMemberName = nameof(Area.Parent) + "123";
            TreeDefault.ParentMemberName = nameof(Area.Parent);
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var treeResult = areas.BuildTree(options =>
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
                //.ConfigureChildrenTypeAsList()
                //.ConfigureChildrenTypeAsHashSet()
                //.ConfigureChildrenTypeAsCollection()
                //.ConfigureChildrenTypeAsReadOnlyCollection()
                .ConfigureChildrenTypeAsImmutableList()

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
            var qbj = treeResult.FindRaw(510113000000);
            var qbjAllDescendants = treeResult.GetAllDescendants(qbj);
            Console.ReadKey();


        }

        static void Main4(string[] args)
        {
            #region 真实行政区划数据
            var areas = GetAreaEntities() as List<Area>;
            #endregion
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            //var treeResult = areas.Build(options =>
            //{
            //    options
            //    //.ConfigureRootId(0)
            //    .ConfigureRootId(110000000000)
            //    .ConfigureId(e => e.Code)
            //    .ConfigureParentId(e => e.Pcode)        
            //    .ConfigureChildrenTypeAsImmutableList()
            //    .ConfigureChildrenTypeAsReadOnlyCollection()
            //    .ConfigureChildrenType<List<Area>>()    
            //    .ConfigureChildrenTypeAsList()   
            //    .Default=new MyTreeOptionsDefault()
            //    ;
            //    options.MinChildrenCapacity = 20;
            //});

            var treeResult = areas.BuildTree(0, e => e.Code, e => e.Pcode);
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(treeResult.Root);
            var qbj = treeResult.FindRaw(510113000000);
            var qbjAllDescendants = treeResult.GetAllDescendants(qbj);
            Console.ReadKey();


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
                  *      
                  *      
                  *      Net40、45版本后续会提供。
                  */

        }

        static async Task Main(string[] args)
        {

            //Stopwatch stopwatch = new Stopwatch();
            //List<Area> areas = new List<Area>();
            //stopwatch.Start();
            //for (int i = 0; i < 1100000; i++)
            //{
            //    areas.Add(new Area());
            //}
            //stopwatch.Stop();
            //Console.WriteLine(stopwatch.Elapsed);
            //Queue<Area> areas1 = new Queue<Area>();
            //stopwatch.Restart();
            //for (int i = 0; i < 1100000; i++)
            //{
            //    var a= new Area();
            //    areas1.Enqueue(a);
            //}
            //stopwatch.Stop();
            //Console.WriteLine(stopwatch.Elapsed);

            //ConcurrentQueue<Area> values = new ConcurrentQueue<Area>();
            //stopwatch.Restart();
            //await Task.Run(() =>
            //{
            //    for (int i = 0; i < 1100000; i++)
            //    {
            //        values.Enqueue(new Area());
            //    }
            //});

            //stopwatch.Stop();
            //Console.WriteLine(stopwatch.Elapsed);

            //Case1();
            //Case2();
            Case3();

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
             *      
             *      
             *      Net40、45版本后续会提供。
             */

        }

        static void Case1()
        {
            #region 模拟行五级政区划准备数据大概100万条数据，并且打乱顺序
            Random random = new Random();
            List<Tree> list = new List<Tree>();
            for (int i = 20; i < 26; i++)
            {
                var shen = new Tree { Id = i.ToString(), ParentId = "0", Name = "省" + i, Order = random.Next(100000000) };
                list.Add(shen);
                for (int s = 20; s < 40; s++)
                {
                    var shi = new Tree { Id = shen.Id + s.ToString(), ParentId = shen.Id, Name = "市" + s, Order = random.Next(100000000) };
                    list.Add(shi);

                    for (int q = 20; q < 40; q++)
                    {
                        var qu = new Tree { Id = shi.Id + q.ToString(), ParentId = shi.Id, Name = "区" + q, Order = random.Next(100000000) };
                        list.Add(qu);

                        for (int j = 20; j < 40; j++)
                        {
                            var jie = new Tree { Id = qu.Id + j.ToString(), ParentId = qu.Id, Name = "街道" + j, Order = random.Next(100000000) };
                            list.Add(jie);

                            for (int sq = 20; sq < 40; sq++)
                            {
                                var shequ = new Tree { Id = jie.Id + sq.ToString(), ParentId = jie.Id, Name = "社区" + sq, Order = random.Next(100000000) };
                                list.Add(shequ);
                            }
                        }
                    }
                }
            }

            list.Add(new Tree() { Id = "0", Key = "国家", Name = "国家1", Order = random.Next(100000000) });
            Console.WriteLine(list.Count());
            list.Reverse();
            list = list.OrderBy(e => e.Order).ToList();

            #endregion

            var tree = list.BuildTree("0");
            Console.WriteLine(tree.Root);
            Console.WriteLine(tree.Root.Children.Count);


            Console.ReadKey();
        }

        static void Case2()
        {
            #region 真实行政区划数据
            var areas = GetAreaEntities() as List<Area>;
            #endregion

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
            Console.WriteLine(areaTree.Root);

            var qbj = areaTree.FindRaw(510113000000);
            var qbjAllDescendants = areaTree.GetAllDescendants(qbj);
            var ps1 = areaTree.GetAllAncestors(qbj);
            var a1 = areas.Find(e => e.Code == 1);

            var qbj2 = areaTree.Find(510113000000);
            Console.WriteLine(qbj2.Node);
            Console.WriteLine(qbj2.Node.Children.ElementAt(0));
            var ps2 = qbj2.GetAllAncestors();
            var ds2 = qbj2.GetAllDescendants();
            areaTree.GetAllDescendants();


            Console.ReadKey();



        }

        static void Case3()
        {
            #region 真实行政区划数据
            var areas = GetAreaEntities() as List<Area>;
            #endregion

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            //TreeDefault.ParentIdMemberName = "Pcode";
            //TreeDefault.IdMemberName = "Code";
            //TreeDefault.ChildrenMemberName = "Children";
            var options = TreeBuilder.Configure<Area, long>(options =>
            {
                options
                .ConfigureRootId(0)//配置RootId
                .ConfigureId(e => e.Code)//配置Id
                .ConfigureParentId(e => e.Pcode)//配置ParentId
                .ConfigureChildren(e => e.Children)//配置Children

                #region 配置ChildrenType，以最后一次配置为最终配置
                //.ConfigureChildrenTypeAsList()//ChildrenType配置为List
                .ConfigureChildrenTypeAsHashSet()//ChildrenType配置为HashSet
                //.ConfigureChildrenTypeAsReadOnlyCollection()//ChildrenType配置为ReadOnlyCollection
                //.ConfigureChildrenTypeAsImmutableList()//ChildrenType配置为ImmutableList
                ////.ConfigureChildrenType<List<Area>>()//自定义配置
                ////.ConfigureChildrenType<TreeReadOnlyCollection<Area>>()//错误示范,只读集合不能配置为ChildrenType
                #endregion
                ;
                options.MinChildrenCapacity = 20;
            });
            var options2 = new TreeOptions<Area, long>();
            options2.ConfigureRootId(0)//配置RootId
                .ConfigureId(e => e.Code)//配置Id
                .ConfigureParentId(e => e.Pcode)//配置ParentId
                .ConfigureChildren(e => e.Children)//配置Children
                .ConfigureChildrenTypeAsHashSet()//ChildrenType配置为HashSet
                ;
            var areaTree = areas.BuildTree(options2);
            //var areaTree = options.BuildTree(areas);
            stopwatch.Stop();
            Console.WriteLine(areaTree.Root);
            Console.WriteLine(stopwatch.ElapsedMilliseconds);
            for (int i = 0; i < 10; i++)
            {
                stopwatch.Restart();
                var areaTree2 = options.BuildTree(areas);
                stopwatch.Stop();
                Console.WriteLine($"第{i + 1}次,Root:{areaTree2.Root},Time：{stopwatch.ElapsedMilliseconds}");
            }
            var qbj = areaTree.FindRaw(510113000000);
            var qbjAllDescendants = areaTree.GetAllDescendants(qbj);
            var ps1 = areaTree.GetAllAncestors(qbj);
            var a1 = areas.Find(e => e.Code == 1);

            var qbj2 = areaTree.Find(510113000000);
            Console.WriteLine(qbj2.Node);
            Console.WriteLine(qbj2.Node.Children.ElementAt(0));
            var ps2 = qbj2.GetAllAncestors();
            var ds2 = qbj2.GetAllDescendants();
            areaTree.GetAllDescendants();


            Console.ReadKey();
        }

        static void Case1_1()
        {
            #region 模拟行五级政区划准备数据大概100万条数据，并且打乱顺序
            Random random = new Random();
            List<Tree> list = new List<Tree>();
            for (int i = 20; i < 26; i++)
            {
                var shen = new Tree { Id = i.ToString(), ParentId = "0", Name = "省" + i, Order = random.Next(100000000) };
                list.Add(shen);
                for (int s = 20; s < 40; s++)
                {
                    var shi = new Tree { Id = shen.Id + s.ToString(), ParentId = shen.Id, Name = "市" + s, Order = random.Next(100000000) };
                    list.Add(shi);

                    for (int q = 20; q < 40; q++)
                    {
                        var qu = new Tree { Id = shi.Id + q.ToString(), ParentId = shi.Id, Name = "区" + q, Order = random.Next(100000000) };
                        list.Add(qu);

                        for (int j = 20; j < 40; j++)
                        {
                            var jie = new Tree { Id = qu.Id + j.ToString(), ParentId = qu.Id, Name = "街道" + j, Order = random.Next(100000000) };
                            list.Add(jie);

                            for (int sq = 20; sq < 40; sq++)
                            {
                                var shequ = new Tree { Id = jie.Id + sq.ToString(), ParentId = jie.Id, Name = "社区" + sq, Order = random.Next(100000000) };
                                list.Add(shequ);
                            }
                        }
                    }
                }
            }

            list.Add(new Tree() { Id = "0", Key = "国家", Name = "国家1", Order = random.Next(100000000) });
            Console.WriteLine(list.Count());
            list.Reverse();
            list = list.OrderBy(e => e.Order).ToList();

            #endregion

            TreeDefault.MinChildrenCapacity = 20;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            var tree = list.BuildTree("0");
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);
            Console.WriteLine(tree.Root);
            Console.WriteLine(tree.Root.Children.Count);

            stopwatch.Restart();
            var tree2 = list.BuildTree("0");
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);

            stopwatch.Restart();
            var tree3 = list.BuildTree("0");
            stopwatch.Stop();
            Console.WriteLine(stopwatch.Elapsed);

            Console.ReadKey();
        }


        private static IEnumerable<Area> AreaTreeToList(Area area)
        {
            if (area is null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            yield return area;

            var children = area.Children;
            if (children == null || !children.Any())
                yield break;

            foreach (var child in children)
            {
                if (child == null)
                    continue;
                //yield return child;//下面调用的AreaTreeToList会返回当前child

                foreach (var child2 in AreaTreeToList(child))
                {
                    yield return child2;
                }

            }
        }


        /// <summary>
        /// 真实行政区划数据
        /// </summary>
        /// <returns></returns>
        private static IEnumerable<Area> GetAreaEntities()
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

    }
    public class Tree : Tree<Tree, string>
    {

        public string Name { get; set; }

        public string Key { get; set; }

        public int Level { get; set; }

        /// <summary>
        /// 排序使用的随机数
        /// </summary>
        public long Order { get; set; }

        public string Id { get; set; }
        public string ParentId { get; set; }

        public ICollection<Tree> Children { get; set; }

        //public HashSet<Tree> Children { get; set; }

        public Tree Parent { get; set; }

        public override string ToString()
        {
            return $"{Key}:{Name}";
        }
    }



    public class Area : ITree<Area, long>
    {
        public long Code { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public long Pcode { get; set; }

        public int Category { get; set; }

        public Area Parent { get; set; }

        //public ReadOnlyCollection<Area> Children { get; set; }
        //public ImmutableList<Area> Children { get; set; }
        //public ICollection<Area> Children { get; set; }
        public HashSet<Area> Children { get; set; }
        //public List<Area> Children { get; set; }
        //public MyList<Area> Children { get; set; }
        //public AreaList Children { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }


    public class User : ITree<User, int>
    {
        public int Id { get; set; }
        public int ParentId { get; set; }

        public ICollection<User> Children { get; set; }

        public User Parent { get; set; }

    }

    public class List2<T> : List<T>
    {

    }

    public abstract class List3<T> : List<T>
    {
        public List3()
        {

        }
    }

    public class List4 : List<string>
    {

    }

    /// <summary>
    /// 只读集合
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MyReadOnlyCollection<T> : ReadOnlyCollection<T>, ICollection<T>
    {
        private IList<T> _list;
        public MyReadOnlyCollection() : base(new List<T>())
        {
            _list = base.Items;
        }

        #region 注释以下代码变成只读集合
        /*
        public void Add(T item)
        {
            //throw new Exception();
            _list.Add(item);
        }

        public bool IsReadOnly => false;
        */
        #endregion
    }

    public interface ITreeReadOnlyCollection<T>
    {
        public new IList<T> Items { get; }
    }

    public class TreeReadOnlyCollection<T> : ReadOnlyCollection<T>, ICollection<T>, ITreeReadOnlyCollection<T>
    {
        private IList<T> _items;
        public TreeReadOnlyCollection() : base(new List<T>())
        {
            _items = base.Items;
        }

        public new IList<T> Items { get => _items; }

        #region 注释以下代码变成只读集合
        /*
        public void Add(T item)
        {
            //throw new Exception();
            _list.Add(item);
        }

        public bool IsReadOnly => false;
        */
        #endregion
    }


    public class MyList<T> : List<T>
    {
        public MyList()
        {

        }
    }

    public class AreaList : List<Area>
    {
    }

    public class MyTreeOptionsDefault : TreeOptionsDefault<Area, long>
    {

        protected override void CheckNode(Area entity)
        {
            base.CheckNode(entity);
        }

        protected override void CheckOptions()
        {
            base.CheckOptions();
        }
        protected override void UnknownParentHandler(Area entity)
        {

        }
    }
}
