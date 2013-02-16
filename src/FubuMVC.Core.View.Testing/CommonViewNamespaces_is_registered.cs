﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using FubuCore;
using FubuMVC.Core.Registration;
using HtmlTags;
using NUnit.Framework;
using FubuTestingSupport;

namespace FubuMVC.Core.View.Testing
{
    [TestFixture]
    public class CommonViewNamespaces_is_registered
    {
        [Test]
        public void is_registered()
        {
            var registry = new FubuRegistry();
            registry.Import<ViewEnginesExtension>();
            registry.AlterSettings<CommonViewNamespaces>(x =>
            {
                x.Add("Foo");
                x.Add("Bar");
            });

            var graph = BehaviorGraph.BuildFrom(registry);
            var useNamespaces = graph.Services.DefaultServiceFor<CommonViewNamespaces>().Value.As<CommonViewNamespaces>();
            useNamespaces.Namespaces.Each(x => Debug.WriteLine(x));

            useNamespaces.Namespaces.ShouldHaveTheSameElementsAs(new[]
            { 
                typeof(VirtualPathUtility).Namespace,
                typeof(string).Namespace,
                typeof(FileSet).Namespace,
                typeof(ParallelQuery).Namespace,
                typeof(HtmlTag).Namespace,
                "Foo",
                "Bar",
            });
        }
    }
}