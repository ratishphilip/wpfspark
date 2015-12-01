#region File Header

// -------------------------------------------------------------------------------
// 
// This file is part of the WPFSpark project: http://wpfspark.codeplex.com/
//
// Author: Ratish Philip
// 
// WPFSpark v1.1
//
// -------------------------------------------------------------------------------

#endregion

using System.Windows;
using System.Windows.Controls;

namespace WPFSpark
{
    public class NotifiableUIElementCollection : UIElementCollection
    {
        #region Fields

        private INotifiableParent parent; 

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="visualParent">Visual Parent</param>
        /// <param name="logicalParent">Logical Parent</param>
        public NotifiableUIElementCollection(UIElement visualParent, FrameworkElement logicalParent)
            : base(visualParent, logicalParent)
        {
            parent = (INotifiableParent)logicalParent;
        }
        
        #endregion

        #region Overrides

        /// <summary>
        /// Adds a child to the parent
        /// </summary>
        /// <param name="element">child element</param>
        /// <returns></returns>
        public override int Add(System.Windows.UIElement element)
        {
            if (parent != null)
                return parent.AddChild(element);

            return -1;
        }
        
        #endregion
    }
}
