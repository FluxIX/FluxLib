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
   public class GeneralTree<NodeValueType> : IGeneralTree<NodeValueType, GeneralTree<NodeValueType>, GeneralTreeNode<NodeValueType>>
   {
      public GeneralTree()
         : this( null )
      {
      }

      public GeneralTree( NodeValueType value )
         : this( new GeneralTreeNode<NodeValueType>( value ) )
      {
      }

      public GeneralTree( GeneralTreeNode<NodeValueType> root )
      {
         Root = root;
      }

      public Boolean Equals( GeneralTree<NodeValueType> other )
      {
         IEnumerator<GeneralTreeNode<NodeValueType>> localEnumerator = ForwardDepthFirstNodeEnumerator();
         IEnumerator<GeneralTreeNode<NodeValueType>> otherEnumerator = ForwardDepthFirstNodeEnumerator();

         Boolean localHasNext = localEnumerator.MoveNext();
         Boolean otherHasNext = otherEnumerator.MoveNext();
         Boolean result = localHasNext == otherHasNext;
         while( result && localHasNext && otherHasNext )
            if( result = localEnumerator.Current.Equals( otherEnumerator.Current ) )
            {
               localHasNext = localEnumerator.MoveNext();
               otherHasNext = otherEnumerator.MoveNext();
               result = localHasNext == otherHasNext;
            }

         return result;
      }

      public GeneralTreeNode<NodeValueType> Root
      {
         get;
         protected set;
      }

      public Int32 Count
      {
         get
         {
            Int32 result = 0;

            if( Root != null )
               result = 1 + Root.DescendentCount;

            return result;
         }
      }

      public Boolean IsEmpty
      {
         get
         {
            return Count == 0;
         }
      }

      #region Manipulation Operations
      public void InsertRoot( NodeValueType value )
      {
         InsertRoot( new GeneralTreeNode<NodeValueType>( value ) );
      }

      public void InsertRoot( GeneralTreeNode<NodeValueType> node )
      {
         if( node != null )
         {
            if( !IsEmpty )
               node.Children.Add( Root );

            Root = node;
         }
         else
            throw new ArgumentNullException( "node", "Cannot insert a null root." );
      }

      public void InsertBefore( GeneralTreeNode<NodeValueType> reference, NodeValueType value )
      {
         InsertBefore( reference, new GeneralTreeNode<NodeValueType>( value ) );
      }

      public void InsertBefore( GeneralTreeNode<NodeValueType> reference, GeneralTreeNode<NodeValueType> node )
      {
         if( reference == null )
            throw new ArgumentNullException( "reference", "Cannot insert before a null reference node." );
         else if( node == null )
            throw new ArgumentNullException( "node", "Cannot insert a null node." );
         else
         {
            if( reference.IsRoot )
               InsertRoot( node );
            else
            {
               reference.Parent.Children.Add( node );
               reference.Parent.Children.Remove( reference );
               node.Children.Add( reference );
            }
         }
      }

      public void InsertAfter( GeneralTreeNode<NodeValueType> reference, NodeValueType value )
      {
         InsertAfter( reference, new GeneralTreeNode<NodeValueType>( value ) );
      }

      public void InsertAfter( GeneralTreeNode<NodeValueType> reference, GeneralTreeNode<NodeValueType> node )
      {
         if( reference == null )
            throw new ArgumentNullException( "reference", "Cannot insert before a null reference node." );
         else if( node == null )
            throw new ArgumentNullException( "node", "Cannot insert a null node." );
         else
            reference.Children.Add( node );
      }

      public void Remove( NodeValueType value, Boolean orphanChildren = false )
      {
         Remove( new GeneralTreeNode<NodeValueType>( value ) );
      }

      public void Remove( GeneralTreeNode<NodeValueType> node, Boolean orphanChildren = false )
      {
         if( node == null )
            throw new ArgumentNullException( "node", "Cannot remove a null node." );
         else if( !IsEmpty )
         {
            GeneralTreeNode<NodeValueType> presentNode = Search( node );
            if( presentNode != null )
            {
               presentNode.Parent.Children.Remove( presentNode );

               if( orphanChildren )
                  node.Clear();
            }
         }
      }

      public void Clear()
      {
         Root.Clear();
         Root = null;
      }
      #endregion

      public Boolean Contains( NodeValueType value )
      {
         return Search( value ) != null;
      }

      public Boolean Contains( NodeValueType value, IEnumerator<GeneralTreeNode<NodeValueType>> enumerator )
      {
         return Search( value, enumerator ) != null;
      }

      #region Searching
      public GeneralTreeNode<NodeValueType> Search( NodeValueType value )
      {
         return Search( value, DefaultNodeEnumerator );
      }

      protected GeneralTreeNode<NodeValueType> Search( GeneralTreeNode<NodeValueType> node )
      {
         return Search( node.Value );
      }

      public GeneralTreeNode<NodeValueType> Search( NodeValueType value, IEnumerator<GeneralTreeNode<NodeValueType>> enumerator )
      {
         if( enumerator == null )
            throw new ArgumentNullException( "enumerator", "Node enumerator cannot be null." );
         else
         {
            enumerator.Reset();

            GeneralTreeNode<NodeValueType> result = null;

            while( result == null && enumerator.MoveNext() )
               if( enumerator.Current == null && value == null || enumerator.Current.Equals( value ) )
                  result = enumerator.Current;

            return result;
         }
      }
      #endregion

      #region Enumerators
      IEnumerator IEnumerable.GetEnumerator()
      {
         return DefaultNodeEnumerator;
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> GetEnumerator()
      {
         return DefaultNodeEnumerator;
      }

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
         IEnumerator<NodeValueType> result;

         if( !IsEmpty )
            result = Root.ForwardDepthFirstNodeValueEnumerator();
         else
            result = new ListEnumerator<NodeValueType>();

         return result;
      }

      public IEnumerator<NodeValueType> ReverseDepthFirstNodeValueEnumerator()
      {
         IEnumerator<NodeValueType> result;

         if( !IsEmpty )
            result = Root.ReverseDepthFirstNodeValueEnumerator();
         else
            result = new ListEnumerator<NodeValueType>();

         return result;
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
         IEnumerator<NodeValueType> result;

         if( !IsEmpty )
            result = Root.ForwardBreadthFirstNodeValueEnumerator();
         else
            result = new ListEnumerator<NodeValueType>();

         return result;
      }

      public IEnumerator<NodeValueType> ReverseBreadthFirstNodeValueEnumerator()
      {
         IEnumerator<NodeValueType> result;

         if( !IsEmpty )
            result = Root.ReverseBreadthFirstNodeValueEnumerator();
         else
            result = new ListEnumerator<NodeValueType>();

         return result;
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
         IEnumerator<GeneralTreeNode<NodeValueType>> result;

         if( !IsEmpty )
            result = Root.ForwardDepthFirstNodeEnumerator();
         else
            result = new ListEnumerator<GeneralTreeNode<NodeValueType>>();

         return result;
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> ReverseDepthFirstNodeEnumerator()
      {
         IEnumerator<GeneralTreeNode<NodeValueType>> result;

         if( !IsEmpty )
            result = Root.ReverseDepthFirstNodeEnumerator();
         else
            result = new ListEnumerator<GeneralTreeNode<NodeValueType>>();

         return result;
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
         IEnumerator<GeneralTreeNode<NodeValueType>> result;

         if( !IsEmpty )
            result = Root.ForwardBreadthFirstNodeEnumerator();
         else
            result = new ListEnumerator<GeneralTreeNode<NodeValueType>>();

         return result;
      }

      public IEnumerator<GeneralTreeNode<NodeValueType>> ReverseBreadthFirstNodeEnumerator()
      {
         IEnumerator<GeneralTreeNode<NodeValueType>> result;

         if( !IsEmpty )
            result = Root.ReverseBreadthFirstNodeEnumerator();
         else
            result = new ListEnumerator<GeneralTreeNode<NodeValueType>>();

         return result;
      }

      protected IEnumerator<NodeValueType> defaultValueEnumerator;

      public IEnumerator<NodeValueType> DefaultValueEnumerator
      {
         get
         {
            return defaultValueEnumerator;
         }

         set
         {
            if( value != null )
               defaultValueEnumerator = value;
            else
               throw new ArgumentNullException( "value", "The default value enumerator cannot be null." );
         }
      }

      protected IEnumerator<GeneralTreeNode<NodeValueType>> defaultNodeEnumerator;

      public IEnumerator<GeneralTreeNode<NodeValueType>> DefaultNodeEnumerator
      {
         get
         {
            return defaultNodeEnumerator;
         }

         set
         {
            if( value != null )
               defaultNodeEnumerator = value;
            else
               throw new ArgumentNullException( "value", "The default node enumerator cannot be null." );
         }
      }
      #endregion
   }
}
