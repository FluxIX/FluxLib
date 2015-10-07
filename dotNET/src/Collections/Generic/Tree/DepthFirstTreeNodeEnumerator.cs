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
   public class DepthFirstTreeNodeEnumerator<NodeValueType, TreeNodeType> : IEnumerator<TreeNodeType>, IDisposable where TreeNodeType : IGeneralTreeNode<NodeValueType, TreeNodeType>
   {
      protected readonly static TreeNodeType InvalidItem = default( TreeNodeType );

      public DepthFirstTreeNodeEnumerator( EnumerationDirection direction = EnumerationDirection.Forward )
         : this( InvalidItem, direction )
      {
      }

      public DepthFirstTreeNodeEnumerator( TreeNodeType root, EnumerationDirection direction = EnumerationDirection.Forward )
      {
         Completed = false;
         Direction = direction;

         CurrentItem = InvalidItem;
         Root = root;

         ProgressStack = new Stack<IList<TreeNodeType>>();
      }

      protected EnumerationDirection Direction
      {
         get;
         set;
      }

      protected TreeNodeType Root
      {
         get;
         set;
      }

      protected Stack<IList<TreeNodeType>> ProgressStack
      {
         get;
         set;
      }

      protected TreeNodeType CurrentItem
      {
         get;
         set;
      }

      public virtual Boolean Completed
      {
         get;
         protected set;
      }

      public virtual void Reset()
      {
         CurrentItem = InvalidItem;
         Completed = false;
      }

      public virtual void Dispose()
      {
         Root = CurrentItem = InvalidItem;
      }

      public virtual TreeNodeType Current
      {
         get
         {
            return CurrentItem;
         }
      }

      Object IEnumerator.Current
      {
         get
         {
            return CurrentItem;
         }
      }

      public virtual Boolean MoveNext()
      {
         Boolean result;

         if( !Completed )
         {
            if( CurrentItem.Equals( InvalidItem ) && !Root.Equals( InvalidItem ) ) // Need to initialize the enumeration.
            {
               ProgressStack.Clear();

               ITreeNode<NodeValueType, TreeNodeType> node = Root;
               while( node.HasChildren )
               {
                  ProgressStack.Push( new List<TreeNodeType>( node.Children ) );

                  Int32 index = GetTargetIndex( node.Children );
                  node = node.Children[ index ];
               }
            }

            if( ProgressStack.Count > 0 )
            {
               IList<TreeNodeType> nodes = ProgressStack.Peek();

               while( nodes.Count == 0 && ProgressStack.Count > 0 )
               {
                  ProgressStack.Pop();
                  nodes = ProgressStack.Peek();
               }

               if( nodes.Count > 0 )
               {
                  Int32 index = GetTargetIndex( nodes );
                  CurrentItem = nodes[ index ];

                  nodes.RemoveAt( index );
               }
               else // Enumeration is complete.
               {
                  CurrentItem = InvalidItem;
                  Completed = true;
               }
            }

            result = !CurrentItem.Equals( InvalidItem );
         }
         else
            result = false;

         return result;
      }

      protected Int32 GetTargetIndex( IList<TreeNodeType> nodes )
      {
         Int32 result;

         if( Direction == EnumerationDirection.Forward )
            result = 0;
         else if( Direction == EnumerationDirection.Reverse )
            result = nodes.Count - 1;
         else
            throw new NotSupportedException( String.Format( "'{0}' is not a supported direction.", Direction ) );

         return result;
      }
   }
}
