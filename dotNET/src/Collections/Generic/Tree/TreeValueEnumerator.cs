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
   public class TreeValueEnumerator<NodeValueType, TreeNodeType> : IEnumerator<NodeValueType>, IDisposable where TreeNodeType : ITreeNode<NodeValueType, TreeNodeType>
   {
      public TreeValueEnumerator( IEnumerator<TreeNodeType> nodeEnumerator, Boolean resetEnumerator = true )
      {
         NodeEnumerator = nodeEnumerator;
         NodeEnumerator.Reset();
      }

      protected IEnumerator<TreeNodeType> nodeEnumerator;

      protected IEnumerator<TreeNodeType> NodeEnumerator
      {
         get
         {
            return nodeEnumerator;
         }

         set
         {
            if( value != null )
               nodeEnumerator = value;
            else
               throw new ArgumentNullException( "value", "Node enumerator cannot be null." );
         }
      }

      public virtual void Reset()
      {
         NodeEnumerator.Reset();
      }

      public virtual void Dispose()
      {
         NodeEnumerator.Dispose();
      }

      public virtual NodeValueType Current
      {
         get
         {
            return NodeEnumerator.Current.Value;
         }
      }

      Object IEnumerator.Current
      {
         get
         {
            return NodeEnumerator.Current.Value;
         }
      }

      public virtual Boolean MoveNext()
      {
         return NodeEnumerator.MoveNext();
      }
   }
}
