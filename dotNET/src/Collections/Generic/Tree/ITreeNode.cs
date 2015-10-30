/* *
 * Copyright (C) 2015 Christopher Herrick
 * 
 * This file is part of the FluxLib library.
 *
 * The FluxLib library is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.

 * The FluxLib library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.

 * You should have received a copy of the GNU Lesser General Public License
 * along with the FluxLib library.  If not, see <http://www.gnu.org/licenses/>.
 * */

using System;
using System.Collections.Generic;

namespace FluxLib.Collections.Generic.Tree
{
   public interface ITreeNode<NodeValueType, ConcreteTreeNodeType> : IEquatable<ConcreteTreeNodeType>
      where ConcreteTreeNodeType : ITreeNode<NodeValueType, ConcreteTreeNodeType>
   {
      NodeValueType Value
      {
         get;
      }

      Boolean HasChildren
      {
         get;
      }

      Boolean HasChild( NodeValueType value );

      Boolean HasChild( ConcreteTreeNodeType node );

      Boolean HasDescendent( NodeValueType value );

      Boolean HasDescendent( ConcreteTreeNodeType node );

      Boolean HasAncestor( NodeValueType value );

      Boolean HasAncestor( ConcreteTreeNodeType node );

      Boolean HasParent
      {
         get;
      }

      Boolean IsRoot
      {
         get;
      }

      Boolean IsLeaf
      {
         get;
      }

      ConcreteTreeNodeType Parent
      {
         get;
      }

      IList<ConcreteTreeNodeType> Children
      {
         get;
      }

      Int32 ChildrenCount
      {
         get;
      }

      Int32 DescendentCount
      {
         get;
      }

      Int32 Depth
      {
         get;
      }

      void Clear();

      #region Enumeration
      IEnumerator<NodeValueType> DepthFirstNodeValueEnumerator( EnumerationDirection direction );
      IEnumerator<NodeValueType> ForwardDepthFirstNodeValueEnumerator();
      IEnumerator<NodeValueType> ReverseDepthFirstNodeValueEnumerator();
      IEnumerator<NodeValueType> BreadthFirstNodeValueEnumerator( EnumerationDirection direction );
      IEnumerator<NodeValueType> ForwardBreadthFirstNodeValueEnumerator();
      IEnumerator<NodeValueType> ReverseBreadthFirstNodeValueEnumerator();
      IEnumerator<ConcreteTreeNodeType> DepthFirstNodeEnumerator( EnumerationDirection direction );
      IEnumerator<ConcreteTreeNodeType> ForwardDepthFirstNodeEnumerator();
      IEnumerator<ConcreteTreeNodeType> ReverseDepthFirstNodeEnumerator();
      IEnumerator<ConcreteTreeNodeType> BreadthFirstNodeEnumerator( EnumerationDirection direction );
      IEnumerator<ConcreteTreeNodeType> ForwardBreadthFirstNodeEnumerator();
      IEnumerator<ConcreteTreeNodeType> ReverseBreadthFirstNodeEnumerator();
      #endregion

      TreeType ToTree<TreeType>() where TreeType : ITree<NodeValueType, TreeType, ConcreteTreeNodeType>, new();
   }
}
