using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using TickTrader.Algo.Api;
using TickTrader.Algo.CoreV1;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Tests.Drawables
{
    [TestClass]
    public class DrawablePropertyTests
    {
        private static readonly Dictionary<string, ITypeValueProcessor> _typeProcessors = new();


        static DrawablePropertyTests()
        {
            RegisterTypeProcessor<bool, BoolPropValueProcessor>();
            RegisterTypeProcessor<string, StringPropValueProcessor>();
            RegisterTypeProcessor<int, Int32PropValueProcessor>();
            RegisterTypeProcessor<uint, UInt32PropValueProcessor>();
            RegisterTypeProcessor<uint?, NullUInt32PropValueProcessor>();
            RegisterTypeProcessor<double, DoublePropValueProcessor>();
            RegisterTypeProcessor<double?, NullDoublePropValueProcessor>();
            RegisterTypeProcessor<ushort, UInt16PropValueProcessor>();
            RegisterTypeProcessor<ushort?, NullUInt16PropValueProcessor>();
            RegisterTypeProcessor<DateTime, DateTimePropValueProcessor>();
            RegisterTypeProcessor<DrawableObjectVisibility, VisibilityPropValueProcessor>();
            RegisterTypeProcessor<Colors, ColorPropValueProcessor>();
            RegisterTypeProcessor<LineStyles>(new EnumPropValueProcessor<LineStyles, Metadata.Types.LineStyle>(static s => s.ToDomainEnum()));
            RegisterTypeProcessor<LineStyles?>(new NullEnumPropValueProcessor<LineStyles, Metadata.Types.LineStyle>(static s => s.ToDomainEnum()));
            RegisterTypeProcessor<DrawableSymbolAnchor>(new EnumPropValueProcessor<DrawableSymbolAnchor, Drawable.Types.SymbolAnchor>(static a => a.ToDomainEnum()));
            RegisterTypeProcessor<DrawableLineRayMode>(new EnumPropValueProcessor<DrawableLineRayMode, Drawable.Types.LineRayMode>(static m => m.ToDomainEnum()));
            RegisterTypeProcessor<DrawablePositionMode>(new EnumPropValueProcessor<DrawablePositionMode, Drawable.Types.PositionMode>(static m => m.ToDomainEnum()));
            RegisterTypeProcessor<DrawableGannDirection>(new EnumPropValueProcessor<DrawableGannDirection, Drawable.Types.GannDirection>(static d => d.ToDomainEnum()));
            RegisterTypeProcessor<DrawableControlZeroPosition>(new EnumPropValueProcessor<DrawableControlZeroPosition, Drawable.Types.ControlZeroPosition>(static p => p.ToDomainEnum()));
        }

        private static void RegisterTypeProcessor<TType, TProcessor>() where TProcessor : ITypeValueProcessor, new()
        {
            RegisterTypeProcessor<TType>(new TProcessor());
        }

        private static void RegisterTypeProcessor<TType>(ITypeValueProcessor processor)
        {
            _typeProcessors.Add(typeof(TType).FullName, processor);
        }


        [TestMethod]
        public void TestCommonProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj, info => info, _ => true);
        }

        [TestMethod]
        public void TestLineProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Line, info => info.LineProps, DrawableObjectInfo.SupportsLineProps,
                new Dictionary<string, string> { { "Color", "ColorArgb" } });
        }

        [TestMethod]
        public void TestShapeProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Shape, info => info.ShapeProps, DrawableObjectInfo.SupportsShapeProps,
                new Dictionary<string, string> { { "BorderColor", "BorderColorArgb" }, { "FillColor", "FillColorArgb" } });
        }

        [TestMethod]
        public void TestSymbolProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Symbol, info => info.SymbolProps, DrawableObjectInfo.SupportsSymbolProps,
                new Dictionary<string, string> { { "Color", "ColorArgb" } });
        }

        [TestMethod]
        public void TestTextProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Text, info => info.TextProps, DrawableObjectInfo.SupportsTextProps,
                new Dictionary<string, string> { { "Color", "ColorArgb" } });
        }

        [TestMethod]
        public void TestControlProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Control, info => info.ControlProps, DrawableObjectInfo.SupportsControlProps);
        }

        [TestMethod]
        public void TestBitmapProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Bitmap, info => info.BitmapProps, DrawableObjectInfo.SupportsBitmapProps);
        }

        [TestMethod]
        public void TestSpecialProps()
        {
            const int cnt = 16;

            RunPropertyTest(cnt, obj => obj.Special, info => info.SpecialProps, DrawableObjectInfo.SupportsSpecialProps);
        }

        [TestMethod]
        public void TestAnchorList()
        {
            const int cnt = 16;
            const string name = "Test object";
            var ignoreList = new HashSet<string> { };
            var propNameMap = new Dictionary<string, string> { };
            (var ctx, var collection) = DrawableTestContext.Create();

            foreach (var objType in Enum.GetValues<DrawableObjectType>())
            {
                collection.Clear();
                ctx.ResetUpdates();
                var obj = collection.Create(name, objType);
                ctx.FlushUpdates();
                var anchorCnt = DrawableObjectInfo.GetAnchorListSize(objType.ToDomainEnum());
                Assert.AreEqual(anchorCnt, obj.Anchors.Count, "Invalid anchor list size");
                Assert.AreEqual(anchorCnt, ctx.Updates[0].ObjInfo.Anchors?.Count ?? 0, "Invalid anchor list size");
                ctx.ResetUpdates();

                for (var i = 0; i < cnt; i++)
                {
                    var testRes = TestProperties(ctx, obj, anchorCnt > 0, obj => obj.Anchors, info => info.Anchors, propNameMap, ignoreList);
                    ProcessTestResults(testRes, $"Iteration #{i}; Type: {objType}");

                    for (var j = 0; j < obj.Anchors.Count; j++)
                    {
                        var testResItem = TestProperties(ctx, obj, true, obj => obj.Anchors[j], info => info.Anchors[j], propNameMap, ignoreList);
                        ProcessTestResults(testResItem, $"Iteration #{i}; Type: {objType}; Item {j}");
                    }
                }
            }
        }

        [TestMethod]
        public void TestLevelList()
        {
            const int cnt = 16;
            const int maxLevels = 128;
            const string name = "Test object";
            var ignoreList = new HashSet<string> { "Count" };
            var propNameMap = new Dictionary<string, string> { { "DefaultColor", "DefaultColorArgb" } };
            var ignoreListItem = new HashSet<string> { };
            var propNameMapItem = new Dictionary<string, string> { { "Color", "ColorArgb" } };
            (var ctx, var collection) = DrawableTestContext.Create();

            foreach (var objType in Enum.GetValues<DrawableObjectType>())
            //foreach (var objType in new[] { DrawableObjectType.Levels, DrawableObjectType.FiboChannel })
            {
                var hasLevels = DrawableObjectInfo.SupportsLevelsList(objType.ToDomainEnum());

                collection.Clear();
                var obj = collection.Create(name, objType);
                ctx.FlushAndResetUpdates();

                // test levels default props
                for (var i = 0; i < cnt; i++)
                {
                    var testRes = TestProperties(ctx, obj, hasLevels, obj => obj.Levels, info => info.Levels, propNameMap, ignoreList);
                    ProcessTestResults(testRes, $"Iteration #{i}; Type: {objType}");
                }

                for (var k = 1; k <= 2 * maxLevels; k++)
                {
                    var levelCnt = k <= maxLevels ? k : 2 * maxLevels - k;
                    obj.Levels.Count = levelCnt;
                    ctx.FlushUpdates();
                    if (hasLevels)
                    {
                        Assert.AreEqual(levelCnt, obj.Levels.Count, "Invalid level list size");
                        Assert.AreEqual(1, ctx.Updates.Count, "Expected update on levels cnt change");
                        Assert.AreEqual(levelCnt, ctx.Updates[0].ObjInfo.Levels.Count, "Invalid level list size");
                    }
                    else
                    {
                        Assert.AreEqual(levelCnt, obj.Levels.Count, "Invalid level list size");
                        Assert.AreEqual(0, ctx.Updates.Count, "No update when leves not supported");
                    }
                    ctx.ResetUpdates();

                    const int sparseFactor = 8; // testing every level is time consuming
                    for (var j = levelCnt % sparseFactor; j < obj.Levels.Count; j += sparseFactor)
                    {
                        var testRes = TestProperties(ctx, obj, hasLevels, obj => obj.Levels[j], info => info.Levels[j], propNameMapItem, ignoreListItem);
                        ProcessTestResults(testRes, $"Iteration #{k}; Type: {objType}; Item {j}");
                    }
                }
            }
        }


