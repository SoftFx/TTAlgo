using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TickTrader.Algo.Api;
using TickTrader.Algo.Domain;

namespace TickTrader.Algo.Core.Tests.Drawables
{
    [TestClass]
    public class DrawablePropertyTests
    {
        private const BindingFlags PublicInstancePropFlags = BindingFlags.Public | BindingFlags.Instance;

        private static readonly Dictionary<string, ITypeValueProcessor> _typeProcessors;


        static DrawablePropertyTests()
        {
            _typeProcessors = new()
            {
                { typeof(bool).FullName, new BoolPropValueProcessor() },
                { typeof(string).FullName, new StringPropValueProcessor() },
                { typeof(int).FullName, new Int32PropValueProcessor() },
            };
        }


        [TestMethod]
        public void TestCommonProps()
        {
            const string name = "Test object";
            (var ctx, var collection) = DrawableTestContext.Create();

            var obj = collection.Create(name, DrawableObjectType.TextBlockControl);
            ctx.FlushAndResetUpdates();
            var testRes = TestProperties(ctx, obj, o => o, i => i);
            foreach (var test in testRes)
            {
                Console.WriteLine(test);
            }
        }


        private List<PropertyTestResult> TestProperties<TApi, TDomain>(DrawableTestContext ctx, IDrawableObject obj,
            Func<IDrawableObject, TApi> apiSelector, Func<DrawableObjectInfo, TDomain> domainSelector)
        {
            var isSupportedProp = typeof(TApi).GetProperty("IsSupported");
            TApi apiView = apiSelector(obj);
            var shouldUpdate = true;
            if (isSupportedProp != null)
            {
                shouldUpdate = (bool)isSupportedProp.GetValue(apiView);
            }

            var testRes = new List<PropertyTestResult>();
            var apiProps = typeof(TApi).GetProperties(PublicInstancePropFlags).Where(p => p.CanWrite).ToArray();
            var domainPropsLookup = typeof(TDomain).GetProperties(PublicInstancePropFlags).ToDictionary(p => p.Name);
            foreach (var aProp in apiProps)
            {
                var propRes = new PropertyTestResult(aProp);
                testRes.Add(propRes);
                try
                {
                    if (!_typeProcessors.TryGetValue(aProp.PropertyType.FullName, out var valProcessor))
                    {
                        propRes.Error = "Unsupported property type";
                        continue;
                    }

                    var oldValue = aProp.GetValue(apiView);
                    var newValue = valProcessor.Update(oldValue);
                    if (Equals(oldValue, newValue))
                    {
                        propRes.Error = "Can't produce new proprety value";
                        continue;
                    }

                    ctx.ResetUpdates();
                    aProp.SetValue(apiView, newValue);
                    ctx.FlushUpdates();

                    if (shouldUpdate && ctx.Updates.Count == 0)
                    {
                        // When IsSupported == false, no update is expected on property modification
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
                    var dPropName = aProp.Name;
                    if (!domainPropsLookup.TryGetValue(dPropName, out var dProp))
                    {
                        propRes.Error = "Can't find domain property with name: " + dPropName;
                        continue;
                    }
                    var newDomainValue = dProp.GetValue(domainView);
                    if (!valProcessor.CheckEqual(newValue, newDomainValue))
                    {
                        propRes.Error = "Value was not modified";
                        continue;
                    }
                }
                catch (Exception ex)
                {
                    propRes.Error = "Exception caught: " + ex.GetType().FullName;
                }
            }

            return testRes;
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

            public bool CheckEqual(object x, object y)
            {
                var v1 = (T)x; var v2 = (T)y;
                return _cmp.Equals(v1, v2);
            }

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
            public override object Update(object x)
            {
                var i = (int)x;
                return i + 42;
            }
        }
    }
}
