// Copyright (c) 2017 Ratish Philip 
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
// WPFSpark v1.3.1
// 

using System;
using System.Windows;
using System.Windows.Controls;

namespace WPFSpark
{
    /// <summary>
    /// Represents a bit location within the BitMatrix
    /// </summary>
    internal struct MatrixCell
    {
        internal long Row;
        internal long Col;

        internal MatrixCell(long row, long col)
        {
            Row = row;
            Col = col;
        }

        internal bool IsValid()
        {
            return (Row >= 0) && (Col >= 0);
        }

        internal static MatrixCell InvalidCell()
        {
            return new MatrixCell(-1, -1);
        }
    }

    /// <summary>
    /// Encapsulates bit based representation of data using
    /// 64 bit unsigned long numbers. In case of Vertical
    /// Orientation the matrix is stored as a transposed matrix i.e.
    /// even though from the outside it appears as a M x N matrix,
    /// internally it is stored and processed as a N x M matrix.
    /// </summary>
    internal sealed class FluidBitMatrix
    {
        #region Constants

        // Maximum number of bits an item can occupy in a resultRow
        private const int MaxBitsPerItem = 60;
        // Number of bits in each cell
        private const int BitsPerCell = 64;
        // Based on BitsPerCell. 2 ^ ShiftIndex = BitsPerCell
        private const int ShiftIndex = 6;

        #endregion

        #region Fields

        // Represents the total rows in the matrix
        private readonly long _rowsInternal;
        // Represents  the total columns in the matrix
        private readonly long _columnsInternal;
        // Storage for the bits
        private readonly UInt64[] _data;
        // Stores the mask for each bit within a cell
        private static readonly UInt64[] Mask;
        // Number of Cells required for each resultRow. If _columnsInternal is
        // not a multiple of BitsPerCell, then add an additional cell
        // to each resultRow
        // Each column begins with a new UInt64
        // _cellsPerRow = (_columnsInternal / BitsPerCell) + 1
        private readonly long _cellsPerRow;

        #endregion

        #region Properties

        /// <summary>
        /// Represents the number of Rows in the matrix. 
        /// </summary>
        internal long Rows => (BitOrientation == Orientation.Horizontal) ? _rowsInternal : _columnsInternal;
        /// <summary>
        /// Represents the number of Columns in the matrix
        /// </summary>
        internal long Columns => (BitOrientation == Orientation.Horizontal) ? _columnsInternal : _rowsInternal;
        /// <summary>
        /// Represents the orientation of the matrix
        /// </summary>
        internal Orientation BitOrientation { get; }

        #endregion

        #region Construction / Initialization

        /// <summary>
        /// Static Ctor
        /// </summary>
        static FluidBitMatrix()
        {
            // Define the mask bits
            Mask = new UInt64[BitsPerCell];
            for (var i = 0; i < BitsPerCell; i++)
            {
                Mask[i] = (UInt64)1 << i;
            }
        }

        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="rows">Number of Rows</param>
        /// <param name="columns">Number of Columns</param>
        /// <param name="orientation">Horizontal or Vertical</param>
        internal FluidBitMatrix(long rows, long columns, Orientation orientation = Orientation.Horizontal)
        {
            switch (orientation)
            {
                // In case of Horizontal Orientation, Create and process a 
                // M x N matrix
                case Orientation.Horizontal:
                    _rowsInternal = rows;
                    _columnsInternal = columns;
                    break;
                // In case of Vertical Orientation, the matrix is stored as 
                // a transposed matrix i.e. even though from the outside it 
                // appears as a M x N matrix, internally it is created and 
                // processed as a N x M matrix.
                case Orientation.Vertical:
                    _rowsInternal = columns;
                    _columnsInternal = rows;
                    break;
            }
            // Orientation
            BitOrientation = orientation;
            // Calculate number of 64 bit cells required to represent 
            // the columns in a single row
            _cellsPerRow = (_columnsInternal + (BitsPerCell - 1)) >> ShiftIndex;
            // Total cells
            var cellCount = _rowsInternal * _cellsPerRow;
            _data = new UInt64[cellCount];
        }

        #endregion

        #region APIs

