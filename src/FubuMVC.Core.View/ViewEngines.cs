﻿using System;
using System.Collections.Generic;
using System.Linq;
using FubuCore.Descriptions;
using FubuMVC.Core.Registration;
using FubuMVC.Core.View.Attachment;

namespace FubuMVC.Core.View
{
    public class ViewEngines
    {
        private readonly IList<Func<IViewToken, bool>> _excludes = new List<Func<IViewToken, bool>>();
        private readonly List<IViewFacility> _facilities = new List<IViewFacility>();
        private readonly Lazy<ViewBag> _viewBag;
        private readonly IList<ViewTokenPolicy> _viewPolicies = new List<ViewTokenPolicy>();
        private BehaviorGraph _graph;

        public ViewEngines()
        {
            _viewBag = new Lazy<ViewBag>(buildViewBag);
        }

        // Testing constructor
        public ViewEngines(IEnumerable<IViewToken> theViews)
        {
            _viewBag = new Lazy<ViewBag>(() => new ViewBag(theViews));
        }

        /// <summary>
        /// All of the views found in this running application.
        /// </summary>
        public ViewBag Views
        {
            get { return _viewBag.Value; }
        }

        /// <summary>
        /// All of the registered view engines in this application
        /// </summary>
        public IEnumerable<IViewFacility> Facilities
        {
            get { return _facilities; }
        }

        internal void UseGraph(BehaviorGraph graph)
        {
            _graph = graph;
        }

        /// <summary>
        ///   Define a view activation policy for views matching the filter.
        ///   <seealso cref = "IfTheInputModelOfTheViewMatches" />
        /// </summary>
        public PageActivationExpression IfTheViewMatches(Func<IViewToken, bool> filter)
        {
            return new PageActivationExpression(this, filter);
        }

        /// <summary>
        ///   Define a view activation policy by matching on the input type of a view.
        ///   A view activation element implements <see cref = "IPageActivationAction" /> and takes part in setting up a View instance correctly
        ///   at runtime.
        /// </summary>
        public PageActivationExpression IfTheInputModelOfTheViewMatches(Func<Type, bool> filter)
        {
            Func<IViewToken, bool> combined = viewToken => { return filter(viewToken.ViewModel); };

            return IfTheViewMatches(combined);
        }

        private ViewBag buildViewBag()
        {
            var views = new List<IViewToken>();

            foreach (IViewFacility facility in _facilities)
            {
                views.AddRange(facility.FindViews(_graph));
            }

            _excludes.Each(views.RemoveAll);

            _viewPolicies.Each(x => x.Alter(views));

            return new ViewBag(views);
        }


        /// <summary>
        /// Programmatically add a new view facility.  This method is generally called
        /// by each Bottle and should not be necessary by users
        /// </summary>
        /// <param name="facility"></param>
        public void AddFacility(IViewFacility facility)
        {
            Type typeOfFacility = facility.GetType();
            if (_facilities.Any(f => f.GetType() == typeOfFacility)) return;

            _facilities.Add(facility);
        }

        /// <summary>
        /// Add a new ViewTokenPolicy to alter or configure the behavior of a view
        /// at configuation time
        /// </summary>
        /// <param name="policy"></param>
        public void AddPolicy(ViewTokenPolicy policy)
        {
            _viewPolicies.Add(policy);
        }

        /// <summary>
        /// Exclude discovered views from being used with the view attachment.  Helpful for being able
        /// to run FubuMVC simultaneously with ASP.Net MVC or some other web framework in the same
        /// application
        /// </summary>
        /// <param name="filter"></param>
        public void ExcludeViews(Func<IViewToken, bool> filter)
        {
            _excludes.Add(filter);
        }
    }

    /// <summary>
    /// Used to create a policy altering views represented by an IViewToken
    /// </summary>
    public class ViewTokenPolicy : DescribesItself
    {
        private readonly Action<IViewToken> _alteration;
        private readonly string _description;
        private readonly Func<IViewToken, bool> _filter;

        public ViewTokenPolicy(Func<IViewToken, bool> filter, Action<IViewToken> alteration, string description)
        {
            _filter = filter;
            _alteration = alteration;
            _description = description;
        }

        #region DescribesItself Members

        public void Describe(Description description)
        {
            description.Title = _description;
        }

        #endregion

        public void Alter(IEnumerable<IViewToken> views)
        {
            views.Where(_filter).Each(_alteration);
        }

        public override string ToString()
        {
            return string.Format("ViewTokenPolicy: {0}", _description);
        }
    }
}