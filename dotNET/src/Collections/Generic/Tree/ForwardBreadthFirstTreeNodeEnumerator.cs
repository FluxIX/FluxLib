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
   public class ForwardBreadthFirstTreeNodeEnumerator<NodeValueType, TreeNodeType> : IEnumerator<TreeNodeType>, IDisposable where TreeNodeType : IGeneralTreeNode<NodeValueType, TreeNodeType>
   {
      protected readonly static TreeNodeType InvalidItem = default( TreeNodeType );

      public ForwardBreadthFirstTreeNodeEnumerator()
         : this( InvalidItem )
      {
      }

      public ForwardBreadthFirstTreeNodeEnumerator( TreeNodeType root )
      {
         Completed = false;

         CurrentItem = InvalidItem;
         Root = root;

         ProgressQueue = new Queue<TreeNodeType>();
      }

      protected Queue<TreeNodeType> ProgressQueue
      {
         get;
         set;
      }

      protected TreeNodeType Root
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

         ProgressQueue.Clear();
         ProgressQueue.Enqueue( Root );
      }

      public virtual void Dispose()
      {
         Root = CurrentItem = InvalidItem;
         ProgressQueue.Clear();
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
            if( ProgressQueue.Count > 0 )
            {
               TreeNodeType node = ProgressQueue.Dequeue();

               foreach( TreeNodeType child in node.Children )
                  ProgressQueue.Enqueue( child );

               CurrentItem = node;
            }
            else // Enumeration is complete.
            {
               CurrentItem = InvalidItem;
               Completed = true;
            }

            result = !CurrentItem.Equals( InvalidItem );
         }
         else
            result = false;

         return result;
      }
   }
}
