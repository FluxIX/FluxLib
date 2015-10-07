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
using System.Collections;
using System.Collections.Generic;

namespace FluxLib.Collections.Generic.Tree
{
   public class GeneralTreeNode<NodeValueType> : IGeneralTreeNode<NodeValueType, GeneralTreeNode<NodeValueType>>
   {
      public GeneralTreeNode( NodeValueType value )
      {
         Value = value;
         Children = new List<GeneralTreeNode<NodeValueType>>();
      }

      #region IEquatable<U> Implementation
      /// <remarks>Only the Value is checked for equality to prevent a complete traversal of the tree.</remarks>
      public Boolean Equals( GeneralTreeNode<NodeValueType> other )
      {
         return Value.Equals( other.Value );
      }
      #endregion

      #region ITreeNode<T,U> Implementation
      public NodeValueType Value
      {
         get;
         protected set;
      }

      public Boolean HasChildren
      {
         get
         {
            return ChildrenCount > 0;
         }
      }

      public Boolean HasChild( NodeValueType value )
      {
         return HasChild( new GeneralTreeNode<NodeValueType>( value ) );
      }

      public Boolean HasChild( GeneralTreeNode<NodeValueType> node )
      {
         return Children.Contains( node );
      }

      public Boolean HasParent
      {
         get
         {
            return Parent != null;
         }
      }

      public Boolean IsRoot
      {
         get
         {
            return !HasParent;
         }
      }

      public Boolean IsLeaf
      {
         get
         {
            return !HasChildren;
         }
      }

      public GeneralTreeNode<NodeValueType> Parent
      {
         get;
         protected set;
      }

      public Int32 ChildrenCount
      {
         get
         {
            return Children.Count;
         }
      }

      public Int32 DescendentCount
      {
         get
         {
            Int32 result = 0;

            foreach( GeneralTreeNode<NodeValueType> child in Children )
               result += child.DescendentCount;

            return result;
         }
      }

      public Int32 Depth
      {
         get
         {
            Int32 result = 0;

            GeneralTreeNode<NodeValueType> node = Parent;
            while( !node.IsRoot )
            {
               ++result;
               node = node.Parent;
            }

            return result;
         }
      }

      public void Clear()
      {
         foreach( GeneralTreeNode<NodeValueType> child in Children )
            child.Clear();
      }

      #region Enumeration
      public IEnumerator<NodeValueType> DepthFirstNodeValueEnumerator( EnumerationDirection direction )
      {
         IEnumerator<NodeValueType> result;

         if( direction == EnumerationDirection.Forward )
            result = ForwardDepthFirstNodeValueEnumerator();
         else if( direction == EnumerationDirection.Reverse )
            result = ReverseDepthFirstNodeValueEnumerator();
         else
            throw new NotSupportedException( String.Format( "Enumeration direction '{0}' is not supported.", direction ) );

         return result;
      }

      public IEnumerator<NodeValueType> ForwardDepthFirstNodeValueEnumerator()
      {
         return new TreeValueEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( ForwardDepthFirstNodeEnumerator() );
      }

      public IEnumerator<NodeValueType> ReverseDepthFirstNodeValueEnumerator()
      {
         return new TreeValueEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( ReverseDepthFirstNodeEnumerator() );
      }

      public IEnumerator<NodeValueType> BreadthFirstNodeValueEnumerator( EnumerationDirection direction )
      {
         IEnumerator<NodeValueType> result;

         if( direction == EnumerationDirection.Forward )
            result = ForwardBreadthFirstNodeValueEnumerator();
         else if( direction == EnumerationDirection.Reverse )
            result = ReverseBreadthFirstNodeValueEnumerator();
         else
            throw new NotSupportedException( String.Format( "Enumeration direction '{0}' is not supported.", direction ) );

         return result;
      }

      public IEnumerator<NodeValueType> ForwardBreadthFirstNodeValueEnumerator()
      {
         return new TreeValueEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( ForwardBreadthFirstNodeEnumerator() );
      }

      public IEnumerator<NodeValueType> ReverseBreadthFirstNodeValueEnumerator()
      {
         return new TreeValueEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( ReverseBreadthFirstNodeEnumerator() );
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> DepthFirstNodeEnumerator( EnumerationDirection direction )
      {
         IEnumerator<GeneralTreeNode<NodeValueType>> result;

         if( direction == EnumerationDirection.Forward )
            result = ForwardDepthFirstNodeEnumerator();
         else if( direction == EnumerationDirection.Reverse )
            result = ReverseDepthFirstNodeEnumerator();
         else
            throw new NotSupportedException( String.Format( "Enumeration direction '{0}' is not supported.", direction ) );

         return result;
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> ForwardDepthFirstNodeEnumerator()
      {
         return new DepthFirstTreeNodeEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( this, EnumerationDirection.Forward );
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> ReverseDepthFirstNodeEnumerator()
      {
         return new DepthFirstTreeNodeEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( this, EnumerationDirection.Reverse );
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> BreadthFirstNodeEnumerator( EnumerationDirection direction )
      {
         IEnumerator<GeneralTreeNode<NodeValueType>> result;

         if( direction == EnumerationDirection.Forward )
            result = ForwardBreadthFirstNodeEnumerator();
         else if( direction == EnumerationDirection.Reverse )
            result = ReverseBreadthFirstNodeEnumerator();
         else
            throw new NotSupportedException( String.Format( "Enumeration direction '{0}' is not supported.", direction ) );

         return result;
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> ForwardBreadthFirstNodeEnumerator()
      {
         return new ForwardBreadthFirstTreeNodeEnumerator<NodeValueType, GeneralTreeNode<NodeValueType>>( this );
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> ReverseBreadthFirstNodeEnumerator()
      {
         return new ReverseEnumerator<GeneralTreeNode<NodeValueType>>( ForwardBreadthFirstNodeEnumerator() );
      }
      #endregion

      public TreeType ToTree<TreeType>() where TreeType : ITree<NodeValueType, TreeType, GeneralTreeNode<NodeValueType>>, new()
      {
         TreeType result = new TreeType();

         result.InsertRoot( this );

         return result;
      }
      #endregion

      #region IGeneralTreeNode<T,U> Implementation
      public IList<GeneralTreeNode<NodeValueType>> Siblings
      {
         get
         {
            List<GeneralTreeNode<NodeValueType>> result = new List<GeneralTreeNode<NodeValueType>>();

            if( !IsRoot )
               foreach( GeneralTreeNode<NodeValueType> node in Parent.Children )
                  if( !Object.ReferenceEquals( this, node ) )
                     result.Add( node );

            return result;
         }
      }

      public IList<GeneralTreeNode<NodeValueType>> Children
      {
         get;
         protected set;
      }
      #endregion
   }
}
