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
// WPFSpark v1.3
// 

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace WPFSpark
{
    /// <summary>
    /// Interaction logic for FluidStatusBar.xaml
    /// </summary>
    public partial class FluidStatusBar : UserControl
    {
        #region Enums

        private enum EngineState
        {
            Ready,
            Processing,
            Scheduled
        }

        #endregion

        #region Constants

        public const double MinFadeOutDistance = 100.0;
        public static readonly Duration MinDuration = new Duration(TimeSpan.FromMilliseconds(300));

        #endregion

        #region Fields

        Storyboard fadeInOutSB = null;
        Storyboard fadeInSB = null;

        Storyboard fadeOutLeftSB = null;
        Storyboard fadeOutRightSB = null;
        Storyboard fadeOutUpSB = null;
        Storyboard fadeOutDownSB = null;

        Queue<StatusMessage> messageQueue = null;
        private StatusMessage _msgToProcess;
        private object _syncObject = new object();
        private EngineState _state;

        #endregion

        #region DependencyProperties

        #region FadeOutDirection

        /// <summary>
        /// FadeOutDirection Dependency Property
        /// </summary>
        public static readonly DependencyProperty FadeOutDirectionProperty =
            DependencyProperty.Register("FadeOutDirection", typeof(StatusDirection), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(StatusDirection.Left, OnFadeOutDirectionChanged));

        /// <summary>
        /// Gets or sets the FadeOutDirection property. This dependency property 
        /// indicates the direction in which the old text should fade out.
        /// </summary>
        public StatusDirection FadeOutDirection
        {
            get { return (StatusDirection)GetValue(FadeOutDirectionProperty); }
            set { SetValue(FadeOutDirectionProperty, value); }
        }

        /// <summary>
        /// Handles changes to the FadeOutDirection property.
        /// </summary>
        /// <param name="d">FluidStatusBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnFadeOutDirectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fsBar = (FluidStatusBar)d;
            var oldFadeOutDirection = (StatusDirection)e.OldValue;
            var newFadeOutDirection = fsBar.FadeOutDirection;
            fsBar.OnFadeOutDirectionChanged(oldFadeOutDirection, newFadeOutDirection);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the FadeOutDirection property.
        /// </summary>
        /// <param name="oldFadeOutDirection">Old Value</param>
        /// <param name="newFadeOutDirection">New Value</param>
        protected void OnFadeOutDirectionChanged(StatusDirection oldFadeOutDirection, StatusDirection newFadeOutDirection)
        {
            switch (newFadeOutDirection)
            {
                case StatusDirection.Right:
                    fadeInOutSB = fadeOutRightSB;
                    break;
                case StatusDirection.Up:
                    fadeInOutSB = fadeOutUpSB;
                    break;
                case StatusDirection.Down:
                    fadeInOutSB = fadeOutDownSB;
                    break;
                default:
                    fadeInOutSB = fadeOutLeftSB;
                    break;
            }
        }

        #endregion

        #region FadeOutDistance

        /// <summary>
        /// FadeOutDistance Dependency Property
        /// </summary>
        public static readonly DependencyProperty FadeOutDistanceProperty =
            DependencyProperty.Register("FadeOutDistance", typeof(double), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(MinFadeOutDistance, OnFadeOutDistanceChanged));

        /// <summary>
        /// Gets or sets the FadeOutDistance property. This dependency property 
        /// indicates the width of the fade out animation.
        /// </summary>
        public double FadeOutDistance
        {
            get { return (double)GetValue(FadeOutDistanceProperty); }
            set { SetValue(FadeOutDistanceProperty, value); }
        }

        /// <summary>
        /// Handles changes to the FadeOutDistance property.
        /// </summary>
        /// <param name="d">FluidStatusBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnFadeOutDistanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fsBar = (FluidStatusBar)d;
            var oldFadeOutDistance = (double)e.OldValue;
            var newFadeOutDistance = fsBar.FadeOutDistance;
            fsBar.OnFadeOutDistanceChanged(oldFadeOutDistance, newFadeOutDistance);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the FadeOutDistance property.
        /// </summary>
        /// <param name="oldFadeOutDistance">Old Value</param>
        /// <param name="newFadeOutDistance">New Value</param>
        protected void OnFadeOutDistanceChanged(double oldFadeOutDistance, double newFadeOutDistance)
        {
            UpdateFadeOutDistance(fadeOutLeftSB, new Thickness(0, 0, newFadeOutDistance, 0));
            UpdateFadeOutDistance(fadeOutUpSB, new Thickness(0, 0, 0, newFadeOutDistance));
            UpdateFadeOutDistance(fadeOutRightSB, new Thickness(newFadeOutDistance, 0, 0, 0));
            UpdateFadeOutDistance(fadeOutDownSB, new Thickness(0, newFadeOutDistance, 0, 0));
        }

        #endregion

        #region FadeOutDuration

        /// <summary>
        /// FadeOutDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty FadeOutDurationProperty =
            DependencyProperty.Register("FadeOutDuration", typeof(Duration), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(MinDuration, OnFadeOutDurationChanged));

        /// <summary>
        /// Gets or sets the FadeOutDuration property. This dependency property 
        /// indicates the duration for fading out the text.
        /// </summary>
        public Duration FadeOutDuration
        {
            get { return (Duration)GetValue(FadeOutDurationProperty); }
            set { SetValue(FadeOutDurationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the FadeOutDuration property.
        /// </summary>
        /// <param name="d">FluidStatusBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnFadeOutDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fsBar = (FluidStatusBar)d;
            var oldFadeOutDuration = (Duration)e.OldValue;
            var newFadeOutDuration = fsBar.FadeOutDuration;
            fsBar.OnFadeOutDurationChanged(oldFadeOutDuration, newFadeOutDuration);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the FadeOutDuration property.
        /// </summary>
        /// <param name="oldFadeOutDuration">Old Value</param>
        /// <param name="newFadeOutDuration">New Value</param>
        protected void OnFadeOutDurationChanged(Duration oldFadeOutDuration, Duration newFadeOutDuration)
        {
            UpdateFadeOutDuration(fadeOutLeftSB, newFadeOutDuration);
            UpdateFadeOutDuration(fadeOutRightSB, newFadeOutDuration);
            UpdateFadeOutDuration(fadeOutUpSB, newFadeOutDuration);
            UpdateFadeOutDuration(fadeOutDownSB, newFadeOutDuration);
        }

        #endregion

        #region MoveDuration

        /// <summary>
        /// MoveDuration Dependency Property
        /// </summary>
        public static readonly DependencyProperty MoveDurationProperty =
            DependencyProperty.Register("MoveDuration", typeof(Duration), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(MinDuration, OnMoveDurationChanged));

        /// <summary>
        /// Gets or sets the MoveDuration property. This dependency property 
        /// indicates the duration for moving the text while fading out.
        /// </summary>
        public Duration MoveDuration
        {
            get { return (Duration)GetValue(MoveDurationProperty); }
            set { SetValue(MoveDurationProperty, value); }
        }

        /// <summary>
        /// Handles changes to the MoveDuration property.
        /// </summary>
        /// <param name="d">FluidStatusBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnMoveDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var fsBar = (FluidStatusBar)d;
            var oldMoveDuration = (Duration)e.OldValue;
            var newMoveDuration = fsBar.MoveDuration;
            fsBar.OnMoveDurationChanged(oldMoveDuration, newMoveDuration);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the MoveDuration property.
        /// </summary>
        /// <param name="oldMoveDuration">Old Value</param>
        /// <param name="newMoveDuration">New Value</param>
        protected void OnMoveDurationChanged(Duration oldMoveDuration, Duration newMoveDuration)
        {
            UpdateMoveDuration(fadeOutLeftSB, newMoveDuration);
            UpdateMoveDuration(fadeOutRightSB, newMoveDuration);
            UpdateMoveDuration(fadeOutUpSB, newMoveDuration);
            UpdateMoveDuration(fadeOutDownSB, newMoveDuration);
        }

        #endregion

        #region TextHorizontalAlignment

        /// <summary>
        /// TextHorizontalAlignment Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextHorizontalAlignmentProperty =
            DependencyProperty.Register("TextHorizontalAlignment", typeof(HorizontalAlignment), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(HorizontalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the TextHorizontalAlignment property. This dependency property 
        /// indicates the alignment of the Text in the FluidStatusBar.
        /// </summary>
        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(TextHorizontalAlignmentProperty); }
            set { SetValue(TextHorizontalAlignmentProperty, value); }
        }

        #endregion

        #region TextVerticalAlignment

        /// <summary>
        /// TextVerticalAlignment Dependency Property
        /// </summary>
        public static readonly DependencyProperty TextVerticalAlignmentProperty =
            DependencyProperty.Register("TextVerticalAlignment", typeof(VerticalAlignment), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(VerticalAlignment.Center, FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// Gets or sets the TextVerticalAlignment property. This dependency property 
        /// indicates the VerticalAlignment of the status message in the FluidStatusBar.
        /// </summary>
        public VerticalAlignment TextVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(TextVerticalAlignmentProperty); }
            set { SetValue(TextVerticalAlignmentProperty, value); }
        }

        #endregion

        #region Message

        /// <summary>
        /// Message Dependency Property
        /// </summary>
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(StatusMessage), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(OnMessageChanged));

        /// <summary>
        /// Gets or sets the Message property. This dependency property 
        /// indicates the message to be displayed in the FluidStatusBar.
        /// </summary>
        public StatusMessage Message
        {
            get { return (StatusMessage)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        /// <summary>
        /// Handles changes to the Message property.
        /// </summary>
        /// <param name="d">FluidStatusBar</param>
        /// <param name="e">DependencyProperty changed event arguments</param>
        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var statusBar = (FluidStatusBar)d;
            var oldMessage = (StatusMessage)e.OldValue;
            var newMessage = statusBar.Message;
            statusBar.OnMessageChanged(oldMessage, newMessage);
        }

        /// <summary>
        /// Provides derived classes an opportunity to handle changes to the Message property.
        /// </summary>
        /// <param name="oldMessage">Old Value</param>
        /// <param name="newMessage">New Value</param>
        protected virtual void OnMessageChanged(StatusMessage oldMessage, StatusMessage newMessage)
        {
            SetStatus(newMessage ?? StatusMessage.Empty);
        }

        #endregion

        #region SyncLatest

        /// <summary>
        /// SyncLatest Dependency Property
        /// </summary>
        public static readonly DependencyProperty SyncLatestProperty =
            DependencyProperty.Register("SyncLatest", typeof(bool), typeof(FluidStatusBar),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the SyncLatest property. This dependency property 
        /// indicates whether the display of status message is synced with the latest message.
        /// That is, when messages arrive, they are not enqueued to be displayed. Instead
        /// the latest one is displayed and the older ones are discarded.
        /// </summary>
        public bool SyncLatest
        {
            get { return (bool)GetValue(SyncLatestProperty); }
            set { SetValue(SyncLatestProperty, value); }
        }

        #endregion

        #endregion

        #region Construction / Initialization

        public FluidStatusBar()
        {
            InitializeComponent();

            _state = EngineState.Ready;
            _msgToProcess = StatusMessage.Empty;
            messageQueue = new Queue<StatusMessage>();

            GetStoryboards();
        }

        #endregion

        #region APIs

        /// <summary>
        /// Sets the new status message in the status bar.
        /// </summary>
        /// <param name="statusMsg">New Status Message</param>
        public void SetStatus(StatusMessage statusMsg)
        {
            if (statusMsg == null)
                return;

            SetStatus(statusMsg.Message, statusMsg.IsAnimated);
        }

        /// <summary>
        /// Sets the new status message in the status bar.
        /// </summary>
        /// <param name="message">New message to be displayed</param>
        /// <param name="isAnimated">Flag to indicate whether the old status message 
        /// should be animated when it fades out</param>
        public void SetStatus(string message, bool isAnimated)
        {
            lock (_syncObject)
            {
                if (SyncLatest)
                {
                    _msgToProcess.Update(message, isAnimated);
                }
                else
                {
                    messageQueue.Enqueue(new StatusMessage(message, isAnimated));
                }
            }

            PreProcess();
        }

        #endregion

        #region Helpers

        private void GetStoryboards()
        {
            fadeOutLeftSB = (Storyboard)Resources["FadeInOutLeftStoryboard"] as Storyboard;
            if (fadeOutLeftSB != null)
            {
                fadeOutLeftSB.Completed += PostProcess;
            }

            fadeOutRightSB = Resources["FadeInOutRightStoryboard"] as Storyboard;
            if (fadeOutRightSB != null)
            {
                fadeOutRightSB.Completed += PostProcess;
            }

            fadeOutUpSB = Resources["FadeInOutUpStoryboard"] as Storyboard;
            if (fadeOutUpSB != null)
            {
                fadeOutUpSB.Completed += PostProcess;
            }

            fadeOutDownSB = Resources["FadeInOutDownStoryboard"] as Storyboard;
            if (fadeOutDownSB != null)
            {
                fadeOutDownSB.Completed += PostProcess;
            }

            fadeInSB = Resources["FadeInStoryboard"] as Storyboard;
            if (fadeInSB != null)
            {
                fadeInSB.Completed += PostProcess;
            }

            fadeInOutSB = fadeOutLeftSB;
        }

        private void UpdateFadeOutDistance(Storyboard sb, Thickness thickness)
        {
            if (sb != null)
            {
                foreach (Timeline timeline in sb.Children)
                {
                    ThicknessAnimation anim = timeline as ThicknessAnimation;
                    if (anim != null)
                    {
                        anim.SetValue(ThicknessAnimation.ToProperty, thickness);
                    }
                }
            }
        }

        private void UpdateMoveDuration(Storyboard sb, Duration duration)
        {
            if (sb != null)
            {
                foreach (Timeline timeline in sb.Children)
                {
                    ThicknessAnimation anim = timeline as ThicknessAnimation;
                    if (anim != null)
                    {
                        anim.SetValue(ThicknessAnimation.DurationProperty, duration);
                    }
                }
            }
        }

        private void UpdateFadeOutDuration(Storyboard sb, Duration duration)
        {
            if (sb != null)
            {
                foreach (Timeline timeline in sb.Children)
                {
                    DoubleAnimation anim = timeline as DoubleAnimation;
                    if (anim != null)
                    {
                        anim.SetValue(DoubleAnimation.DurationProperty, duration);
                    }
                }
            }
        }

        private void PreProcess()
        {
            switch (_state)
            {
                case EngineState.Ready:
                    _state = EngineState.Processing;
                    Process();
                    break;
                case EngineState.Processing:
                    _state = EngineState.Scheduled;
                    break;
                case EngineState.Scheduled:
                    break;
            }
        }

        private void Process()
        {
            if (SyncLatest)
            {
                if (_msgToProcess.IsAnimated)
                {
                    // Copy the text to begin fade out
                    FadeOutTextBlock.Text = FadeInTextBlock.Text;
                    FadeInTextBlock.Text = _msgToProcess.Message;

                    fadeInOutSB?.Begin();
                }
                else
                {
                    FadeInTextBlock.Text = _msgToProcess.Message;
                    fadeInSB?.Begin();
                }
            }
            else
            {
                if (messageQueue.Count <= 0)
                {
                    PostProcess(null, null);
                    return;
                }

                var msg = StatusMessage.Empty;
                lock (_syncObject)
                {
                    msg = messageQueue.Peek();
                }

                if (msg.IsAnimated)
                {
                    // Copy the text to begin fade out
                    FadeOutTextBlock.Text = FadeInTextBlock.Text;
                    FadeInTextBlock.Text = msg.Message;

                    fadeInOutSB?.Begin();
                }
                else
                {
                    FadeInTextBlock.Text = msg.Message;
                    fadeInSB?.Begin();
                }
            }
        }

        #endregion

        #region Event Handlers

        private void PostProcess(object sender, EventArgs e)
        {
            if (SyncLatest)
            {
                switch (_state)
                {
                    case EngineState.Ready:
                        // Do Nothing
                        break;
                    case EngineState.Processing:
                        // No more messages waiting
                        _state = EngineState.Ready;
                        break;
                    case EngineState.Scheduled:
                        // New Message is waiting in the pipeline to be displayed
                        _state = EngineState.Processing;
                        Process();
                        break;
                }
            }
            else
            {
                lock (_syncObject)
                {
                    // Deque the message which has been displayed
                    if (messageQueue.Count > 0)
                        messageQueue.Dequeue();
                }

                if (messageQueue.Count > 0)
                {
                    // process the next message in the messageQueue
                    _state = EngineState.Processing;
                    Process();
                }
                else
                {
                    // No more messages waiting
                    _state = EngineState.Ready;
                }
            }
        }

        #endregion
    }
}
