// Copyright (c) 2016 Ratish Philip 
//
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is 
// furnished to do so, subject to the following conditions: 
// 
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software. 
// 
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE. 
//
// This file is part of the WPFSpark project: https://github.com/ratishphilip/wpfspark
//
// WPFSpark v1.2.1
// 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace WPFSpark
{
    /// <summary>
    /// Panel which contains all the headers
    /// </summary>
    public class PivotHeaderPanel : Canvas
    {
        #region Constants

        private const int ADD_FADE_IN_DURATION = 250;
        private const int UPDATE_FADE_IN_DURATION = 50;
        private const int TRANSITION_DURATION = 300;

        #endregion

        #region Events

        public event EventHandler HeaderSelected;

        #endregion

        #region Fields

        readonly Storyboard _addFadeInSb;
        readonly Storyboard _updateFadeInSb;
        readonly List<UIElement> _headerCollection;
        Queue<object[]> _animationQueue;
        bool _isAnimationInProgress;
        readonly CubicEase _easingFn;

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        public PivotHeaderPanel()
        {
            // Define the storyboards
            var addFadeInAnim = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(ADD_FADE_IN_DURATION)));
            Storyboard.SetTargetProperty(addFadeInAnim, new PropertyPath(OpacityProperty));
            _addFadeInSb = new Storyboard();
            _addFadeInSb.Children.Add(addFadeInAnim);

            var updateFadeInAnim = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(UPDATE_FADE_IN_DURATION)));
            Storyboard.SetTargetProperty(updateFadeInAnim, new PropertyPath(OpacityProperty));
            _updateFadeInSb = new Storyboard();
            _updateFadeInSb.Children.Add(updateFadeInAnim);

            _updateFadeInSb.Completed += OnAnimationCompleted;

            _easingFn = new CubicEase
            {
                EasingMode = EasingMode.EaseOut
            };

            // Initialize the header collection
            _headerCollection = new List<UIElement>();
        }

        #endregion

        #region APIs

        /// <summary>
        /// Adds a child to the HeaderPanel
        /// </summary>
        /// <param name="child">Child to be added</param>
        public async void AddChild(UIElement child)
        {
            if (child == null)
                return;

            await Dispatcher.InvokeAsync(() =>
            {
                child.Opacity = 0;
                // Get the Desired size of the child
                child.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));

                // Check if the child needs to be added at the end or inserted in between
                if ((Children.Count == 0) || (Children[Children.Count - 1] == _headerCollection.Last()))
                {
                    child.RenderTransform = CreateTransform(child);
                    Children.Add(child);
                    _headerCollection.Add(child);

                    _addFadeInSb.Begin((FrameworkElement)child);
                }
                else
                {
                    var lastChild = Children[Children.Count - 1];
                    Children.Add(child);
                    var index = _headerCollection.IndexOf(lastChild) + 1;
                    // Insert the new child after the last child in the header collection
                    if (index >= 1)
                    {
                        var newLocationX = ((TranslateTransform)(((TransformGroup)_headerCollection[index].RenderTransform).Children[0])).X;
                        _headerCollection.Insert(index, child);
                        child.RenderTransform = CreateTransform(new Point(newLocationX, 0.0));

                        InsertChild(child, index + 1);
                    }
                }

                // Subscribe to the HeaderItemSelected event and set Active property to false
                var headerItem = child as IPivotHeader;

                if (headerItem != null)
                {
                    headerItem.HeaderItemSelected += OnHeaderItemSelected;
                }
            });
        }

        /// <summary>
        /// Checks if the given UIElement is already added to the Children collection.
        /// </summary>
        /// <param name="child">UIElement</param>
        /// <returns>true/false</returns>
        public bool Contains(UIElement child)
        {
            return Children.Contains(child);
        }

        /// <summary>
        /// Cycles 'count' elements to the left
        /// </summary>
        /// <param name="count">Number of elements to move</param>
        public async Task MoveForward(int count)
        {
            if ((_isAnimationInProgress) || (count <= 0) || (count >= _headerCollection.Count))
                return;

            _isAnimationInProgress = true;

            await Dispatcher.InvokeAsync(() =>
            {
                // Create the animation queue so that the items removed from the beginning 
                // are added in the end in a sequential manner.
                _animationQueue = new Queue<Object[]>();

                lock (_animationQueue)
                {
                    for (var i = 0; i < count; i++)
                    {
                        _animationQueue.Enqueue(new object[] { _headerCollection[i], true });
                    }
                }

                // Get the total width of the first "count" children
                var distanceToMove = ((TranslateTransform)(((TransformGroup)_headerCollection[count].RenderTransform).Children[0])).X;

                // Calculate the new location of each child and create appropriate transition
                foreach (var child in _headerCollection)
                {
                    var oldTranslationX = ((TranslateTransform)(((TransformGroup)child.RenderTransform).Children[0])).X;
                    var newTranslationX = oldTranslationX - distanceToMove;
                    var transition = CreateTransition(child, new Point(newTranslationX, 0.0), TimeSpan.FromMilliseconds(TRANSITION_DURATION), _easingFn);
                    // Process the animation queue once the last child's transition is completed
                    if (child == _headerCollection.Last())
                    {
                        transition.Completed += (s, e) =>
                        {
                            ProcessAnimationQueue();
                        };
                    }
                    transition.Begin();
                }
            });
        }

        /// <summary>
        /// Cycles 'count' elements to the right
        /// </summary>
        /// <param name="count">Number of elements to move</param>
        public async void MoveBack(int count)
        {
            if (_isAnimationInProgress)
                return;

            _isAnimationInProgress = true;

            await Dispatcher.InvokeAsync(() =>
            {
                if ((count <= 0) || (count >= _headerCollection.Count))
                    return;

                // Create the animation queue so that the items removed from the end 
                // are added at the beginning in a sequential manner.
                _animationQueue = new Queue<Object[]>();

                lock (_animationQueue)
                {
                    for (var i = _headerCollection.Count - 1; i >= _headerCollection.Count - count; i--)
                    {
                        _animationQueue.Enqueue(new object[] { _headerCollection[i], false });
                    }
                }

                // Get the total width of the last "count" number of children
                var distanceToMove = ((TranslateTransform)(((TransformGroup)_headerCollection[_headerCollection.Count - 1].RenderTransform).Children[0])).X -
                                        ((TranslateTransform)(((TransformGroup)_headerCollection[_headerCollection.Count - count].RenderTransform).Children[0])).X +
                                        _headerCollection[_headerCollection.Count - 1].DesiredSize.Width;

                // Calculate the new location of each child and create appropriate transition
                foreach (var child in _headerCollection)
                {
                    var oldTranslationX = ((TranslateTransform)(((TransformGroup)child.RenderTransform).Children[0])).X;
                    var newTranslationX = oldTranslationX + distanceToMove;
                    var transition = CreateTransition(child, new Point(newTranslationX, 0.0), TimeSpan.FromMilliseconds(TRANSITION_DURATION), null);
                    // Process the animation queue once the last child's transition is completed
                    if (child == _headerCollection.Last())
                    {
                        transition.Completed += (s, e) =>
                        {
                            ProcessAnimationQueue();
                        };
                    }
                    transition.Begin();
                }
            });
        }

        /// <summary>
        /// Removes all the children from the header
        /// </summary>
        public void ClearHeader()
        {
            foreach (var headerItem in _headerCollection.OfType<IPivotHeader>())
            {
                // Unsubscribe
                headerItem.HeaderItemSelected -= OnHeaderItemSelected;
            }

            _headerCollection.Clear();
            Children.Clear();
        }

        /// <summary>
        /// Resets the location of the header items so that the 
        /// first child that was added is moved to the beginning.
        /// </summary>
        public void Reset()
        {
            if (Children.Count > 0)
            {
                OnHeaderItemSelected(Children[0], null);
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Inserts the child at the specified index.
        /// </summary>
        /// <param name="child">Child to be inserted</param>
        /// <param name="index">Index at where insertion should be performed</param>
        private void InsertChild(UIElement child, int index)
        {
            // Move all the children after the 'index' to the right to accommodate the new child
            for (var i = index; i < _headerCollection.Count; i++)
            {
                var oldTranslationX = ((TranslateTransform)(((TransformGroup)_headerCollection[i].RenderTransform).Children[0])).X;
                var newTranslationX = oldTranslationX + child.DesiredSize.Width;
                _headerCollection[i].RenderTransform = CreateTransform(new Point(newTranslationX, 0.0));
            }

            _addFadeInSb.Begin((FrameworkElement)child);
        }

        /// <summary>
        /// Appends the child at the beginning or the end based on the isDirectionForward flag
        /// </summary>
        /// <param name="child">Child to be appended</param>
        /// <param name="isDirectionForward">Flag to indicate whether the items has to be added at the end or at the beginning</param>
        private void AppendChild(UIElement child, bool isDirectionForward)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                child.Opacity = 0;
                child.RenderTransform = CreateTransform(child, isDirectionForward);
                _headerCollection.Remove(child);
                if (isDirectionForward)
                    _headerCollection.Add(child);
                else
                    _headerCollection.Insert(0, child);

                _updateFadeInSb.Begin((FrameworkElement)child);
            }));
        }

        /// <summary>
        /// Handles the completed event of each animation in the Animation Queue
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        private void OnAnimationCompleted(object sender, EventArgs e)
        {
            lock (_animationQueue)
            {
                if (_animationQueue.Count > 0)
                    _animationQueue.Dequeue();
            }

            ProcessAnimationQueue();
        }

        /// <summary>
        /// Process the animation for the next element in the Animation Queue
        /// </summary>
        private void ProcessAnimationQueue()
        {
            lock (_animationQueue)
            {
                if (_animationQueue.Count > 0)
                {
                    var next = _animationQueue.Peek();
                    var child = (UIElement)next[0];
                    var isDirectionForward = (bool)next[1];
                    AppendChild(child, isDirectionForward);
                }
                else
                {
                    _isAnimationInProgress = false;
                }
            }
        }

        /// <summary>
        /// Gets the position available before the first child
        /// </summary>
        /// <returns>Distance on the X-axis</returns>
        private double GetFirstChildPosition()
        {
            var transX = 0.0;

            // Get the first child in the headerCollection
            var firstChild = _headerCollection.FirstOrDefault();
            if (firstChild != null)
                transX = ((TranslateTransform)(((TransformGroup)firstChild.RenderTransform).Children[0])).X;

            return transX;
        }

        /// <summary>
        /// Gets the position available after the last child
        /// </summary>
        /// <returns>Distance on the X-axis</returns>
        private double GetNextAvailablePosition()
        {
            var transX = 0.0;
            // Get the last child in the headerCollection 
            var lastChild = _headerCollection.LastOrDefault();
            // Add the X-Location of the child + its Desired width to get the next child's position
            if (lastChild != null)
                transX = ((TranslateTransform)(((TransformGroup)lastChild.RenderTransform).Children[0])).X + lastChild.DesiredSize.Width;

            return transX;
        }

        /// <summary>
        /// Creates a translation transform for the child so that it can be placed
        /// at the beginning or the end.
        /// </summary>
        /// <param name="child">Item to be translated</param>
        /// <param name="isDirectionForward">Flag to indicate whether the items has to be added at the end or at the beginning</param>
        /// <returns>TransformGroup</returns>
        private TransformGroup CreateTransform(UIElement child, bool isDirectionForward = true)
        {
            if (child == null)
                return null;

            var transX = 0.0;
            if (isDirectionForward)
                transX = GetNextAvailablePosition();
            else
                // All the children have moved forward to make space for the children to be
                // added in the beginning of the header collection. So calculate the 
                // child's location by subtracting its width from the first child's location
                transX = GetFirstChildPosition() - child.DesiredSize.Width;

            var translation = new TranslateTransform
            {
                X = transX,
                Y = 0.0
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translation);

            return transformGroup;
        }

        /// <summary>
        /// Creates a translation transform
        /// </summary>
        /// <param name="translation">Translation value</param>
        /// <returns>TransformGroup</returns>
        private static TransformGroup CreateTransform(Point translation)
        {
            var translateTransform = new TranslateTransform
            {
                X = translation.X,
                Y = translation.Y
            };

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(translateTransform);

            return transformGroup;
        }

        /// <summary>
        /// Creates the animation for translating the element
        /// to a new location
        /// </summary>
        /// <param name="element">Item to be translated</param>
        /// <param name="translation">Translation value</param>
        /// <param name="period">Translation duration</param>
        /// <param name="easing">Easing function</param>
        /// <returns>Storyboard</returns>
        private static Storyboard CreateTransition(UIElement element, Point translation, TimeSpan period, EasingFunctionBase easing)
        {
            Duration duration = new Duration(period);

            // Animate X
            var translateAnimationX = new DoubleAnimation();
            translateAnimationX.To = translation.X;
            translateAnimationX.Duration = duration;
            if (easing != null)
                translateAnimationX.EasingFunction = easing;

            Storyboard.SetTarget(translateAnimationX, element);
            Storyboard.SetTargetProperty(translateAnimationX,
                new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));

            var sb = new Storyboard();
            sb.Children.Add(translateAnimationX);

            return sb;
        }

        #endregion

        #region EventHandlers

        /// <summary>
        /// Handles the HeaderItemSelected event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">EventArgs</param>
        async void OnHeaderItemSelected(object sender, EventArgs e)
        {
            if ((_isAnimationInProgress) || (_headerCollection == null) || (_headerCollection.Count == 0))
                return;

            var child = sender as UIElement;
            if (child == null)
                return;

            // Check if the header selected is not the first header
            var index = _headerCollection.IndexOf(child);
            if (index <= 0)
                return;

            // Move the selected header to the left most position
            await MoveForward(index);

            // Raise the HeaderSelected event
            HeaderSelected?.Invoke(child, new EventArgs());
        }

        #endregion        
    }
}