        /// <summary>
        /// Tries to find an empty region of given width and height starting
        /// from the startIndex row.
        /// </summary>
        /// <param name="startIndex">The row to start the search from</param>
        /// <param name="width">Width of the Region</param>
        /// <param name="height">Height of the Region</param>
        /// <param name="cell">The cell location</param>
        /// <returns>true if successful otherwise false</returns>
        internal bool TryFindRegion(long startIndex, int width, int height, out MatrixCell cell)
        {
            cell = MatrixCell.InvalidCell();

            // Swap width and height if the BitOrientation is Vertical
            if ((BitOrientation == Orientation.Vertical) && (width != height))
            {
                var temp = width;
                width = height;
                height = temp;
            }

            if ((startIndex < 0 || startIndex >= _rowsInternal) ||
                (startIndex + (height - 1) >= _rowsInternal) ||
                ((width < 1) || (width > MaxBitsPerItem)) ||
                (width > _columnsInternal))
                return false;

            // Optimization: If both width and height are 1 then use a faster 
            // loop to find the next empty cell
            if ((width == 1) && (height == 1))
            {
                for (var row = startIndex; row < _rowsInternal; row++)
                {
                    for (var col = 0; col < _columnsInternal; col++)
                    {
                        // Is the cell unset?
                        if (this[row, col])
                            continue;

                        // Swap the row and col values if the BitOrientation is Vertical
                        cell = (BitOrientation == Orientation.Horizontal) ? new MatrixCell(row, col) : new MatrixCell(col, row);
                        return true;
                    }
                }

                // If the code has reached here it means that it did not find any unset
                // bit in the entire matrix. Return false from here.
                return false;
            }

            var mask = (((UInt64)1) << width) - 1;
            for (var row = startIndex; row < (_rowsInternal - height + 1); row++)
            {
                // Quickcheck: If the row is empty then no need to check individual bits in the row
                if (!RowHasData(row))
                {
                    // Current column has no bits set. Check the bits in the next (height - 1)
                    // rows starting at the same column position (0) to check if they are unset
                    if (!AnyBitsSetInRegion(row, 0, width, height))
                    {
                        // Swap the row and col values if the BitOrientation is Vertical
                        cell = (BitOrientation == Orientation.Horizontal) ? new MatrixCell(row, 0) : new MatrixCell(0, row);
                        return true;
                    }
                }

                // Row is not empty and has some set bits.  Check if there are
                // 'width' continuous unset bits in the column.
                // 1. Check the first 'width' bits
                var rowData = 0UL;
                var col = 0;
                for (; col < width; col++)
                {
                    rowData <<= 1;
                    rowData |= this[row, col] ? 1UL : 0UL;
                }
                if ((rowData & mask) == 0)
                {
                    // Current column has 'width' unset bits starting from 0 position.
                    // Check the bits in the next (height - 1) rows starting at the same
                    // column position (0) to check if they are unset
                    if (!AnyBitsSetInRegion(row, 0, width, height))
                    {
                        // Swap the row and col values if the BitOrientation is Vertical
                        cell = (BitOrientation == Orientation.Horizontal) ? new MatrixCell(row, 0) : new MatrixCell(0, row);
                        return true;
                    }
                }

                // Shift the rowData by 1 bit and clear the (width + 1)th most significant bit.
                // This way a set of 'width' continuous bits is matched against the mask 
                // to check if all the bits are zero.
                var colBegin = 0;
                while (col < _columnsInternal)
                {
                    rowData <<= 1;
                    rowData &= mask;
                    rowData |= this[row, col++] ? 1UL : 0UL;
                    colBegin++;

                    if ((rowData & mask) != 0)
                        continue;

                    // Current column has 'width' unset bits starting from colBegin position.
                    // Check the bits in the next (height - 1) rows starting at the same
                    // column position (colBegin) to check if they are unset
                    if (AnyBitsSetInRegion(row, colBegin, width, height))
                        continue;

                    // Swap the row and col values if the BitOrientation is Vertical
                    cell = (BitOrientation == Orientation.Horizontal) ? new MatrixCell(row, colBegin) : new MatrixCell(colBegin, row);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the bits in the region starting at location and having 
        /// given width and height
        /// </summary>
        /// <param name="location">Top Left of the the Region</param>
        /// <param name="width">Width of the Region</param>
        /// <param name="height">Height of the Region</param>
        internal void SetRegion(MatrixCell location, int width, int height)
        {
            long targetRow = location.Row;
            long targetCol = location.Col;
            int targetWidth = width;
            int targetHeight = height;

            // Interchange Row & Column and width & height if the BitOrientation is Vertical
            if (BitOrientation == Orientation.Vertical)
            {
                targetRow = location.Col;
                targetCol = location.Row;
                targetWidth = height;
                targetHeight = width;
            }

            // Optimization: If the region is only 1 bit wide and high
            if ((targetWidth == 1) && (targetHeight == 1))
            {
                this[targetRow, targetCol] = true;
                return;
            }

            for (var row = 0; row < targetHeight; row++)
            {
                for (var col = 0; col < targetWidth; col++)
                {
                    this[targetRow + row, targetCol + col] = true;
                }
            }
        }

        /// <summary>
        /// Gets the width and height of region encapsulating all the 
        /// set bits in the matrix
        /// </summary>
        /// <returns>Size of the region</returns>
        internal Size GetFilledMatrixDimensions()
        {
            return (BitOrientation == Orientation.Horizontal) ?
                    new Size(_columnsInternal, GetLastRow()) :
                    new Size(GetLastRow(), _columnsInternal);
        }

        /// <summary>
        /// Resets all the bits in the matrix
        /// </summary>
        internal void ResetMatrix()
        {
            for (long i = 0; i < _data.LongLength; i++)
            {
                _data[i] = 0UL;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Bit accessor
        /// </summary>
        /// <param name="row">Row</param>
        /// <param name="column">Column</param>
        /// <returns>True if the bit is set, otherwise False</returns>
        private bool this[long row, long column]
        {
            get
            {
                if (row < 0 || row >= _rowsInternal)
                    throw new ArgumentOutOfRangeException(nameof(row));
                if (column < 0 || column >= _columnsInternal)
                    throw new ArgumentOutOfRangeException(nameof(column));

                // Calculate the offset to get the cell containing the required bit
                var offset = row * _cellsPerRow + (column >> ShiftIndex);
                // To obtain the mask offset, get (resultColumn % 64), which can be
                // obtained faster by doing (resultColumn AND 63) i.e. (resultColumn & 0x3F)
                var maskOffset = column & 0x3F;
                return (_data[offset] & Mask[maskOffset]) != 0;
            }
            set
            {
                if (row < 0 || row >= _rowsInternal)
                    throw new ArgumentOutOfRangeException(nameof(row));
                if (column < 0 || column >= _columnsInternal)
                    throw new ArgumentOutOfRangeException(nameof(column));

                // Calculate the offset to get the cell containing the required bit
                var offset = row * _cellsPerRow + (column >> ShiftIndex);
                // To obtain the mask offset, get (resultColumn % 64), which can be
                // obtained faster by doing (resultColumn AND 63) i.e. (resultColumn & 0x3F)
                var maskOffset = column & 0x3F;

                if (value)
                {
                    // Set bit
                    _data[offset] |= Mask[maskOffset];
                }
                else
                {
                    // Reset bit
                    _data[offset] &= ~(Mask[maskOffset]);
                }
            }
        }

        /// <summary>
        /// Checks if the given row has any bit set.
        /// </summary>
        /// <param name="row">Row number</param>
        /// <returns>True if any bit in the row is set otherwise False</returns>
        private bool RowHasData(long row)
        {
            // TODO: This check can be commented since this is a private method which is called after boundary check is done
            if (row < 0 || row >= _rowsInternal)
                throw new ArgumentOutOfRangeException(nameof(row));

            var baseCell = row * _cellsPerRow;
            for (var i = 0; i < _cellsPerRow; i++)
            {
                if (_data[baseCell + i] > 0)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Checks if the region starting at (column, col) & having a width and 
        /// height of 'width' and 'height' respectively has any set bits.
        /// </summary>
        /// <param name="row">one column below the Row in which the region starts</param>
        /// <param name="col">Column in which the region starts</param>
        /// <param name="width">Width of the region</param>
        /// <param name="height">Height of the region</param>
        /// <returns>True if any bits are set in the region, otherwise False</returns>
        private bool AnyBitsSetInRegion(long row, long col, int width, int height)
        {
            var mask = (((UInt64)1) << width) - 1;
            var rowData = (UInt64)0;
            var hasSetBits = false;

            // Since the first column of the region is already verified to contain 'width' 
            // contiguous unset bit, check the remaining of the rows
            for (var rowOffset = 1; rowOffset < height; rowOffset++)
            {
                // Quickcheck: If the row is empty then no need to check individual bits in the row
                if (!RowHasData(row + rowOffset))
                {
                    continue;
                }

                // Check if there are 'width' continuous unset bits in the the column at the 'col' position
                for (var colOffset = 0; colOffset < width; colOffset++)
                {
                    rowData <<= 1;
                    rowData |= this[row + rowOffset, col + colOffset] ? 1UL : 0UL;
                }

                // If the bit is not set, move to next bit
                if ((rowData & mask) == 0)
                {
                    continue;
                }

                hasSetBits = true;
                break;
            }

            return hasSetBits;
        }

        /// <summary>
        /// Gets the index of the last row that has set bits
        /// </summary>
        /// <returns>Row index</returns>
        private long GetLastRow()
        {
            var row = _rowsInternal - 1;

            while ((row >= 0) && !RowHasData(row))
                row--;

            // Add 1 as row is a zero-based index
            return row + 1;
        }

        #endregion
    }
}