#if DEBUG
        [TestMethod]
        public void TestPropsDiag()
        {
            const int cnt = 5;
            const string name = "Test object";
            var ignoreList = new HashSet<string> { };
            var propNameMap = new Dictionary<string, string> { };
            (var ctx, var collection) = DrawableTestContext.Create();

            var obj = collection.Create(name, DrawableObjectType.Rectangle);
            ctx.FlushAndResetUpdates();
            for (var i = 0; i < cnt; i++)
            {
                var testRes = TestProperties(ctx, obj, true, obj => obj.Shape, info => info.ShapeProps, propNameMap, ignoreList);
                ProcessTestResults(testRes, new string('-', 32), true);
            }
        }
#endif


        private static void RunPropertyTest<TApi, TDomain>(int modifyCnt, Func<IDrawableObject, TApi> apiSelector,
            Func<DrawableObjectInfo, TDomain> domainSelector, Func<Drawable.Types.ObjectType, bool> isSupportedFunc,
            Dictionary<string, string> propNameMap = null, HashSet<string> ignoreList = null)
        {
            const string name = "Test object";
            ignoreList ??= new HashSet<string> { };
            propNameMap ??= new Dictionary<string, string> { };
            (var ctx, var collection) = DrawableTestContext.Create();

            foreach (var objType in Enum.GetValues<DrawableObjectType>())
            {
                collection.Clear();
                var obj = collection.Create(name, objType);
                ctx.FlushAndResetUpdates();

                for (var i = 0; i < modifyCnt; i++)
                {
                    var testRes = TestProperties(ctx, obj, isSupportedFunc(objType.ToDomainEnum()), apiSelector, domainSelector, propNameMap, ignoreList);
                    ProcessTestResults(testRes, $"Iteration #{i}; Type: {objType}");
                }
            }
        }

        private static void ProcessTestResults(List<PropertyTestResult> testRes, string header, bool diag = false)
        {
            if (diag)
            {
                Console.WriteLine(header);
                foreach (var test in testRes)
                {
                    Console.WriteLine(test);
                }
            }
            else
            {
                var hasError = testRes.Any(r => r.HasError);
                if (hasError)
                {
                    var errMsg = new StringBuilder();
                    errMsg.AppendLine();
                    errMsg.AppendLine(header);
                    foreach (var test in testRes.Where(r => r.HasError))
                    {
                        errMsg.AppendLine(test.ToString());
                    }
                    throw new Exception(errMsg.ToString());
                }
            }
        }

        private static List<PropertyTestResult> TestProperties<TApi, TDomain>(DrawableTestContext ctx, IDrawableObject obj,
            bool isPropsSupported, Func<IDrawableObject, TApi> apiSelector, Func<DrawableObjectInfo, TDomain> domainSelector,
            Dictionary<string, string> propNameMap, HashSet<string> ignoreList)
        {
            var testRes = new List<PropertyTestResult>();

            var isSupportedProp = ReflectionCache<TApi>.IsSupportedProp;
            TApi apiView = apiSelector(obj);
            if (isSupportedProp != null)
            {
                var propRes = new PropertyTestResult(isSupportedProp);
                testRes.Add(propRes);
                var isSupportedValue = (bool)isSupportedProp.GetValue(apiView);
                if (isPropsSupported != isSupportedValue)
                {
                    propRes.Error = "Inconsistent property group support";
                }
            }

            var apiProps = ReflectionCache<TApi>.WriteProps;
            var domainPropsLookup = ReflectionCache<TDomain>.PropsLookup;
            foreach (var apiProp in apiProps)
            {
                if (ignoreList.Contains(apiProp.Name))
                    continue;

                var propRes = new PropertyTestResult(apiProp);
                testRes.Add(propRes);
                try
                {
                    if (!_typeProcessors.TryGetValue(apiProp.PropertyType.FullName, out var valProcessor))
                    {
                        propRes.Error = "Unsupported property type";
                        continue;
                    }

                    var oldValue = apiProp.GetValue(apiView);
                    var newValue = valProcessor.Update(oldValue);
                    if (Equals(oldValue, newValue))
                    {
                        propRes.Error = "Can't produce new proprety value";
                        continue;
                    }

                    ctx.ResetUpdates();
                    apiProp.SetValue(apiView, newValue);
                    ctx.FlushUpdates();

                    if (!isPropsSupported)
                    {
                        // When IsSupported == false, no update is expected on property modification
                        if (ctx.Updates.Count > 0)
                            propRes.Error = "Unexpected updates found";

                        continue;
                    }

                    if (ctx.Updates.Count != 1)
                    {
                        propRes.Error = "Invalid update count";
                        continue;
                    }
                    var upd = ctx.Updates[0];
                    if (upd.Action != CollectionUpdate.Types.Action.Updated)
                    {
                        propRes.Error = "Unexpected update action: " + upd.Action;
                        continue;
                    }
                    if (upd.ObjInfo == null)
                    {
                        propRes.Error = "Empty object info";
                        continue;
                    }

                    var domainView = domainSelector(upd.ObjInfo);
                    var apiPropName = apiProp.Name;
                    var domainPropName = apiProp.Name;
                    if (propNameMap.ContainsKey(apiPropName))
                        domainPropName = propNameMap[apiPropName];
                    if (!domainPropsLookup.TryGetValue(domainPropName, out var domainProp))
                    {
                        propRes.Error = "Can't find domain property with name: " + domainPropName;
                        continue;
                    }
                    var newDomainValue = domainProp.GetValue(domainView);
                    if (!valProcessor.CheckEqual(newValue, newDomainValue))
                    {
                        propRes.Error = "Value was not modified";
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    propRes.Error = "Exception caught: " + ex.ToString();
                }
            }

            ctx.ResetUpdates();
            return testRes;
        }


        private static class ReflectionCache<T>
        {
            private const BindingFlags PublicInstancePropFlags = BindingFlags.Public | BindingFlags.Instance;

            public static readonly PropertyInfo IsSupportedProp = typeof(T).GetProperty("IsSupported");
            public static readonly PropertyInfo[] WriteProps = typeof(T).GetProperties(PublicInstancePropFlags).Where(p => p.CanWrite).ToArray();
            public static readonly Dictionary<string, PropertyInfo> PropsLookup = typeof(T).GetProperties(PublicInstancePropFlags).ToDictionary(p => p.Name);
        }

        private class PropertyTestResult
        {
            public string Type { get; }

            public string Name { get; }

            public string Error { get; set; }

            public bool HasError => !string.IsNullOrEmpty(Error);


            public PropertyTestResult(PropertyInfo propInfo)
            {
                Type = propInfo.PropertyType.Name;
                Name = propInfo.Name;
            }


            public override string ToString()
            {
                return !HasError
                    ? $"[{Type}] {Name}: Ok"
                    : $"[{Type}] {Name}: Fail ({Error})";
            }
        }

        private interface ITypeValueProcessor
        {
            bool CheckEqual(object x, object y);

            object Update(object x);
        }

        private abstract class DefaultTypeValueProcessor<T> : ITypeValueProcessor
        {
            protected static readonly IEqualityComparer<T> _cmp = EqualityComparer<T>.Default;

            public virtual bool CheckEqual(object x, object y) => _cmp.Equals((T)x, (T)y);

            public abstract object Update(object x);
        }

        private class BoolPropValueProcessor : DefaultTypeValueProcessor<bool>
        {
            public override object Update(object x)
            {
                var b = (bool)x;
                return !b;
            }
        }

        private class StringPropValueProcessor : DefaultTypeValueProcessor<string>
        {
            public override object Update(object x)
            {
                var s = (string)x;
                return string.IsNullOrEmpty(s) ? "test" : s + "test";
            }
        }

        private class Int32PropValueProcessor : DefaultTypeValueProcessor<int>
        {
            public override object Update(object x) => (int)x + 1;
        }

        private class UInt32PropValueProcessor : DefaultTypeValueProcessor<uint>
        {
            public override object Update(object x) => (uint)x + 1;
        }

        private class NullUInt32PropValueProcessor : DefaultTypeValueProcessor<uint?>
        {
            public override object Update(object x) => ((uint?)x + 1) ?? 1;
        }

        private class DoublePropValueProcessor : DefaultTypeValueProcessor<double?>
        {
            public override object Update(object x) => (double)x + 0.42;
        }

        private class NullDoublePropValueProcessor : DefaultTypeValueProcessor<double?>
        {
            public override object Update(object x) => ((double?)x + 0.42) ?? 0.21;
        }

        private class UInt16PropValueProcessor : ITypeValueProcessor
        {
            public bool CheckEqual(object x, object y) => (ushort)x == (uint)y; // domain type is uint (protobuf)

            public object Update(object x) => (ushort)((ushort)x + 1);
        }

        private class NullUInt16PropValueProcessor : ITypeValueProcessor
        {
            public bool CheckEqual(object x, object y) => (ushort?)x == (uint?)y; // domain type is uint? (protobuf)

            public object Update(object x) => (ushort)(((ushort?)x + 1) ?? 1);
        }

        private class DateTimePropValueProcessor : DefaultTypeValueProcessor<DateTime>
        {
            public override bool CheckEqual(object x, object y) => new UtcTicks((DateTime)x) == (UtcTicks)y;

            public override object Update(object x) => ((DateTime)x).AddYears(1);
        }

        private class VisibilityPropValueProcessor : ITypeValueProcessor
        {
            public bool CheckEqual(object x, object y) => (uint)x == (uint)y;

            public object Update(object x)
            {
                var v = (DrawableObjectVisibility)x;
                return v ^ DrawableObjectVisibility.TimeframeM1 ^ DrawableObjectVisibility.TimeframeH1;
            }
        }

        private class ColorPropValueProcessor : ITypeValueProcessor
        {
            public bool CheckEqual(object x, object y)
            {
                var c1 = ((Colors)x).ToArgb();
                var c2 = (uint?)y;
                return c1 == c2;
            }

            public object Update(object x)
            {
                Span<Colors> span = stackalloc[] { (Colors)x };
                var bytes = MemoryMarshal.Cast<Colors, byte>(span);
                bytes[3] ^= 128;
                bytes[2] ^= 160;
                bytes[1] ^= 96;
                bytes[0] ^= 192;
                return span[0];
            }
        }

        private static class EnumValueGenerator<T> where T : struct, Enum
        {
            private static readonly T[] _values = Enum.GetValues<T>();
            private static readonly IEqualityComparer<T> _cmp = EqualityComparer<T>.Default;


            public static T NextValue(object value)
            {
                T current = (T)value;
                var index = 0;
                while (index < _values.Length && !_cmp.Equals(current, _values[index])) index++;
                index += 1;
                if (index >= _values.Length)
                    index = 0;
                return _values[index];
            }
        }

        private class EnumPropValueProcessor<TApi, TDomain> : ITypeValueProcessor
            where TApi : struct, Enum
        {
            private static readonly IEqualityComparer<TDomain> _cmp = EqualityComparer<TDomain>.Default;

            private readonly Func<TApi, TDomain> _convert;


            public EnumPropValueProcessor(Func<TApi, TDomain> convert)
            {
                _convert = convert;
            }


            public bool CheckEqual(object x, object y)
            {
                var v1 = _convert((TApi)x);
                var v2 = (TDomain)y;
                return _cmp.Equals(v1, v2);
            }

            public object Update(object x) => EnumValueGenerator<TApi>.NextValue(x);
        }

        private class NullEnumPropValueProcessor<TApi, TDomain> : ITypeValueProcessor
            where TApi : struct, Enum where TDomain : struct, Enum
        {
            private static readonly IEqualityComparer<TDomain?> _cmp = EqualityComparer<TDomain?>.Default;

            private readonly Func<TApi, TDomain> _convert;


            public NullEnumPropValueProcessor(Func<TApi, TDomain> convert)
            {
                _convert = convert;
            }


            public bool CheckEqual(object x, object y)
            {
                var v1 = x == null ? default(TDomain?) : _convert((TApi)x);
                var v2 = (TDomain?)y;
                return _cmp.Equals(v1, v2);
            }

            public object Update(object x) => x == null ? default(TApi) : EnumValueGenerator<TApi>.NextValue(x);
        }
    }
}
