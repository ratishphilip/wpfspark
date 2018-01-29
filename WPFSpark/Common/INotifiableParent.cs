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

namespace WPFSpark
{
    /// <summary>
    /// Interface for a Notifiable Parent
    /// </summary>
    public interface INotifiableParent
    {
        int AddChild(UIElement child);
    }
}
